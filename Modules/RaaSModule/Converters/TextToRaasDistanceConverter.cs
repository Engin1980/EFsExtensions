using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.Converters;
using Eng.EFsExtensions.Modules.RaaSModule.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.RaaSModule.Converters
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
