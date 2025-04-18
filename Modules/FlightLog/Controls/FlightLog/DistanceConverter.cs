using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.Converters;
using ESystem.Exceptions;
using ESystem.Structs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog
{
  public class DistanceConverter : TypedConverter<Distance, double>
  {
    protected override double Convert(Distance value, object parameter, CultureInfo culture)
    {
      string numberFormat;
      DistanceUnit unit;
      (numberFormat, unit) = Decode(parameter);
      double ret = value.To(unit).Value;
      return ret;
    }

    private static (string, DistanceUnit) Decode(object parameter)
    {
      string retStringFormat = "F0";
      DistanceUnit retDistanceUnit;
      if (parameter is string s && s.Count(q => q == ';') < 2)
      {
        string[] pts = s.Split(";");
        if (pts.Length == 2)
        {
          retStringFormat = pts[0];
          retDistanceUnit = ESystem.Enums.FromDisplayName<DistanceUnit>(pts[1]) ?? throw new ApplicationException($"Unknown distance unit '{parameter}'.");
        }
        else
          retDistanceUnit = ESystem.Enums.FromDisplayName<DistanceUnit>(s) ?? throw new ApplicationException($"Unknown distance unit '{parameter}'.");
      }
      else
        throw new ApplicationException($"Unexpected ConverterParameter '{parameter}'.");
      return (retStringFormat, retDistanceUnit);
    }

    protected override Distance ConvertBack(double value, object parameter, CultureInfo culture)
    {
      string _;
      DistanceUnit unit;
      (_, unit) = Decode(parameter);
      Distance ret = Distance.Of(value, unit);
      return ret;
    }
  }
}
