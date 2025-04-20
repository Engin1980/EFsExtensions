using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.Converters;
using Eng.EFsExtensions.Modules.FailuresModule.Model.Incidents;
using ESystem.Miscelaneous;
using ESystem.WPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FailuresModule.Converters
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
