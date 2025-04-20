using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.Converters;
using ESystem.WPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.Converters
{
  public class BoolToSolidBrushConverter : TypedConverter<bool, SolidColorBrush>
  {
    protected override SolidColorBrush Convert(bool value, object parameter, CultureInfo culture)
    {
      SolidColorBrush ret;
      ret = value
        ? new SolidColorBrush(Colors.LightGreen)
        : new SolidColorBrush(Colors.LightPink);

      return ret;
    }

    protected override bool ConvertBack(SolidColorBrush value, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
