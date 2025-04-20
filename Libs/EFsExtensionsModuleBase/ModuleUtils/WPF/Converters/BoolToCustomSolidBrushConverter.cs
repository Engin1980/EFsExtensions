//using ESystem.Asserting;
//using ESystem.WPF;
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Media;

//namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.Converters
//{
//  public class BoolToCustomSolidBrushConverter : TypedConverter<bool, SolidColorBrush>
//  {
//    protected override SolidColorBrush Convert(bool value, object parameter, CultureInfo culture)
//    {
//      var colorDefs = ((string)parameter).Split(";");
//      EAssert.IsTrue(colorDefs.Length == 2, $"There must be exactly 2 colors delimited by ';' defined for {nameof(BoolToCustomSolidBrushConverter)}.");
//      string colorDef = value ? colorDefs[0] : colorDefs[1];
//      SolidColorBrush ret;
//      try
//      {
//        Color color = (Color)ColorConverter.ConvertFromString(colorDef);
//        ret = new SolidColorBrush(color);
//      }
//      catch (Exception ex)
//      {
//        throw new ApplicationException($"Unable to convert '{colorDef}' to System.Windows.Media.Color.", ex);
//      }

//      return ret;
//    }

//    protected override bool ConvertBack(SolidColorBrush value, object parameter, CultureInfo culture)
//    {
//      throw new NotImplementedException();
//    }
//  }
//}
