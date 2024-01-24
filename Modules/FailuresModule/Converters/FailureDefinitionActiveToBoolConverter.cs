using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.Converters;
using Eng.Chlaot.Modules.FailuresModule.Model.Sustainers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Eng.Chlaot.Modules.FailuresModule.Converters
{
  internal class FailureDefinitionActiveToBoolConverter : TypedConverter<string, object>
  {
    private static BindingList<FailureSustainer> activeSustainers = new();
    public static void SetActiveSustainers(BindingList<FailureSustainer> sustainers) => activeSustainers = sustainers;

    protected override object Convert(string value, object parameter, CultureInfo culture)
    {
      bool isActive = activeSustainers.Any(q => q.Failure.Id == value);
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

    protected override string ConvertBack(object value, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
