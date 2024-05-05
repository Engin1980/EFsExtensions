using Eng.Chlaot.Modules.FailuresModule.Model.Failures;
using ESimConnect;
using ESimConnect.Definitions;
using ESystem.Asserting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.FailuresModule.Model.Sustainers
{
  public abstract class SimVarBasedFailureSustainer : FailureSustainer
  {
    // taken from https://github.com/kanaron/RandFailuresFS2020/blob/ab2cb278df8ede6739bcfe60a7f34c9f97b8f5ba/RandFailuresFS2020/RandFailuresFS2020/Simcon.cs
    private const string DEFAULT_UNIT = "Number";
    private const SimConnectSimTypeName DEFAULT_TYPE = SimConnectSimTypeName.FLOAT64;
    private bool isRegistered = false;
    private TypeId? typeId = null;
    private RequestId? requestId = null;
    protected bool IsSimPaused { get; private set; } = false;
    protected event Action<double>? DataReceived;

    protected SimVarBasedFailureSustainer(FailureDefinition failure) : base(failure)
    {
      SimCon.SystemEventInvoked += SimCon_EventInvoked;
    }

    protected override void InitInternal()
    {
      SimCon.SystemEvents.Register(ESimConnect.Enumerations.SimSystemEvents.System.Paused);
      SimCon.SystemEvents.Register(ESimConnect.Enumerations.SimSystemEvents.System.Unpaused);
    }

    private void SimCon_EventInvoked(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectSystemEventInvokedEventArgs e)
    {
      if (e.Event == ESimConnect.Enumerations.SimSystemEvents.System.Paused)
        IsSimPaused = true;
      else if (e.Event == ESimConnect.Enumerations.SimSystemEvents.System.Unpaused)
        IsSimPaused = false;
    }

    private void RegisterIfRequired()
    {
      if (isRegistered) return;

      SimCon.DataReceived += SimCon_DataReceived;

      string name = Failure.SimConPoint;
      this.typeId = SimCon.Values.Register<double>(name, DEFAULT_UNIT, DEFAULT_TYPE);
      isRegistered = true;
    }

    protected void RequestData()
    {
      RegisterIfRequired();
      EAssert.IsNotNull(this.typeId);
      this.requestId = SimCon.Values.Request(this.typeId.Value);
    }

    private void SimCon_DataReceived(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
    {
      if (e.RequestId == requestId)
      {
        double data = (double)e.Data;
        DataReceived?.Invoke(data);
      }
    }

    internal void SendData(double value)
    {
      RegisterIfRequired();
      EAssert.IsNotNull(this.typeId);
      SimCon.Values.Send(this.typeId.Value, value);
    }

    internal void RequestDataRepeatedly()
    {
      RegisterIfRequired();
      EAssert.IsNotNull(this.typeId);
      this.requestId = SimCon.Values.RequestRepeatedly(typeId.Value, SimConnectPeriod.SECOND);
    }
  }
}
