using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  //TODO add to esystem
  public static class Extensions
  {
    public static string ToString(this TimeSpan ts, string format)
    {
      if (string.IsNullOrEmpty(format))
        return ts.ToString(); // Default formatting

      return format
          .Replace("dd", ts.Days.ToString("D2"))  // Two-digit days
          .Replace("d", ts.Days.ToString())       // Single-digit days
          .Replace("hh", ts.Hours.ToString("D2")) // Two-digit hours
          .Replace("h", ts.Hours.ToString())      // Single-digit hours
          .Replace("mm", ts.Minutes.ToString("D2"))
          .Replace("m", ts.Minutes.ToString())
          .Replace("ss", ts.Seconds.ToString("D2"))
          .Replace("s", ts.Seconds.ToString());
    }
  }
}
