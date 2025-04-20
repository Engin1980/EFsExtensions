using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.Converters;
using Eng.EFsExtensions.Modules.FailuresModule.Model.Incidents;
using ESystem.Miscelaneous;
using ESystem.WPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Eng.EFsExtensions.Modules.FailuresModule.Converters
{
    public class PercentageToIntConverter : TypedConverter<Percentage, int>
  {
    protected sealed override int Convert(Percentage value, object parameter, CultureInfo culture) => (int)(value * 100);

    protected sealed override Percentage ConvertBack(int value, object parameter, CultureInfo culture) => Percentage.Of(value);

    protected override int ConvertToTarget(object value, object parameter, CultureInfo culture) => (int)(double)value;
  }
}
