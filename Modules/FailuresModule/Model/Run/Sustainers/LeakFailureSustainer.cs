using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateCheckingSimConnection;
using FailuresModule.Model.Run.Sustainers;
using FailuresModule.Model.Sim;

namespace FailuresModule.Types.Run.Sustainers
{
  internal class LeakFailureSustainer : SimVarBasedFailureSustainer
  {
    private const int MAXIMAL_EXPECTED_NUMBER_OF_TICKS_BEFORE_LEAK_OUT = 100 * 60;
    private const int MINIMAL_EXPECTED_NUMBER_OF_TICKS_BEFORE_LEAK_OUT = 5 * 60;
    private readonly int expectedNumberOfTicksBeforeLeakOut;
    private double? leakPerTick;
    private double? currentValue;
    private int simSecondElapsedRequestId;

    public LeakFailureSustainer(LeakFailureDefinition failure) : base(failure)
    {
      expectedNumberOfTicksBeforeLeakOut = new Random().Next(MINIMAL_EXPECTED_NUMBER_OF_TICKS_BEFORE_LEAK_OUT, MAXIMAL_EXPECTED_NUMBER_OF_TICKS_BEFORE_LEAK_OUT);
      ResetInternal();
      base.DataReceived += LeakFailureSustainer_DataReceived;
      base.SimCon.RegisterSystemEvent(ESimConnect.SimEvents.System._1sec, out this.simSecondElapsedRequestId);
      base.SimCon.EventInvoked += SimCon_EventInvoked;
    }

    private void SimCon_EventInvoked(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectEventInvokedEventArgs e)
    {
      if (e.RequestId == simSecondElapsedRequestId && currentValue != null && base.IsSimPaused == false)
        ApplyLeak();
    }

    private void ApplyLeak()
    {
      this.currentValue -= this.leakPerTick;
      if (this.currentValue < 0) this.currentValue = 0;
      base.SendData(this.currentValue!.Value);
    }

    private void LeakFailureSustainer_DataReceived(double value)
    {
      if (currentValue == null)
      {
        lock (this)
        {
          if (currentValue == null)
          {
            currentValue = value;
            leakPerTick = currentValue / expectedNumberOfTicksBeforeLeakOut;
          }
        }
      }
    }

    protected override void ResetInternal()
    {
      leakPerTick = null;
      currentValue = null;
    }

    protected override void StartInternal()
    {
      RequestData();
    }
  }
}
