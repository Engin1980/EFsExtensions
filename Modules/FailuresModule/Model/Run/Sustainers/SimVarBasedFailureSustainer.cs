using FailuresModule.Model.Sim;
using FailuresModule.Types.Run.Sustainers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Model.Run.Sustainers
{
  public abstract class SimVarBasedFailureSustainer : FailureSustainer
  {
    // taken from https://github.com/kanaron/RandFailuresFS2020/blob/ab2cb278df8ede6739bcfe60a7f34c9f97b8f5ba/RandFailuresFS2020/RandFailuresFS2020/Simcon.cs
    private const string DEFAULT_UNIT = "Number";
    private const string DEFAULT_TYPE = "FLOAT64";
    private bool isRegistered = false;
    private int typeId;
    private int requestId = -1;
    protected bool IsSimPaused { get; private set; } = false;
    protected event Action<double>? DataReceived;

    protected SimVarBasedFailureSustainer(FailureDefinition failure) : base(failure)
    {
      base.SimCon.EventInvoked += SimCon_EventInvoked;
    }

    protected override void InitInternal()
    {
      base.SimCon.RegisterSystemEvent(ESimConnect.SimEvents.System.Paused);
      base.SimCon.RegisterSystemEvent(ESimConnect.SimEvents.System.Unpaused);
    }

    private void SimCon_EventInvoked(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectEventInvokedEventArgs e)
    {
      if (e.Event == ESimConnect.SimEvents.System.Paused)
        IsSimPaused = true;
      else if (e.Event == ESimConnect.SimEvents.System.Unpaused)
        IsSimPaused = false;
    }

    private void RegisterIfRequired()
    {
      if (isRegistered) return;

      this.SimCon.DataReceived += SimCon_DataReceived;

      string name = this.Failure.SimConPoint;
      this.typeId = this.SimCon.RegisterPrimitive<double>(name, DEFAULT_UNIT, DEFAULT_TYPE);
      this.isRegistered = true;
    }

    protected void RequestData()
    {
      RegisterIfRequired();
      this.SimCon.RequestPrimitive(this.typeId, out this.requestId);
    }

    private void SimCon_DataReceived(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
    {
      if (e.RequestId == this.requestId)
      {
        double data = (double)e.Data;
        DataReceived?.Invoke(data);
      }
    }

    internal void SendData(double value)
    {
      RegisterIfRequired();
      this.SimCon.SendPrimitive(this.typeId, value);
    }

    internal void RequestDataRepeatedly()
    {
      RegisterIfRequired();
      this.SimCon.RequestPrimitiveRepeatedly(this.typeId, out this.requestId, Microsoft.FlightSimulator.SimConnect.SIMCONNECT_PERIOD.SECOND);
    }
  }
}
