using ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.Modules.FailuresModule.Model.Failures;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Eng.Chlaot.Modules.FailuresModule.Model.Run.Sustainers
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
      get => base.GetProperty<double>(nameof(CurrentSneak))!;
      set => base.UpdateProperty(nameof(CurrentSneak), value);
    }

    public double LastForcedValue
    {
      get => base.GetProperty<double>(nameof(LastForcedValue))!;
      set => base.UpdateProperty(nameof(LastForcedValue), value);
    }

    public double LastSimValue
    {
      get => base.GetProperty<double>(nameof(LastSimValue))!;
      set => base.UpdateProperty(nameof(LastSimValue), value);
    }
    public double SneakAdjustPerTick { get; set; }
    public new SneakFailureDefinition Failure { get; private set; }

    #endregion Properties

    #region Constructors

    public SneakFailureSustainer(SneakFailureDefinition failure) : base(failure)
    {
      this.Failure = failure;
      this.SneakAdjustPerTick = rnd.NextDouble(failure.MinimalSneakAdjustPerSecond, failure.MaximalSneakAdjustPerSecond);
      this.updateTimer = new Timer();
      this.updateTimer.AutoReset = true;
      this.updateTimer.Elapsed += UpdateTimer_Elapsed;

      ResetInternal();
      base.DataReceived += SneakFailureSustainer_DataReceived;
      base.RequestDataRepeatedly();
    }

    private void UpdateTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
      if (double.IsNaN(LastForcedValue) == false)
        base.SendData(this.LastForcedValue);
    }

    #endregion Constructors

    #region Methods

    protected override void InitInternal()
    {
      base.InitInternal();
    }

    protected override void ResetInternal()
    {
      this.updateTimer.Enabled = false;
      this.CurrentSneak = double.NaN;
      this.LastForcedValue = double.NaN;
      this.LastSimValue = double.NaN;
    }
    protected override void StartInternal()
    {
      lock (this)
      {
        CurrentSneak = rnd.NextDouble(Failure.MinimalInitialSneakValue, Failure.MaximalInitialSneakValue);
      }
      this.updateTimer.Interval = this.Failure.TickIntervalInMS;
      this.updateTimer.Enabled = true;
    }

    private void SneakFailureSustainer_DataReceived(double value)
    {
      if (double.IsNaN(CurrentSneak))
        return;

      lock (this)
      {
        this.LastSimValue = value;

        if (base.IsSimPaused)
          base.SendData(LastForcedValue);
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
            this.Reset();
            Finished?.Invoke(this);
          }
        }
      }

    }

    #endregion Methods
  }
}
