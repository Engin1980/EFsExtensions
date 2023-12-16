using ELogging;
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
      simCon.EventInvoked += SimCon_EventInvoked;
    }

    public void Start()
    {
      isRunning = true;
    }

    public bool IsRunning { get => this.isRunning; }

    private void SimCon_EventInvoked(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectEventInvokedEventArgs e)
    {
      if (e.Event == SimEvents.System.Pause)
        isPaused = e.Value != 0;
      else if (e.Event == SimEvents.System._1sec && !isPaused)
        this.OnSecondElapsed?.Invoke();
    }

    private void SimCon_DataReceived(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
    {
      if (e.RequestId == simLeakRequestId)
      {
        double value = (double)e.Data;
        OnLeakDataUpdate(value);
      } else
      {
        MockPlaneData data = (MockPlaneData)e.Data;
        OnMockPlaneDataUpdate(data);
      }
    }

    private void OnLeakDataUpdate(double value)
    {
      double newValue = Math.Max(0, value - simLeakStep);
      simCon.SendPrimitive(simLeakId, newValue);
      Thread.Sleep(1000);
      simCon.RequestPrimitive(simLeakId, ++simLeakRequestId);
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

    private void SimCon_ThrowsException(ESimConnect.ESimConnect sender, Microsoft.FlightSimulator.SimConnect.SIMCONNECT_EXCEPTION ex)
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
      this.simCon.RequestData<MockPlaneData>();
    }

    private int simVarFailId;
    private int simLeakId;
    private int simLeakRequestId = 1;
    private double simLeakStep = 0.01;
    internal void Open()
    {
      simCon.Open();
      simCon.RegisterType<MockPlaneData>();
      simCon.RequestDataRepeatedly<MockPlaneData>(null, SIMCONNECT_PERIOD.SECOND, sendOnlyOnChange: true);
      simCon.RegisterSystemEvent(SimEvents.System.Pause);
      simCon.RegisterSystemEvent(SimEvents.System._1sec);

      // simVarFailId = simCon.RegisterPrimitive<double>("PARTIAL PANEL ALTITUDE", "Number", "FLOAT64");
      simLeakId = simCon.RegisterPrimitive<double>("FUEL TANK LEFT MAIN LEVEL", "Number", "FLOAT64");
    }

    internal void FailEngine()
    {
      simCon.SendClientEvent("TOGGLE_ENGINE1_FAILURE");
    }

    internal void FailPartialPanelAltimeter()
    {
      uint value = 1;
      simCon.SendPrimitive(simVarFailId, value);
    }

    internal void FailLeak()
    {
      simCon.RequestPrimitive(simLeakId, ++simLeakRequestId);
    }
  }
}
