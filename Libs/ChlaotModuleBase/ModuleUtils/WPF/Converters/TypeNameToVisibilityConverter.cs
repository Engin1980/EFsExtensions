using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.Converters
{
  public class TypeNameToVisibilityConverter : TypedConverter<object, Visibility>
  {
    protected override Visibility Convert(object value, object parameter, CultureInfo culture)
    {
      Type t = value.GetType();
      string expectedTypeName = (string)parameter;
      Visibility ret = expectedTypeName.Equals(t.Name) ? Visibility.Visible : Visibility.Hidden;
      return ret;
    }

    protected override object ConvertBack(Visibility value, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
