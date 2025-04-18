using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.Converters;
using ESystem.Structs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog
{
  public class SpeedConverter : TypedConverter<Speed, string>
  {
    public static SpeedUnit DefaultUnit { get; set; } = SpeedUnit.KTS;

    protected override string Convert(Speed value, object parameter, CultureInfo culture)
    {
      string numberFormat = (string)parameter ?? "N3";
      string ret = value.To(DefaultUnit).Value.ToString(numberFormat) + " " + DefaultUnit.GetDisplayString();
      return ret;
    }

    protected override Speed ConvertBack(string value, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
