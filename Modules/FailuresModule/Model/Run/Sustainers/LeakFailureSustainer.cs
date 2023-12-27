using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FailuresModule.Model.Run.Sustainers;
using FailuresModule.Model.Sim;

namespace FailuresModule.Types.Run.Sustainers
{
  internal class LeakFailureSustainer : SimVarBasedFailureSustainer
  {
    #region Fields

    private const int MAXIMAL_EXPECTED_NUMBER_OF_TICKS_BEFORE_LEAK_OUT = 100 * 60;
    private const int MINIMAL_EXPECTED_NUMBER_OF_TICKS_BEFORE_LEAK_OUT = 5 * 60;
    private readonly int expectedNumberOfTicksBeforeLeakOut;

    private int simSecondElapsedRequestId;

    #endregion Fields

    #region Properties

    public double? CurrentValue
    {
      get => base.GetProperty<double?>(nameof(CurrentValue))!;
      private set => base.UpdateProperty(nameof(CurrentValue), value);
    }

    public double? InitialValue
    {
      get => base.GetProperty<double?>(nameof(InitialValue))!;
      private set => base.UpdateProperty(nameof(InitialValue), value);
    }

    public double? LeakPerTick
    {
      get => base.GetProperty<double?>(nameof(LeakPerTick))!;
      private set => base.UpdateProperty(nameof(LeakPerTick), value);
    }

    #endregion Properties

    #region Constructors

    public LeakFailureSustainer(LeakFailureDefinition failure) : base(failure)
    {
      expectedNumberOfTicksBeforeLeakOut = new Random().Next(MINIMAL_EXPECTED_NUMBER_OF_TICKS_BEFORE_LEAK_OUT, MAXIMAL_EXPECTED_NUMBER_OF_TICKS_BEFORE_LEAK_OUT);
      ResetInternal();
      base.DataReceived += LeakFailureSustainer_DataReceived;
      base.SimCon.EventInvoked += SimCon_EventInvoked;
    }

    #endregion Constructors

    #region Methods

    protected override void InitInternal()
    {
      base.InitInternal();
      base.SimCon.RegisterSystemEvent(ESimConnect.SimEvents.System._1sec, out this.simSecondElapsedRequestId);
    }

    protected override void ResetInternal()
    {
      LeakPerTick = CurrentValue = InitialValue = null;
    }

    protected override void StartInternal()
    {
      RequestData();
    }

    private void ApplyLeak()
    {
      this.CurrentValue -= this.LeakPerTick;
      if (this.CurrentValue < 0) this.CurrentValue = 0;
      base.SendData(this.CurrentValue!.Value);
    }

    private void LeakFailureSustainer_DataReceived(double value)
    {
      if (InitialValue == null)
      {
        lock (this)
        {
          if (InitialValue == null)
          {
            InitialValue = CurrentValue = value;
            LeakPerTick = CurrentValue / expectedNumberOfTicksBeforeLeakOut;
          }
        }
      }
    }

    private void SimCon_EventInvoked(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectEventInvokedEventArgs e)
    {
      if (e.RequestId == simSecondElapsedRequestId && CurrentValue != null && base.IsSimPaused == false)
        ApplyLeak();
    }

    #endregion Methods
  }
}
