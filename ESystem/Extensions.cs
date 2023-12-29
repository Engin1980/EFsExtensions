using System.Text;

namespace ESystem
{
  public static class Extensions
  {
    public static StringBuilder AppendIf(this StringBuilder sb, bool condition, string text)
    {
      if (condition)
        sb.Append(text);
      return sb;
    }

    public static bool None<T>(this IEnumerable<T> items, Func<T, bool> predicate)
    {
      return items.Any(predicate) == false;
    }

    public static Tout Pipe<Tin, Tout>(this Tin obj, Func<Tin, Tout> selector)
    {
      return selector(obj);
    }

    public static List<Tin> FlattenRecursively<Tin, Tcol>(this IEnumerable<Tin> lst, Func<Tcol, List<Tin>> subListSelector)
    {
      List<Tin> ret = new();

      foreach (var item in lst)
      {
        if (item is Tcol subListSource)
        {
          List<Tin> subList = subListSelector(subListSource);
          List<Tin> flatted = subList.FlattenRecursively(subListSelector);
          ret.AddRange(flatted);
        }
        else
          ret.Add(item);
      }

      return ret;
    }

    public static IEnumerable<T> Tap<T>(this IEnumerable<T> lst, Action<T> action)
    {
      foreach (var item in lst)
      {
        action(item);
      }
      return lst;
    }
  }
}
