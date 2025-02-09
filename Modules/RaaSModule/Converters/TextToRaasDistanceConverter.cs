using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.Converters;
using Eng.Chlaot.Modules.RaaSModule.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.RaaSModule.Converters
{
  internal class TextToRaasDistanceConverter : TypedConverter<RaasDistance, string>
  {
    protected override string Convert(RaasDistance value, object parameter, CultureInfo culture)
    {
      return value.ToString();
    }

    protected override RaasDistance ConvertBack(string value, object parameter, CultureInfo culture)
    {
      RaasDistance ret = RaasDistance.Parse(value);
      return ret;
    }
  }
}
