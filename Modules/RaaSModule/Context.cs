using ELogging;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils;
using Eng.Chlaot.Libs.AirportsLib;
using Eng.Chlaot.Modules.RaaSModule.Model;
using ESystem;
using ESystem.Miscelaneous;
using EXmlLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ESystem.Functions.TryCatch;
using System.Xml.Linq;
using ChlaotModuleBase.ModuleUtils.SimConWrapping;
using ESimConnect;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimObjects;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimConWrapping;
using System.Timers;

namespace Eng.Chlaot.Modules.RaaSModule
{
  internal class Context : NotifyPropertyChanged
  {
    private readonly Logger logger;
    private readonly Action<bool> updateReadyFlag;
    private SimDataStruct __simDataUnsafe;
    private readonly object __simDataUnsafeLock = new();
    private readonly System.Timers.Timer timer;
    private readonly ESimConnect.ESimConnect simConnect;
    private SimDataStruct simData
    {
      get
      {
        lock (this.__simDataUnsafeLock)
        {
          return this.__simDataUnsafe;
        }
      }
      set
      {
        lock (this.__simDataUnsafeLock)
        {
          this.__simDataUnsafe = value;
        }
      }
    }

    public MetaInfo MetaInfo
    {
      get { return base.GetProperty<MetaInfo>(nameof(MetaInfo))!; }
      set { base.UpdateProperty(nameof(MetaInfo), value); }
    }

    public List<Airport> Airports
    {
      get { return base.GetProperty<List<Airport>>(nameof(Airports))!; }
      set { base.UpdateProperty(nameof(Airports), value); }
    }

    public Raas RaaS
    {
      get { return base.GetProperty<Raas>(nameof(RaaS))!; }
      set { base.UpdateProperty(nameof(RaaS), value); }
    }

    public Context(Action<bool> updateReadyFlag)
    {
      this.logger = Logger.Create(this);
      this.updateReadyFlag = updateReadyFlag;
      this.timer = new(1000)
      {
        AutoReset = true,
        Enabled = false,
      };
      this.timer.Elapsed += timer_Elapsed;

      this.simConnect = new();
      simConnect.Connected += simConnect_Connected;
      simConnect.ThrowsException += simConnect_ThrowsException;
      simConnect.Disconnected += simConnect_Disconnected;
      simConnect.DataReceived += simConnect_DataReceived;
    }

    internal void LoadAirportsFile(string recentXmlFile)
    {
      this.Airports = XmlLoader.Load(recentXmlFile);
      this.CheckReadyStatus();
    }

    internal void LoadRaasFile(string xmlFile)
    {
      try
      {
        logger.Invoke(LogLevel.INFO, $"Checking file '{xmlFile}'");
        try
        {
          XmlUtils.ValidateXmlAgainstXsd(xmlFile, new string[] {
            @".\xmls\xsds\Global.xsd",
            @".\xmls\xsds\RaasSchema.xsd"}, out List<string> errors);
          if (errors.Any())
            throw new ApplicationException("XML does not match XSD: " + string.Join("; ", errors.Take(5)));
        }
        catch (Exception ex)
        {
          throw new ApplicationException($"Failed to validate XML file against XSD. Error: " + ex.Message, ex);
        }

        logger.Invoke(LogLevel.INFO, $"Loading file '{xmlFile}'");
        XDocument doc = Try(() => XDocument.Load(xmlFile, LoadOptions.SetLineInfo),
          ex => throw new ApplicationException($"Unable to load xml file '{xmlFile}'.", ex));

        MetaInfo tmpMeta = MetaInfo.Deserialize(doc);
        Raas raas = Try(() => RaasXmlLoader.Load(doc),
          ex => throw new ApplicationException("Unable to read/deserialize copilot-set from '{xmlFile}'. Invalid file content?", ex));

        logger.Invoke(LogLevel.INFO, $"Checking sanity");
        Try(
          () => raas.CheckSanity(),
          ex => throw new ApplicationException("Error loading failures.", ex));

        this.MetaInfo = tmpMeta;
        this.RaaS = raas;

        this.CheckReadyStatus();
        //this.LastLoadedFile = xmlFile;
        logger.Invoke(LogLevel.INFO, $"RaaS set file '{xmlFile}' successfully loaded.");
      }
      catch (Exception ex)
      {
        this.updateReadyFlag(false);
        logger.Invoke(LogLevel.ERROR, $"Failed to load failure set from '{xmlFile}'." + ex.GetFullMessage());
      }
    }

    private void CheckReadyStatus()
    {
      this.updateReadyFlag(this.RaaS != null && this.Airports.Count > 0);
    }

    public void Start()
    {
      this.timer.Enabled = true;
    }

    private void TryConnect()
    {
      try
      {
        logger.Log(LogLevel.DEBUG, "Opening simConnect");
        simConnect.Open();
      }
      catch (Exception ex)
      {
        logger.Log(LogLevel.ERROR, "Failed to open simConnect: " + ex.ToString());
        return;
      }

      logger.Log(LogLevel.DEBUG, "Registering simConnect type");
      simConnect.Structs.Register<SimDataStruct>();

      logger.Log(LogLevel.DEBUG, "Registering simConnect repeated-requests");
      simConnect.Structs.RequestRepeatedly<SimDataStruct>(SimConnectPeriod.SECOND, sendOnlyOnChange: true);
    }

    private void timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
      if (!simConnect.IsOpened)
      {
        TryConnect();
      }
      else
      {
        // TODO implement tests here
      }
    }

    private void simConnect_DataReceived(
      ESimConnect.ESimConnect eSimCon,
      ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
    {
      var data = (SimDataStruct)e.Data;
      this.simData = data;
    }

    private void simConnect_ThrowsException(ESimConnect.ESimConnect eSimCon, SimConnectException ex)
    {
      this.logger.Log(LogLevel.ERROR, "SimConnect exception: " + ex.ToString());
    }

    private void simConnect_Disconnected(ESimConnect.ESimConnect eSimCon)
    {
      this.logger.Log(LogLevel.INFO, "Disconnected from SimConnect");
    }

    private void simConnect_Connected(ESimConnect.ESimConnect eSimCon)
    {
      this.logger.Log(LogLevel.INFO, "Connected to SimConnect");
    }

    public void Stop()
    {

    }
  }
}
