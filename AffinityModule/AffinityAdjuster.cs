using AffinityModule;
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
        if (isRunning)
        {
          logHandler.Invoke(LogLevel.INFO, "AdjustAffinityAsync() invoked, but the task is running yet.");
          return;
        }

        logHandler.Invoke(LogLevel.INFO, "AdjustAffinityAsync() invoked.");
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
          process.ProcessorAffinity = AffinityUtils.ToIntPtr(rule.CoreFlags.ToArray());
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
      logHandler.Invoke(LogLevel.VERBOSE, $"Analysis completed, adjusting {mapping.Count} processes.");

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
          logHandler.Invoke(LogLevel.VERBOSE, $"Adjusting '{pi.Name} ({pi.Id})'.");
          pi.RuleTitle = rule.TitleOrRegex;

          if (rule.ShouldChangeAffinity)
          {
            try
            {
              process.ProcessorAffinity = AffinityUtils.ToIntPtr(rule.CoreFlags.ToArray());
              pi.IsAccessible = true;
            }
            catch (Exception ex)
            {
              pi.IsAccessible = false;
              logHandler.Invoke(LogLevel.VERBOSE, $"Adjusting '{pi.Name} ({pi.Id})' failed. " +
                $"Probably no rights to do this. {ex.Message}");
            }

            try
            {
              pi.Affinity = process.ProcessorAffinity;
            }
            catch (Exception)
            {
              pi.Affinity = null;
            }
          }
        }
        else
        {
          logHandler.Invoke(LogLevel.VERBOSE, $"No rule to cover '{pi.Name} ({pi.Id})', skipping.");
          pi.RuleTitle = "(none)";
        }

        Application.Current.Dispatcher.Invoke(() => { this.processInfos.Add(pi); });
      }
      logHandler.Invoke(LogLevel.INFO, $"Adjustment completed, " +
        $"changed {processInfos.Count(q => q.IsAccessible.HasValue && q.IsAccessible.Value)}, " +
        $"failed {processInfos.Count(q => q.IsAccessible.HasValue && !q.IsAccessible.Value)}, " +
        $"skipped {processInfos.Count(q => q.IsAccessible == null)}.");
      isRunning = false;
      lock (this)
      {
        Monitor.PulseAll(this);
      }
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