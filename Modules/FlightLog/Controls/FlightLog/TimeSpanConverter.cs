using ESystem.WPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog
{
  public class TimeSpanConverter : TypedConverter<TimeSpan?, string>
  {
    public enum DisplayFormat
    {
      DHMS,
      HM,
      MS,
      HMS
    }

    public static DisplayFormat DefaultFormat { get; set; } = DisplayFormat.DHMS;

    protected override string Convert(TimeSpan? value, object parameter, CultureInfo culture)
    {
      if (value == null) return string.Empty;

      DisplayFormat df = DefaultFormat;
      if (parameter is string formatString && Enum.TryParse(formatString, out DisplayFormat parsedFormat))
      {
        df = parsedFormat;
      }

      bool isNeg = value.Value.TotalMilliseconds < 0;
      value = new TimeSpan((long)Math.Abs(value.Value.Ticks));
      string ret = df switch
      {
        DisplayFormat.DHMS => ToDHMS(value.Value),
        DisplayFormat.HM => ToHM(value.Value),
        DisplayFormat.MS => ToMS(value.Value),
        DisplayFormat.HMS => ToHMS(value.Value),
        _ => throw new ESystem.Exceptions.UnexpectedEnumValueException(df),
      };
      if (isNeg)
        ret = "-" + ret;
      return ret;
    }

    private static string ToHMS(TimeSpan value) => $"{value.Hours}:{value.Minutes:D2}:{value.Seconds:D2}";

    private static string ToMS(TimeSpan value) => $"{(int)value.TotalMinutes}:{value.Seconds:D2}";

    private static string ToHM(TimeSpan value) => (int)value.TotalHours + ":" + value.Minutes.ToString("00");

    private static string ToDHMS(TimeSpan value)
    {
      if (value.Days > 0)
        return $"{value.Days}d {value.Hours}:{value.Minutes:D2}:{value.Seconds:D2}";
      else
        return $"{value.Hours}:{value.Minutes:D2}:{value.Seconds:D2}";
    }

    protected override TimeSpan? ConvertBack(string value, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }
}
