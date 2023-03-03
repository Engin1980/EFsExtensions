using Eng.Chlaot.ChlaotModuleBase;
using ESimConnect;
using System;
using Microsoft.FlightSimulator.SimConnect;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;

namespace ChlaotModuleBase.ModuleUtils.StateCheckingSimConnection
{
  public class SimConManager
  {
    public delegate void SimSecondElapsedDelegate();
    public event SimSecondElapsedDelegate? SimSecondElapsed;

    private readonly LogHandler logHandler;
    private ESimConnect.ESimConnect? _SimCon = null;
    private bool isStarted = false;
    public ESimConnect.ESimConnect SimCon => this._SimCon ?? throw new ApplicationException("SimConManager not opened().");

    public SimData SimData { get; } = new();
    public SimConManager(LogHandler logHandler, string? logFileNameIfEnabled = null)
    {
      this.logHandler = logHandler ?? throw new ArgumentNullException(nameof(logHandler));
      if (logFileNameIfEnabled is not null)
        ESimConnect.ESimConnect.SetLogHandler(s => System.IO.File.AppendAllText(logFileNameIfEnabled, s));
      else
        ESimConnect.ESimConnect.SetLogHandler(s => { });
    }

    public void Close()
    {
      if (_SimCon != null)
      {
        this._SimCon.Close();
        this._SimCon.Dispose();
        this._SimCon = null;
      }
    }

    public void Open()
    {
      if (_SimCon != null) return;

      Log(LogLevel.INFO, "Connecting to FS2020");
      ESimConnect.ESimConnect tmp = new ESimConnect.ESimConnect();
      try
      {
        Log(LogLevel.VERBOSE, "Connecting simconnect handlers");
        tmp.DataReceived += Simcon_DataReceived;
        tmp.EventInvoked += Simcon_EventInvoked;
        Log(LogLevel.VERBOSE, "Opening simconnect");
        tmp.Open();
        Log(LogLevel.VERBOSE, "Simconnect ready");
        this._SimCon = tmp;
      }
      catch (Exception ex)
      {
        tmp.Close();
        throw new Exception("Failed to open connection to FS2020", ex);
      }
    }

    public void Start()
    {
      if (isStarted) return;
      Log(LogLevel.VERBOSE, "Simconnect - registering structs");
      SimCon.RegisterType<CommonDataStruct>();
      SimCon.RegisterType<RareDataStruct>();

      Log(LogLevel.VERBOSE, "Simconnect - requesting structs");
      SimCon.RequestDataRepeatedly<CommonDataStruct>(null, SIMCONNECT_PERIOD.SECOND, sendOnlyOnChange: true);
      SimCon.RequestDataRepeatedly<RareDataStruct>(null, SIMCONNECT_PERIOD.SECOND, sendOnlyOnChange: true);

      Log(LogLevel.VERBOSE, "Simconnect - attaching to events");
      SimCon.RegisterSystemEvent(SimEvents.System.Pause);
      SimCon.RegisterSystemEvent(SimEvents.System._1sec);

      Log(LogLevel.VERBOSE, "Simconnect connection ready");

      isStarted = true;
    }

    private void Simcon_DataReceived(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
    {
      Log(LogLevel.INFO, $"FS2020 sim data '{e.RequestId}' of type '{e.Type.Name}' obtained");
      if (e.Type == typeof(CommonDataStruct))
      {
        CommonDataStruct s = (CommonDataStruct)e.Data;
        this.SimData.Update(s);
      }
      else if (e.Type == typeof(RareDataStruct))
      {
        RareDataStruct s = (RareDataStruct)e.Data;
        this.SimData.Update(s);
      }
    }

    private void Simcon_EventInvoked(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectEventInvokedEventArgs e)
    {
      Log(LogLevel.VERBOSE, "Simcon event");
      if (e.Event == SimEvents.System.Pause)
      {
        bool isPaused = e.Value != 0;
        this.SimData.IsSimPaused = isPaused;
      }
      else if (e.Event == SimEvents.System._1sec)
      {
        this.SimSecondElapsed?.Invoke();
      }
    }

    private void Log(LogLevel level, string message)
    {
      this.logHandler?.Invoke(level, "[SimConManager] :: " + message);
    }
  }
}
