using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  //TODO add to esystem
  public static class Extensions
  {
    public static string GetDisplayString(this Enum eenum)
    {
      return ESystem.Enums.GetDisplayName(eenum);
    }

    public static string ToString(this TimeSpan ts, string format)
    {
      if (string.IsNullOrEmpty(format))
        return ts.ToString(); // Default formatting

      return format
          .Replace("dd", ts.Days.ToString("D2"))  // Two-digit days
          .Replace("d", ts.Days.ToString())       // Single-digit days
          .Replace("hh", ts.Hours.ToString("D2")) // Two-digit hours
          .Replace("h", ts.Hours.ToString())      // Single-digit hours
          .Replace("mm", ts.Minutes.ToString("D2"))
          .Replace("m", ts.Minutes.ToString())
          .Replace("ss", ts.Seconds.ToString("D2"))
          .Replace("s", ts.Seconds.ToString());
    }

    public static void RefreshBindings(this DependencyObject parent)
    {
      if (parent == null)
        return;

      foreach (var property in GetDependencyProperties(parent))
      {
        BindingExpression be = BindingOperations.GetBindingExpression(parent, property);
        be?.UpdateTarget(); // Pull data from source and re-run converter
      }

      int childCount = VisualTreeHelper.GetChildrenCount(parent);
      for (int i = 0; i < childCount; i++)
      {
        RefreshBindings(VisualTreeHelper.GetChild(parent, i));
      }
    }

    private static DependencyProperty[] GetDependencyProperties(DependencyObject obj)
    {
      var localValues = obj.GetLocalValueEnumerator();
      var properties = new List<DependencyProperty>();

      while (localValues.MoveNext())
      {
        var entry = localValues.Current;
        if (BindingOperations.IsDataBound(obj, entry.Property))
          properties.Add(entry.Property);
      }

      return properties.ToArray();
    }
  }
}
