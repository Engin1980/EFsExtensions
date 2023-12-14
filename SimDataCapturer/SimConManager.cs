using ELogging;
using ESimConnect;
using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
      MockPlaneData data = (MockPlaneData)e.Data;
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

    internal void Open()
    {
      simCon.Open();
      simCon.RegisterType<MockPlaneData>();
      simCon.RequestDataRepeatedly<MockPlaneData>(null, SIMCONNECT_PERIOD.SECOND, sendOnlyOnChange: true);
      simCon.RegisterSystemEvent(SimEvents.System.Pause);
      simCon.RegisterSystemEvent(SimEvents.System._1sec);
    }
  }
}
