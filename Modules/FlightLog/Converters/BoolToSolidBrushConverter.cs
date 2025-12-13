using ESystem.Asserting;
using ESystem.WPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Converters
{
  internal class BoolToSolidBrushConverter : TypedConverter<bool, SolidColorBrush>
  {
    public string TrueColor { get; set; } = "0F0";
    public string FalseColor { get; set; } = "F00";

    protected override SolidColorBrush Convert(bool value, object parameter, CultureInfo culture)
    {
      var colorHex = value ? TrueColor : FalseColor;
      if (!colorHex.StartsWith('#'))
        colorHex = "#" + colorHex;
      var color = (Color)ColorConverter.ConvertFromString(colorHex);
      return new SolidColorBrush(color);
    }

    protected override bool ConvertBack(SolidColorBrush value, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
