using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.Converters;
using ESystem.Structs;
using ESystem.WPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog
{
  public class WeightConverter : TypedConverter<Weight?, string>
  {
    public static WeightUnit DefaultUnit { get; set; } = WeightUnit.Kilograms;

    protected override string Convert(Weight? value, object parameter, CultureInfo culture)
    {
      if (value == null) return string.Empty;

      string numberFormat = (string)parameter ?? "N0";
      string ret = value.Value.To(DefaultUnit).Value.ToString(numberFormat) + " " + DefaultUnit.GetDisplayString();
      return ret;
    }

    protected override Weight? ConvertBack(string value, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
