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
using ELogging;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateCheckingSimConnection
{
  public class SimConManager : LogIdAble, ISimConManager
  {
    public event ISimConManager.SimSecondElapsedDelegate? SimSecondElapsed;

    private readonly NewLogHandler logHandler;
    private ESimConnect.ESimConnect? simCon = null;
    private bool isStarted = false;

    public SimData SimData { get; } = new();
    public SimConManager()
    {
      this.logHandler = Logger.RegisterSender(this);
    }

    public void Close()
    {
      if (simCon != null)
      {
        this.simCon.Close();
        this.simCon.Dispose();
        this.simCon = null;
      }
    }

    public void Open()
    {
      if (simCon != null) return;

      Log(LogLevel.INFO, "Connecting to FS2020");
      ESimConnect.ESimConnect tmp = new();
      try
      {
        Log(LogLevel.VERBOSE, "Connecting simconnect handlers");
        tmp.DataReceived += Simcon_DataReceived;
        tmp.EventInvoked += Simcon_EventInvoked;
        Log(LogLevel.VERBOSE, "Opening simconnect");
        tmp.Open();
        Log(LogLevel.VERBOSE, "Simconnect ready");
        this.simCon = tmp;
      }
      catch (Exception ex)
      {
        tmp.Close();
        throw new Exception("Failed to open connection to FS2020", ex);
      }
    }

    public void Start()
    {
      if (simCon == null) throw new ApplicationException("SimConManager not opened().");

      if (isStarted) return;
      Log(LogLevel.VERBOSE, "Simconnect - registering structs");
      simCon.RegisterType<CommonDataStruct>();
      simCon.RegisterType<RareDataStruct>();

      Log(LogLevel.VERBOSE, "Simconnect - requesting structs");
      simCon.RequestDataRepeatedly<CommonDataStruct>(null, SIMCONNECT_PERIOD.SECOND, sendOnlyOnChange: true);
      simCon.RequestDataRepeatedly<RareDataStruct>(null, SIMCONNECT_PERIOD.SECOND, sendOnlyOnChange: true);

      Log(LogLevel.VERBOSE, "Simconnect - attaching to events");
      simCon.RegisterSystemEvent(SimEvents.System.Pause);
      simCon.RegisterSystemEvent(SimEvents.System._1sec);

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
      this.logHandler.Invoke(level, message);
    }
  }
}
