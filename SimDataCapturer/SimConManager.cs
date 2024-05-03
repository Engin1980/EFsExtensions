using ESimConnect;
using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimDataCapturer
{
  internal class SimConManager
  {
    private readonly ESimConnect.ESimConnect simCon;

    public delegate void OnDataDelegate(MockPlaneData data);
    public delegate void OnSecondElapsedDelegate();
    public event OnDataDelegate? OnData;
    public event OnSecondElapsedDelegate? OnSecondElapsed;
    private bool isPaused = false;
    private bool isRunning = false;

    public SimConManager()
    {
      simCon = new();
      simCon.ThrowsException += SimCon_ThrowsException;
      simCon.Disconnected += SimCon_Disconnected;
      simCon.Connected += SimCon_Connected;
      simCon.DataReceived += SimCon_DataReceived;
      simCon.SystemEventInvoked += SimCon_SystemEventInvoked;
    }

    public void Start()
    {
      isRunning = true;
    }

    public bool IsRunning { get => this.isRunning; }

    private void SimCon_SystemEventInvoked(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectSystemEventInvokedEventArgs e)
    {
      if (e.Event == ESimConnect.Enumerations.SimSystemEvents.System.Pause)
        isPaused = e.Value != 0;
      else if (e.Event == ESimConnect.Enumerations.SimSystemEvents.System._1sec && !isPaused)
        this.OnSecondElapsed?.Invoke();
    }

    private void SimCon_DataReceived(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
    {
      if (e.RequestId == lVarRequestId)
      {
        double value = (double)e.Data;
      }
      if (e.RequestId == simLeakRequestId)
      {
        double value = (double)e.Data;
        OnLeakDataUpdate(value);
      }
      else if (e.RequestId == simStuckRequestId)
      {
        double value = (double)e.Data;
        OnStuckDataUpdate(value);
      }
      else
      {
        MockPlaneData data = (MockPlaneData)e.Data;
        OnMockPlaneDataUpdate(data);
      }
    }

    private void OnLeakDataUpdate(double value)
    {
      double newValue = Math.Max(0, value - simLeakStep);
      simCon.Values.Send(simLeakId, newValue);
      Thread.Sleep(1000);
      simLeakRequestId =  simCon.Values.Request(simLeakId);
    }

    private void OnMockPlaneDataUpdate(MockPlaneData data)
    {
      this.OnData?.Invoke(data);
    }

    private void SimCon_Connected(ESimConnect.ESimConnect sender)
    {
      // intentionally blank
    }

    private void SimCon_Disconnected(ESimConnect.ESimConnect sender)
    {
      // intentionally blank
    }

    private void SimCon_ThrowsException(ESimConnect.ESimConnect sender, SimConnectException ex)
    {
      throw new ApplicationException(ex.ToString());
    }

    internal void StopAsync()
    {
      simCon.Close();
      isRunning = false;
    }

    internal void RequestDataManually()
    {
      this.simCon.Structs.Request<MockPlaneData>();
    }

    private TypeId simVarFailId;

    private TypeId simLeakId;
    private RequestId simLeakRequestId;
    private double simLeakStep = 0.01;

    private TypeId simStuckId;
    private RequestId simStuckRequestId;
    private double simStuckValue = -1;
    internal void Open()
    {
      simCon.Open();
      simCon.Structs.Register<MockPlaneData>(validate: true);
      simCon.Structs.RequestRepeatedly<MockPlaneData>(SimConnectPeriod.SECOND, sendOnlyOnChange: true);
      simCon.SystemEvents.Register(ESimConnect.Enumerations.SimSystemEvents.System.Pause);
      simCon.SystemEvents.Register(ESimConnect.Enumerations.SimSystemEvents.System._1sec);

      simVarFailId = simCon.Values.Register<double>(SimVars.Aircraft.Engine.ENG_ON_FIRE__index + "1", "Number", SimConnectSimTypeName.FLOAT64);

      simLeakId = simCon.Values.Register<double>("FUEL TANK LEFT MAIN LEVEL", "Number", SimConnectSimTypeName.FLOAT64, validate: true);


      // tohle funguje blbě v defalt planech, ale vubec v FBW
      //simStuckId = simCon.RegisterPrimitive<double>(SimVars.Aircraft.Control.TRAILING_EDGE_FLAPS_LEFT_PERCENT, "Number", "FLOAT64", validate: true);

      simStuckId = simCon.Values.Register<double>(SimVars.Aircraft.Control.FLAPS_HANDLE_INDEX__index + "1", "Number", SimConnectSimTypeName.FLOAT64);
      // not Writeable: simStuckId = simCon.RegisterPrimitive<int>(SimVars.Aircraft.Control.TRAILING_EDGE_FLAPS_LEFT_INDEX, "Number", "INT32", validate: true);
    }

    internal void FailEngine()
    {
      simCon.ClientEvents.Invoke("TOGGLE_ENGINE1_FAILURE");
    }

    internal void FailEngineFire()
    {
      double value = 1;
      simCon.Values.Send(simVarFailId, value);
    }

    internal void FailLeak()
    {
      simLeakRequestId = simCon.Values.Request(simLeakId);
    }

    internal void FailStuck()
    {
      this.simStuckRequestId = simCon.Values.RequestRepeatedly(simStuckId, SimConnectPeriod.SIM_FRAME, sendOnlyOnChange: true);
    }

    private void OnStuckDataUpdate(double value)
    {
      if (-1 == simStuckValue)
      {
        simStuckValue = value;
      }
      if (value != simStuckValue)
        simCon.Values.Send(simStuckId, simStuckValue);
    }

    TypeId lVarTypeId;
    RequestId lVarRequestId;
    private const int clientDataId = 1234;
    internal void TestExternal()
    {
      //customEventId = simCon.RegisterCustomEvent("LVAR_ACCESS.EFIS"); // A32NX.FCU_SPD_INC_434");
      // simCon.RegisterCustomPrimitive<double>("EFIS_CDA", clientDataId); // A32NX_TRANSPONDER_MODE", clientDataId);
      //lVarTypeId = simCon.RegisterPrimitive<double>("L:A32NX_COND_PACK_FLOW_1", "Number", "FLOAT64");
      lVarTypeId = simCon.Values.Register<double>("L:LIGHTING_LANDING_2", "Number", SimConnectSimTypeName.FLOAT64);
      lVarRequestId = simCon.Values.Request(lVarTypeId);
    }

    internal void TestExternalSet()
    {
      double val = 1;
      simCon.Values.Send(lVarTypeId, val);
    }
  }
}
