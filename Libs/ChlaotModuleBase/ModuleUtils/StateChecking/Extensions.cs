using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking
{
  public static class Extensions
  {
    public static TValue? TryGet<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull
    {
      TValue? ret;
      if (dictionary.TryGetValue(key, out ret!) == false)
      {
        ret = default;
      }
      return ret;
    }

    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueProducer) where TKey : notnull
    {
      TValue ret;
      if (dictionary.TryGetValue(key, out ret!) == false)
      {
        ret = valueProducer();
        dictionary[key] = ret;
      }
      return ret;
    }

    public static T SelectIf<T>(this T item, bool condition, Func<T, T> selector)
    {
      return condition
        ? selector.Invoke(item)
        : item;
    }
  }
}
