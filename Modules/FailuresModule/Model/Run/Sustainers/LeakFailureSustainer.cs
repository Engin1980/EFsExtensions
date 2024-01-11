using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FailuresModule.Model.Run.Sustainers;
using FailuresModule.Model.Failures;

namespace FailuresModule.Model.Run.Sustainers
{
  internal class LeakFailureSustainer : SimVarBasedFailureSustainer
  {
    #region Fields

    private readonly int expectedNumberOfTicksBeforeLeakOut;
    private int simSecondElapsedEventId;
    private readonly LeakFailureDefinition failure;

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
      this.failure = failure;

      expectedNumberOfTicksBeforeLeakOut = new Random().Next(failure.MinimumLeakTicks, failure.MaximumLeakTicks);
      ResetInternal();
      base.DataReceived += LeakFailureSustainer_DataReceived;
      base.SimCon.EventInvoked += SimCon_EventInvoked; 
      //TODO not using custom refresh leak-tick-ms interval
    }

    #endregion Constructors

    #region Methods

    protected override void InitInternal()
    {
      base.InitInternal();
      this.simSecondElapsedEventId = base.SimCon.RegisterSystemEvent(ESimConnect.SimEvents.System._1sec);
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
      if (e.RequestId == simSecondElapsedEventId && CurrentValue != null && base.IsSimPaused == false)
        ApplyLeak();
    }

    #endregion Methods
  }
}
