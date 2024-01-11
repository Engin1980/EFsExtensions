using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.Converters;
using FailuresModule.Model.Incidents;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Converters
{
  internal class PercentageFormatter : TypedConverter<Percentage, string>
  {
    protected override string Convert(Percentage value, object parameter, CultureInfo culture)
    {
      double d = value * 100;
      string par = (string)parameter;
      string ret = d.ToString(par) + "%";
      return ret;
    }

    protected override Percentage ConvertBack(string value, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
