using ELogging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Chlaot
{
  internal class LogHelper
  {
    private const string LOG_FILE_NAME = "log.txt";
    internal static void RegisterGlobalLogListener()
    {
      void process(LogItem item)
      {
        var dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string txt = $"{dat} T{item.ThreadInfo.Id:00} {item.Level,-8} {item.SenderName} :: {item.Message}\n";
        try
        {
          lock (typeof(Logger))
          {
            System.IO.File.AppendAllText(LOG_FILE_NAME, txt);
          }
        }
        catch (Exception ex)
        {
          throw new ApplicationException($"Failed to write to log file '{LOG_FILE_NAME}'", ex);
        }
      }

      List<LogRule> rules = new()
      {
        new LogRule("StateCheckEvaluator",false,false,true,true),
        new LogRule(".*", true,true,true,true)
      };
      Logger.RegisterLogAction(process, rules);
    }

    internal static void RegisterWindowLogListener(Window owner, TextBox txt)
    {
      void txtOutWriter(LogItem li)
      {
        string s = $"{DateTime.Now:HH:mm:ss.fff}/{li.ThreadInfo.Id:00} {li.Level} :: {li.SenderName} :: {li.Message}\n";
        txt.AppendText(s);
        txt.ScrollToEnd();
      };
      void dispatcherEnsurer(LogItem li)
      {
        if (Thread.CurrentThread != Application.Current.Dispatcher.Thread)
          Application.Current.Dispatcher.Invoke(() => txtOutWriter(li));
        else
          txtOutWriter(li);
      }

      List<LogRule> rules = new()
      {
        //new LogRule("Playing\\.AutoPlaybackManager",false,false,true,true),
        new LogRule("Playing\\.Player",false,true,true,true),
        new LogRule("SimConManager",false,true,true,true),
        new LogRule("StateCheckEvaluator",false,false,true,true),
        new LogRule(".*", false, true, true, true)
      };

      Logger.RegisterLogAction(q => dispatcherEnsurer(q), rules, owner);
    }
  }
}
