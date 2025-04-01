using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.Converters;
using ESystem.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog
{
  public class DistanceUnitConverter : TypedConverter<double, double>
  {
    private enum Unit
    {
      m,
      km,
      ft,
      NM
    }

    protected override double Convert(double value, object parameter, CultureInfo culture)
    {
      Unit[]? units = DecodeUnits(parameter);
      if (units == null) return value;

      double ret = Convert(value, units[0], units[1]);
      return ret;
    }

    private static double Convert(double value, Unit from, Unit to)
    {
      if (from == to) return value;

      // Convert the value to meters first
      double valueInMeters = from switch
      {
        Unit.m => value,
        Unit.km => value * 1000,
        Unit.ft => value * 0.3048,
        Unit.NM => value * 1852,
        _ => throw new UnexpectedEnumValueException(from)
      };

      // Convert the value from meters to the target unit
      return to switch
      {
        Unit.m => valueInMeters,
        Unit.km => valueInMeters / 1000,
        Unit.ft => valueInMeters / 0.3048,
        Unit.NM => valueInMeters / 1852,
        _ => throw new UnexpectedEnumValueException(to)
      };
    }


    private Unit[]? DecodeUnits(object parameter)
    {
      if (parameter == null) return null;
      string[] pars = ((string)parameter).Split("2");
      if (pars.Length != 2) return null;

      Unit[] ret = new Unit[2];
      if (!Enum.TryParse(pars[0], out ret[0])) return null;
      if (!Enum.TryParse(pars[1], out ret[1])) return null;
      return ret;
    }

    protected override double ConvertBack(double value, object parameter, CultureInfo culture)
    {
      Unit[]? units = DecodeUnits(parameter);
      if (units == null) return value;

      double ret = Convert(value, units[1], units[0]);
      return ret;
    }
  }
}
