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
      DistanceUnit unit = Decode(parameter);
      double ret = value.To(unit).Value;
      return ret;
    }

    private static DistanceUnit Decode(object parameter)
    {
      DistanceUnit ret;
      try
      {
        ret = ESystem.Enums.FromDisplayName<DistanceUnit>((string)parameter)
          ?? throw new ApplicationException($"Unknown distance unit '{parameter}'.");
      }
      catch (Exception ex)
      {
        throw new ApplicationException($"Unexpected or invalid ConverterParameter '{parameter}'.", ex);
      }

      return ret;
    }

    protected override Distance ConvertBack(double value, object parameter, CultureInfo culture)
    {
      DistanceUnit unit = Decode(parameter);
      Distance ret = Distance.Of(value, unit);
      return ret;
    }
  }
}
