using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.Converters
{
  public class DoubleNanConverter : TypedConverter<double, string>
  {
    protected override string Convert(double value, object parameter, CultureInfo culture)
    {
      string? s = parameter as string;
      string ret;
      if (double.IsNaN(value))
        ret = "";
      else if (s != null)
        ret = value.ToString(s, CultureInfo.GetCultureInfo("en-US"));
      else
        ret = value.ToString(CultureInfo.GetCultureInfo("en-US"));
      return ret;
    }

    protected override double ConvertBack(string value, object parameter, CultureInfo culture)
    {
      double ret;
      if (value == null || value.Length == 0)
        ret = double.NaN;
      else 
        ret = double.Parse(value, CultureInfo.GetCultureInfo("en-US"));
      return ret;
    }
  }
}
