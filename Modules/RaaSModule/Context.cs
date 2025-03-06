using ELogging;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils;
using Eng.EFsExtensions.Libs.AirportsLib;
using Eng.EFsExtensions.Modules.RaaSModule.Model;
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
using ESimConnect;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimConWrapping;
using System.Timers;
using System.Reflection.Metadata.Ecma335;
using System.Speech.Synthesis;
using ESystem.Exceptions;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.AudioPlaying;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Shell;
using System.Collections.ObjectModel;
using Eng.EFsExtensions.Modules.RaaSModule.ContextHandlers;

namespace Eng.EFsExtensions.Modules.RaaSModule
{
  internal class Context : NotifyPropertyChanged
  {

    #region Public Classes + Structs + Interfaces

    public record NearestAirport(Airport Airport, double Distance, List<RunwayWithOrthoDistance> RunwayOrthos);
    public record NearestRunways(Runway Runway, double OrthoDistance);
    public record RunwayWithOrthoDistance(Runway Runway, double OrthoDistance);
    public record LandingRaasData(Airport Airport, Runway Runway, RunwayThreshold Threshold, double OrthoDistance, double ThresholdDistance, Heading Bearing);
    public record HoldingPointData(Airport Airport, Runway Runway, double OrthoDistance);
    public record LineUpData(Airport Airport, Runway Runway, RunwayThreshold Threshold, Heading Bearing, double Distance, double DeltaHeading);

    public class RuntimeDataBox : NotifyPropertyChanged
    {
      #region Public Properties

      public List<LineUpData> LineUp
      {
        get { return base.GetProperty<List<LineUpData>>(nameof(LineUp))!; }
        set { base.UpdateProperty(nameof(LineUp), value); }
      }

      public string LineUpStatus
      {
        get { return base.GetProperty<string>(nameof(LineUpStatus))!; }
        set { base.UpdateProperty(nameof(LineUpStatus), value); }
      }

      public List<HoldingPointData> HoldingPoint
      {
        get { return base.GetProperty<List<HoldingPointData>>(nameof(HoldingPoint))!; }
        set { base.UpdateProperty(nameof(HoldingPoint), value); }
      }

      public string HoldingPointStatus
      {
        get { return base.GetProperty<string>(nameof(HoldingPointStatus))!; }
        set { base.UpdateProperty(nameof(HoldingPointStatus), value); }
      }

      public List<LandingRaasData> Landing
      {
        get { return base.GetProperty<List<LandingRaasData>>(nameof(Landing))!; }
        set { base.UpdateProperty(nameof(Landing), value); }
      }

      public string LandingStatus
      {
        get { return base.GetProperty<string>(nameof(LandingStatus))!; }
        set { base.UpdateProperty(nameof(LandingStatus), value); }
      }

      public NearestAirport? NearestAirport
      {
        get { return base.GetProperty<NearestAirport?>(nameof(NearestAirport))!; }
        set { base.UpdateProperty(nameof(NearestAirport), value); }
      }

      public List<NearestRunways> NearestRunways
      {
        get { return base.GetProperty<List<NearestRunways>>(nameof(NearestRunways))!; }
        set { base.UpdateProperty(nameof(NearestRunways), value); }
      }


      public List<string> DistanceStates
      {
        get { return base.GetProperty<List<string>>(nameof(DistanceStates))!; }
        set { base.UpdateProperty(nameof(DistanceStates), value); }
      }

      #endregion Public Properties

      public RuntimeDataBox()
      {
        this.DistanceStates = new List<string>();
      }
    }

    #endregion Public Classes + Structs + Interfaces

    #region Private Fields

    private const int MAX_HEIGHT_TO_DO_EVALUATIONS_IN_FT = 2000;
    private const int MAX_DISTANCE_TO_DO_EVALUATIONS_IN_M = 8_000;
    private readonly object __simDataUnsafeLock = new();
    private readonly Logger logger;
    private readonly ESimConnect.ESimConnect simConnect;
    private readonly System.Timers.Timer timer;
    private readonly Action<bool> updateReadyFlag;
    private bool isBusy = false;
    private HoldingPointContextHandler holdingPointContextHandler;
    private LineUpContextHandler lineUpContextHandler;
    private LandingContextHandler landingContextHandler;
    private RemainingDistanceContextHandler remainingDistanceContextHandler;

    #endregion Private Fields

    #region Public Properties

    public List<Airport> Airports
    {
      get { return base.GetProperty<List<Airport>>(nameof(Airports))!; }
      set { base.UpdateProperty(nameof(Airports), value); }
    }

    public MetaInfo MetaInfo
    {
      get { return base.GetProperty<MetaInfo>(nameof(MetaInfo))!; }
      set { base.UpdateProperty(nameof(MetaInfo), value); }
    }

    public Raas RaaS
    {
      get { return base.GetProperty<Raas>(nameof(RaaS))!; }
      set { base.UpdateProperty(nameof(RaaS), value); }
    }

    public RuntimeDataBox RuntimeData { get; set; } = new();
    public Settings Settings { get; set; } = new Settings();

    public SimDataStruct SimData
    {
      get
      {
        SimDataStruct ret;
        lock (this.__simDataUnsafeLock)
        {
          ret = GetProperty<SimDataStruct>(nameof(SimData))!;
        }
        return ret;
      }
      set
      {
        lock (this.__simDataUnsafeLock)
        {
          UpdateProperty<SimDataStruct>(nameof(SimData), value);
        }
      }
    }

    #endregion Public Properties

    #region Public Constructors

    public Context(Logger logger, Action<bool> updateReadyFlag)
    {     
      this.logger = logger;
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

    #endregion Public Constructors

    #region Public Methods

    public void Start()
    {
      var args = new ContextHandlerArgs(this.logger, this.RuntimeData, this.RaaS, () => SimData, this.Settings);
      this.landingContextHandler = new LandingContextHandler(args);
      this.holdingPointContextHandler = new HoldingPointContextHandler(args);
      this.lineUpContextHandler = new LineUpContextHandler(args);
      this.remainingDistanceContextHandler = new RemainingDistanceContextHandler(args);

      this.timer.Enabled = true;
    }

    public void Stop()
    {

    }

    #endregion Public Methods

    #region Internal Methods

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

    #endregion Internal Methods

    #region Private Methods

    private void EvaluateNearestAirport()
    {
      Airport? tmpA = null;
      double? tmpD = null;
      if (this.RuntimeData.NearestAirport != null)
      {
        tmpD = GpsCalculator.GetDistance(
          this.RuntimeData.NearestAirport.Airport.Coordinate.Latitude,
          this.RuntimeData.NearestAirport.Airport.Coordinate.Longitude,
          SimData.latitude, SimData.longitude);
        if (tmpD.Value < this.RuntimeData.NearestAirport.Distance)
          tmpA = this.RuntimeData.NearestAirport.Airport;
      }
      if (tmpA == null)
      {
        var closeAirports = Airports
          .Where(q => q.Coordinate.Latitude > SimData.latitude - 1)
          .Where(q => q.Coordinate.Latitude < SimData.latitude + 1)
          .Where(q => q.Coordinate.Longitude > SimData.longitude - 1)
          .Where(q => q.Coordinate.Longitude < SimData.longitude + 1);

        if (!closeAirports.Any())
        {
          closeAirports = Airports.Where(q => q.Coordinate.Latitude > SimData.latitude - 5)
            .Where(q => q.Coordinate.Latitude < SimData.latitude + 5)
            .Where(q => q.Coordinate.Longitude > SimData.longitude - 5)
            .Where(q => q.Coordinate.Longitude < SimData.longitude + 5);

          if (!closeAirports.Any())
          {
             closeAirports = Airports.Where(q => q.Coordinate.Latitude > SimData.latitude - 20)
              .Where(q => q.Coordinate.Latitude < SimData.latitude + 20)
              .Where(q => q.Coordinate.Longitude > SimData.longitude - 20)
              .Where(q => q.Coordinate.Longitude < SimData.longitude + 20);

            if (!closeAirports.Any())
            {
              closeAirports = Airports;
            }
          }
        }

        var tmpAD = closeAirports
          .Select(q => new
          {
            Airport = q,
            Distance = GpsCalculator.GetDistance(q.Coordinate.Latitude, q.Coordinate.Longitude, SimData.latitude, SimData.longitude)
          })
          .MinBy(q => q.Distance)
          ?? throw new UnexpectedNullException();
        tmpA = tmpAD.Airport;
        tmpD = tmpAD.Distance;
      }

      Debug.Assert(tmpA != null && tmpD != null);
      var rwys = tmpA.Runways
        .Select(q => new RunwayWithOrthoDistance(q, GpsCalculator.GetDistanceFromLine(
            q.Thresholds[0].Coordinate.Latitude,
            q.Thresholds[0].Coordinate.Longitude,
            q.Thresholds[1].Coordinate.Latitude,
            q.Thresholds[1].Coordinate.Longitude,
            SimData.latitude, SimData.longitude)))
        .OrderBy(q => q.OrthoDistance)
        .ToList();
      RuntimeData.NearestAirport = new NearestAirport(tmpA, tmpD.Value, rwys);
    }

    private void EvaluateNearestRunways()
    {
      RuntimeData.NearestRunways = RuntimeData.NearestAirport!.Airport.Runways
        .Select(q => new NearestRunways(
          q,
          GpsCalculator.GetDistanceFromLine(
            q.Thresholds[0].Coordinate.Latitude,
            q.Thresholds[0].Coordinate.Longitude,
            q.Thresholds[1].Coordinate.Latitude,
            q.Thresholds[1].Coordinate.Longitude,
            SimData.latitude, SimData.longitude)))
        .OrderBy(q => q.OrthoDistance)
        .ToList();
    }


    private void EvaluateRaas()
    {
      if (SimData.height > MAX_HEIGHT_TO_DO_EVALUATIONS_IN_FT)
        return; // too high, do nothing

      EvaluateNearestAirport();
      if (RuntimeData.NearestAirport == null || 
        RuntimeData.NearestAirport.Distance > MAX_DISTANCE_TO_DO_EVALUATIONS_IN_M)
        return; // too far, do nothing

      EvaluateNearestRunways();
      holdingPointContextHandler.Handle();
      lineUpContextHandler.Handle();
      landingContextHandler.Handle();
      remainingDistanceContextHandler.Handle();
    }
    private void CheckReadyStatus()
    {
      this.updateReadyFlag(this.RaaS != null && this.Airports.Count > 0);
    }

    private void simConnect_Connected(ESimConnect.ESimConnect eSimCon)
    {
      this.logger.Log(LogLevel.INFO, "Connected to SimConnect");
    }

    private void simConnect_DataReceived(
      ESimConnect.ESimConnect eSimCon,
      ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
    {
      var data = (SimDataStruct)e.Data;
      this.SimData = data;
      this.logger.Log(LogLevel.DEBUG, "Received data from SimConnect");
    }

    private void simConnect_Disconnected(ESimConnect.ESimConnect eSimCon)
    {
      this.logger.Log(LogLevel.INFO, "Disconnected from SimConnect");
    }

    private void simConnect_ThrowsException(ESimConnect.ESimConnect eSimCon, SimConnectException ex)
    {
      this.logger.Log(LogLevel.ERROR, "SimConnect exception: " + ex.ToString());
    }

    private void timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
      if (isBusy) return;
      isBusy = true;
      if (!simConnect.IsOpened)
      {
        TryConnect();
      }
      else
      {
        try
        {
          EvaluateRaas();
        }
        catch (Exception ex)
        {
          logger.Log(LogLevel.ERROR, "Error in EvaluateRaas: " + ex.ToString());
        }
      }
      isBusy = false;
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

    #endregion Private Methods

  }
}
