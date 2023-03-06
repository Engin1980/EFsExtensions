using ELogging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Eng.Chlaot.Modules.AffinityModule
{
  internal class AffinityAdjuster : IDisposable
  {
    private NewLogHandler logHandler;
    private bool isRunning = false;
    private readonly List<Rule> rules;
    private readonly BindingList<ProcessInfo> processInfos;

    public AffinityAdjuster(List<Rule> rules, BindingList<ProcessInfo> processInfos)
    {
      logHandler = Logger.RegisterSender(typeof(AffinityAdjuster));
      this.rules = rules;
      this.processInfos = processInfos;
    }

    public void AdjustAffinityAsync()
    {
      lock (this)
      {
        if (isRunning) return;
        isRunning = true;
        var currentTask = new Task(ApplyRules);
        currentTask.Start();
      }
    }

    public void ResetAffinity()
    {
      lock (this)
      {
        while (isRunning)
        {
          Monitor.Wait(this);
        }

        CancelRules();
      }
    }

    private void CancelRules()
    {
      Rule rule = new()
      {
        Roll = "0-256"
      };

      Process[] processes = Process.GetProcesses();
      foreach (var process in processes)
      {
        if (processInfos.Any(q => process.Id == q.Id
                && q.IsAccessible.HasValue
                && q.IsAccessible.Value))
          continue;

        try
        {
          process.ProcessorAffinity = rule.CalculateAffinity();
        }
        catch (Exception)
        {
          //TODO resolve somehow
        }
      }
    }

    private void ApplyRules()
    {
      Dictionary<Process, Rule?> mapping = MapNewProcessesToRules();

      foreach (var item in mapping)
      {
        Process process = item.Key;
        Rule? rule = item.Value;
        ProcessInfo pi = new()
        {
          Id = process.Id,
          Name = process.ProcessName,
          WindowTitle = process.MainWindowTitle,
          ThreadCount = process.Threads.Count
        };

        if (rule != null)
        {
          pi.RuleTitle = rule.TitleOrRegex;
          try
          {
            process.ProcessorAffinity = rule.CalculateAffinity();
            pi.IsAccessible = true;
          }
          catch (Exception)
          {
            pi.IsAccessible = false;
          }

          try
          {
            pi.Affinity = (int)process.ProcessorAffinity;
          }
          catch (Exception)
          {
            pi.Affinity = null;
          }
        }
        else
          pi.RuleTitle = "(none)";
        this.processInfos.Add(pi);
      }
      isRunning = false;
      Monitor.PulseAll(this);
    }

    private Dictionary<Process, Rule?> MapNewProcessesToRules()
    {
      Dictionary<Process, Rule?> ret = new();
      Process[] processes = Process.GetProcesses();

      foreach (var process in processes)
      {
        if (processInfos.Any(q => q.Id == process.Id)) continue; // already set process

        Rule? rule = this.rules
          .FirstOrDefault(q => System.Text.RegularExpressions.Regex.IsMatch(
            process.ProcessName, q.Regex));
        ret[process] = rule;
      }
      return ret;
    }


    public void Dispose()
    {
      Logger.UnregisterSender(this);
      GC.SuppressFinalize(this);
    }
  }
}