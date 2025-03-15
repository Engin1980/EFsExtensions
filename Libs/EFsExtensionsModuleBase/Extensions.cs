using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase
{
  public static class Extensions
  {
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
      foreach (var item in enumerable)
      {
        action(item);
      }
    }

    public static BindingList<T> ToBindingList<T>(this List<T> lst)
    {
      BindingList<T> ret = new BindingList<T>(lst);
      return ret;
    }

    public static double NextDouble(this Random rnd, double minInclusive, double maxExclusive)
    {
      double ret = rnd.NextDouble() * (maxExclusive - minInclusive) + minInclusive;
      return ret;
    }

    public static List<T> With<T>(this List<T> lst, params T[] values)
    {
      lst.AddRange(values.ToList());
      return lst;
    }

  }
}
