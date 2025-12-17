using ESystem.Asserting;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FailuresModule.Model.Incidents
{
  public class TimeTrigger : Trigger
  {
    private readonly static Random rnd = new();

    public enum TimeTriggerInterval
    {
      OncePerTenSeconds,
      OncePerMinute,
      OncePerTenMinutes,
      OncePerHour
    }

    private readonly int secondDigit = rnd.Next(0, 10);
    private readonly int secondValue = rnd.Next(0, 60);
    private readonly int minuteDigit = rnd.Next(0, 10);
    private readonly int minuteValue = rnd.Next(0, 60);

    public TimeTriggerInterval Interval
    {
      get => base.GetProperty<TimeTriggerInterval>(nameof(Interval))!;
      set
      {
        base.UpdateProperty(nameof(Interval), value);
        UpdateProbability();
      }
    }

    public int MtbfHours
    {
      get => base.GetProperty<int>(nameof(MtbfHours))!;
      set
      {
        base.UpdateProperty(nameof(MtbfHours), Math.Max(value, 1));
        UpdateProbability();
      }
    }

    public Func<bool> EvaluatingFunction
    {
      get
      {
        Func<bool> ret = Interval switch
        {
          TimeTriggerInterval.OncePerTenSeconds => () => DateTime.Now.Second % 10 == secondDigit,
          TimeTriggerInterval.OncePerMinute => () => DateTime.Now.Second == secondValue,
          TimeTriggerInterval.OncePerTenMinutes => () => DateTime.Now.Second == secondValue && DateTime.Now.Minute % 10 == minuteDigit,
          TimeTriggerInterval.OncePerHour => () => DateTime.Now.Second == secondValue && DateTime.Now.Minute == minuteValue,
          _ => throw new NotImplementedException()
        };
        return ret;
      }
      set { throw new ApplicationException($"Setting {nameof(EvaluatingFunction)} property is not possible."); }
    }

    private void UpdateProbability()
    {
      this.Probability = this.Interval switch
      {
        TimeTriggerInterval.OncePerTenSeconds => CalculateProbabilityByMTBF(this.MtbfHours, 10),
        TimeTriggerInterval.OncePerMinute => CalculateProbabilityByMTBF(this.MtbfHours, 60),
        TimeTriggerInterval.OncePerTenMinutes => CalculateProbabilityByMTBF(this.MtbfHours, 600),
        TimeTriggerInterval.OncePerHour => CalculateProbabilityByMTBF(this.MtbfHours, 60 * 60),
        _ => throw new NotImplementedException()
      };

      // TODO old implementation, remove if not needed
      //this.Probability = this.Interval switch
      //{
      //  TimeTriggerInterval.OncePerTenSeconds => Percentage.Of((double)1 / this.MtbfHours / 360),
      //  TimeTriggerInterval.OncePerMinute => Percentage.Of((double)1 / this.MtbfHours / 60),
      //  TimeTriggerInterval.OncePerTenMinutes => Percentage.Of((double)1 / this.MtbfHours / 6),
      //  TimeTriggerInterval.OncePerHour => Percentage.Of((double)1 / this.MtbfHours),
      //  _ => throw new NotImplementedException()
      //};
    }

    /// <summary>
    /// Calculates probability of event during the period w.r.t. MTBF
    /// </summary>
    /// <param name="mtbfHours">MTBF in hours</param>
    /// <param name="seconds">Number of seconds</param>
    /// <returns>Event probability (0..1)</returns>
    public static Percentage CalculateProbabilityByMTBF(double mtbfHours, double seconds)
    {
      EAssert.Argument.IsTrue(mtbfHours > 0, "MTBF must be positive.", nameof(mtbfHours));
      EAssert.Argument.IsTrue(seconds >= 0, "Number of seconds must be positive.", nameof(seconds));

      double mtbfSeconds = mtbfHours * 3600.0;
      double probability = 1.0 - Math.Exp(-seconds / mtbfSeconds);
      Percentage ret = Percentage.Of(probability);
      return ret;
    }

    public TimeTrigger()
    {
      this.MtbfHours = 1000;
      this.Interval = TimeTriggerInterval.OncePerHour;
    }
  }
}
