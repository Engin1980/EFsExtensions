using ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase;
using FailuresModule.Model.Sim;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Model.Run.Sustainers
{
  internal class SneakFailureSustainer : SimVarBasedFailureSustainer
  {
    #region Events

    public event Action<SneakFailureSustainer>? Finished;

    #endregion Events

    #region Fields

    private static Random rnd = new Random();
    private int simSecondElapsedEventId;

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
      this.SneakAdjustPerTick = rnd.NextDouble(failure.MinimalSneakAdjustPerTick, failure.MaximalSneakAdjustPerTick);

      ResetInternal();
      base.DataReceived += SneakFailureSustainer_DataReceived;
      base.RequestDataRepeatedly();
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

          if (isFinished)
          {
            this.Reset();
            Finished?.Invoke(this);
          }
          else
            base.SendData(LastForcedValue);
        }
      }

    }

    #endregion Methods
  }
}
