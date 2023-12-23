using FailuresModule.Types.Run.Sustainers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FailuresModule.Converters
{
  internal class FailureDefinitionActiveToBoolConverter : IValueConverter
  {
    private static BindingList<FailureSustainer> activeSustainers = new();
    public static void SetActiveSustainers(BindingList<FailureSustainer> sustainers) => activeSustainers = sustainers;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var id = (string)value;
      bool isActive = activeSustainers.Any(q => q.Failure.Id == id);
      object ret;
      if ((parameter is string s && s.Contains('|')))
            {
        string[] pts = s.Split('|');
        ret = isActive ? pts[0] : pts[1];
      }
      else
        ret = isActive;
      return ret;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
