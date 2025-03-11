using Eng.EFsExtensions.Modules.FailuresModule.Model.Failures;
using ESimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FailuresModule.Model.Sustainers
{
  internal class LeakFailureSustainer : SimVarBasedFailureSustainer
  {
    #region Fields

    private readonly int expectedNumberOfTicksBeforeLeakOut;
    private EventId simSecondElapsedEventId;
    private readonly LeakFailureDefinition failure;

    #endregion Fields

    #region Properties

    public double? CurrentValue
    {
      get => GetProperty<double?>(nameof(CurrentValue))!;
      private set => UpdateProperty(nameof(CurrentValue), value);
    }

    public double? InitialValue
    {
      get => GetProperty<double?>(nameof(InitialValue))!;
      private set => UpdateProperty(nameof(InitialValue), value);
    }

    public double? LeakPerTick
    {
      get => GetProperty<double?>(nameof(LeakPerTick))!;
      private set => UpdateProperty(nameof(LeakPerTick), value);
    }

    #endregion Properties

    #region Constructors

    public LeakFailureSustainer(LeakFailureDefinition failure) : base(failure)
    {
      this.failure = failure;

      expectedNumberOfTicksBeforeLeakOut = new Random().Next(failure.MinimumLeakTicks, failure.MaximumLeakTicks);
      ResetInternal();
      DataReceived += LeakFailureSustainer_DataReceived;
      RequestDataRepeatedly();
      ESimObj.ExtTime.SimSecondElapsed += ESimCon_SimSecondElapsed;
    }

    #endregion Constructors

    #region Methods

    protected override void InitInternal()
    {
      base.InitInternal();
      simSecondElapsedEventId = ESimObj.ESimCon.SystemEvents.Register(ESimConnect.Definitions.SimEvents.System._1sec);
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
      //TODO If set incorrectly, for fuel the leak can be so low that its lower than the current consumption
      //     causing leek is not visibile, or even positive.
      CurrentValue -= LeakPerTick;
      if (CurrentValue < 0) CurrentValue = 0;
      SendData(CurrentValue!.Value);
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

    private void ESimCon_SimSecondElapsed()
    {
      if (CurrentValue != null && ESimObj.ExtTime.IsSimPaused == false)
        ApplyLeak();
    }

    #endregion Methods
  }
}
