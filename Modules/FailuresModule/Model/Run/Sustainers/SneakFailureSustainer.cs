using ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase;
using FailuresModule.Model.Sim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Model.Run.Sustainers
{
  internal class SneakFailureSustainer : SimVarBasedFailureSustainer
  {
    private int simSecondElapsedEventId;
    private readonly SneakFailureDefinition failure;
    private readonly FailureDefinition finalFailure;

    private event Action<SneakFailureSustainer>? Finished;


    public double LastSimValue
    {
      get => base.GetProperty<double>(nameof(LastSimValue))!;
      set => base.UpdateProperty(nameof(LastSimValue), value);
    }


    public double LastForcedValue
    {
      get => base.GetProperty<double>(nameof(LastForcedValue))!;
      set => base.UpdateProperty(nameof(LastForcedValue), value);
    }

    public double SneakAdjustPerTick { get; set; }


    public double CurrentSneak
    {
      get => base.GetProperty<double>(nameof(CurrentSneak))!;
      set => base.UpdateProperty(nameof(CurrentSneak), value);
    }


    public SneakFailureSustainer(SneakFailureDefinition failure, FailureDefinition finalFailure) : base(failure)
    {
      this.failure = failure;
      this.finalFailure = finalFailure;

      ResetInternal();
      base.DataReceived += SneakFailureSustainer_DataReceived;
      base.SimCon.EventInvoked += SimCon_EventInvoked;
      //TODO not using custom refresh leak-tick-ms interval
    }

    protected override void ResetInternal()
    {
      this.CurrentSneak = double.NaN;
      this.LastForcedValue = double.NaN;
      this.LastSimValue = double.NaN;
    }

    private static Random rnd = new Random();
    protected override void StartInternal()
    {
      lock (this)
      {
        CurrentSneak = rnd.NextDouble(failure.MinimalInitialSneakValue, failure.MaximalInitialSneakValue);
      }
    }

    private void SneakFailureSustainer_DataReceived(double value)
    {
      lock (this)
      {
        this.LastSimValue = value;

        if (base.IsSimPaused)
          base.SendData(LastForcedValue);
        else
        {
          CurrentSneak += SneakAdjustPerTick;
          bool isFinished;
          if (failure.Direction == SneakFailureDefinition.EDirection.Up)
          {
            isFinished = value > failure.FinalValue;
            LastForcedValue = value + CurrentSneak;
          }
          else
          {
            isFinished = value < failure.FinalValue;
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

    private void SimCon_EventInvoked(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectEventInvokedEventArgs e)
    {
      if (e.RequestId == simSecondElapsedEventId && !double.IsNaN(CurrentSneak))
        RequestData();
    }
  }
}
