using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.Converters
{
  public class TypeNameToBoolConverter : TypedConverter<object, bool>
  {
    protected override bool Convert(object value, object parameter, CultureInfo culture)
    {
      Type t = value.GetType();
      string expectedTypeName = (string)parameter;
      bool ret = expectedTypeName.Equals(t.Name);
      return ret;
    }

    protected override object ConvertBack(bool value, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
