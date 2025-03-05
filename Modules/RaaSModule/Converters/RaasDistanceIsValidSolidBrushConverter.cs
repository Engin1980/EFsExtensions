using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.Converters;
using Eng.EFsExtensions.Modules.RaaSModule.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Eng.EFsExtensions.Modules.RaaSModule.Converters
{
  internal class RaasDistanceIsValidSolidBrushConverter : TypedConverter<string, SolidColorBrush>
  {
    protected override SolidColorBrush Convert(string value, object parameter, CultureInfo culture)
    {
      if (parameter == null || parameter is not string || (parameter as string)!.Split(";").Length != 2)
      {
        throw new ArgumentException("Parameter must be a string in format '#RRGGBB;#RRGGBB'.");
      }

      string[] colors = (parameter as string)!.Split(";");
      try
      {
        RaasDistance.Parse(value);
        return new SolidColorBrush(FromHex(colors[0]));
      }
      catch
      {
        return new SolidColorBrush(FromHex(colors[1]));
      }
    }

    protected override string ConvertBack(SolidColorBrush value, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    public static Color FromHex(string hex)
    {
      // Odstranění mřížky, pokud je přítomna
      hex = hex.Replace("#", "");

      byte a = 255; // Výchozí hodnota pro alfa kanál
      byte r, g, b;

      if (hex.Length == 8) // ARGB formát (#AARRGGBB)
      {
        a = System.Convert.ToByte(hex[..2], 16);
        r = System.Convert.ToByte(hex.Substring(2, 2), 16);
        g = System.Convert.ToByte(hex.Substring(4, 2), 16);
        b = System.Convert.ToByte(hex.Substring(6, 2), 16);
      }
      else if (hex.Length == 6) // RGB formát (#RRGGBB)
      {
        r = System.Convert.ToByte(hex[..2], 16);
        g = System.Convert.ToByte(hex.Substring(2, 2), 16);
        b = System.Convert.ToByte(hex.Substring(4, 2), 16);
      }
      else if (hex.Length == 4) // ARGB formát (#ARGB)
      {
        a = System.Convert.ToByte(new string(hex[0], 2), 16);
        r = System.Convert.ToByte(new string(hex[1], 2), 16);
        g = System.Convert.ToByte(new string(hex[2], 2), 16);
        b = System.Convert.ToByte(new string(hex[3], 2), 16);
      }
      else if (hex.Length == 3) // RGB formát (#RGB)
      {
        r = System.Convert.ToByte(new string(hex[0], 2), 16);
        g = System.Convert.ToByte(new string(hex[1], 2), 16);
        b = System.Convert.ToByte(new string(hex[2], 2), 16);
      }
      else
      {
        throw new ArgumentException("Invalid HEX format. Use #RGB, #ARGB, #RRGGBB or #AARRGGBB.");
      }

      return Color.FromArgb(a, r, g, b);
    }
  }
}
