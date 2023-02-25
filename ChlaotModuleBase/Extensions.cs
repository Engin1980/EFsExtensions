using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChlaotModuleBase
{
  public static class Extensions
  {
    public static string GetFullMessage(this Exception ex, string delimiter = " <== ")
    {
      List<string> tmp = new();
      while (ex != null)
      {
        tmp.Add(ex.Message);
        ex = ex.InnerException!;
      }
      string ret = string.Join(delimiter, tmp);
      return ret;
    }

  }
}
