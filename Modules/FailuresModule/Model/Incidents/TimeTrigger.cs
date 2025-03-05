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
        TimeTriggerInterval.OncePerTenSeconds => Percentage.Of((double)1 / this.MtbfHours / 360),
        TimeTriggerInterval.OncePerMinute => Percentage.Of((double)1 / this.MtbfHours / 60),
        TimeTriggerInterval.OncePerTenMinutes => Percentage.Of((double)1 / this.MtbfHours / 6),
        TimeTriggerInterval.OncePerHour => Percentage.Of((double)1 / this.MtbfHours),
        _ => throw new NotImplementedException()
      };
    }

    public TimeTrigger()
    {
      this.MtbfHours = 1000;
      this.Interval = TimeTriggerInterval.OncePerHour;
    }
  }
}
