using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Shares
{
  public static class ShareManager
  {
    private static readonly Dictionary<string, object> inner = new();

    public static void EnsureExists(string key, Func<object> producer)
    {
      lock (inner)
      {
        if (inner.ContainsKey(key) == false)
        {
          object it = producer.Invoke();
          inner[key] = it;
        }
      }
    }

    public static T Get<T>(string key)
    {
      object tmp;
      lock (inner)
      {
        if (inner.ContainsKey(key) == false)
          throw new ApplicationException($"ShareManager does not contain key '{key}'.");
        tmp = inner[key];
      }
      if (tmp is not T)
        throw new ApplicationException(
          $"ShareManager item of key {key} is of type {tmp.GetType().Name}, " +
          $"but expected type is {typeof(T).Name}");
      T ret = (T)tmp;
      return ret;
    }

    public static void Delete(string key)
    {
      lock (inner)
      {
        if (inner.ContainsKey(key))
        {
          inner.Remove(key);
        }
      }
    }
  }
}
