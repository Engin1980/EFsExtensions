using EFsExtensionsModuleBase;
using Eng.EFsExtensions.EFsExtensionsModuleBase;
using Eng.EFsExtensions.Modules.FailuresModule.Model.Failures;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Eng.EFsExtensions.Modules.FailuresModule.Model.Sustainers
{
  internal class SneakFailureSustainer : SimVarBasedFailureSustainer
  {
    #region Events

    public event Action<SneakFailureSustainer>? Finished;

    #endregion Events

    #region Fields

    private readonly static Random rnd = new Random();
    private readonly Timer updateTimer;

    #endregion Fields

    #region Properties

    public double CurrentSneak
    {
      get => GetProperty<double>(nameof(CurrentSneak))!;
      set => UpdateProperty(nameof(CurrentSneak), value);
    }

    public double LastForcedValue
    {
      get => GetProperty<double>(nameof(LastForcedValue))!;
      set => UpdateProperty(nameof(LastForcedValue), value);
    }

    public double LastSimValue
    {
      get => GetProperty<double>(nameof(LastSimValue))!;
      set => UpdateProperty(nameof(LastSimValue), value);
    }
    public double SneakAdjustPerTick { get; set; }
    public new SneakFailureDefinition Failure { get; private set; }

    #endregion Properties

    #region Constructors

    public SneakFailureSustainer(SneakFailureDefinition failure) : base(failure)
    {
      Failure = failure;
      SneakAdjustPerTick = rnd.NextDouble(failure.MinimalSneakAdjustPerSecond, failure.MaximalSneakAdjustPerSecond);
      updateTimer = new Timer();
      updateTimer.AutoReset = true;
      updateTimer.Elapsed += UpdateTimer_Elapsed;

      ResetInternal();
      DataReceived += SneakFailureSustainer_DataReceived;
      RequestDataRepeatedly();
    }

    private void UpdateTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
      if (double.IsNaN(LastForcedValue) == false)
        SendData(LastForcedValue);
    }

    #endregion Constructors

    #region Methods

    protected override void InitInternal()
    {
      base.InitInternal();
    }

    protected override void ResetInternal()
    {
      updateTimer.Enabled = false;
      CurrentSneak = double.NaN;
      LastForcedValue = double.NaN;
      LastSimValue = double.NaN;
    }
    protected override void StartInternal()
    {
      lock (this)
      {
        CurrentSneak = rnd.NextDouble(Failure.MinimalInitialSneakValue, Failure.MaximalInitialSneakValue);
      }
      updateTimer.Interval = Failure.TickIntervalInMS;
      updateTimer.Enabled = true;
    }

    private void SneakFailureSustainer_DataReceived(double value)
    {
      if (double.IsNaN(CurrentSneak))
        return;

      lock (this)
      {
        LastSimValue = value;

        if (IsSimPaused)
          SendData(LastForcedValue);
        else
        {
          CurrentSneak += SneakAdjustPerTick;
          bool isFinished;
          if (Failure.Direction == SneakFailureDefinition.EDirection.Up)
          {
            isFinished = value > Failure.FinalValue;
            if (Failure.IsPercentageBased)
              LastForcedValue = value + value * CurrentSneak;
            else
              LastForcedValue = value + CurrentSneak;
          }
          else
          {
            isFinished = value < Failure.FinalValue;
            if (Failure.IsPercentageBased)
              LastForcedValue = value - value * CurrentSneak;
            else
              LastForcedValue = value - CurrentSneak;
          }
          // value is set via this.updateTimer

          if (isFinished)
          {
            Reset();
            Finished?.Invoke(this);
          }
        }
      }

    }

    #endregion Methods
  }
}
