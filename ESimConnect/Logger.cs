using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESimConnect
{
  public static class Logger
  {
    public static Action<string>? LogHandler { get; set; } = null;
    public static void Log(string msg, Type sender = null)
    {
      if (sender != null) msg = $"[{sender.Name}] {msg}";
      LogInternal(msg);
    }
    private static void LogInternal(string msg)
    {
      LogHandler?.Invoke(msg);
    }
  }
}
