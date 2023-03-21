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
    private readonly List<ProcessInfo> processInfos;
    private readonly Action onAdjustmentCompleted;

    public AffinityAdjuster(List<Rule> rules, List<ProcessInfo> processInfos, Action onAdjustmentCompleted)
    {
      logHandler = Logger.RegisterSender(typeof(AffinityAdjuster));
      this.rules = rules;
      this.processInfos = processInfos;
      this.onAdjustmentCompleted = onAdjustmentCompleted;
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
      Rule affinityAllRule = new()
      {
        Roll = "0-256"
      };

      Process[] processes = Process.GetProcesses();
      foreach (var process in processes)
      {
        if (processInfos.Any(q => process.Id == q.Id
            && q.AffinitySetResult != ProcessInfo.EResult.Ok))
          continue;

        try
        {
          process.ProcessorAffinity = AffinityUtils.ToIntPtr(affinityAllRule.CoreFlags.ToArray());
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
        logHandler.Invoke(LogLevel.VERBOSE, $"Adjusting process {process.Id}/{process.ProcessName}");
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
          SetAffinityIfRequired(process, rule, pi);
          SetPriorityIfRequired(process, rule, pi);
        }
        else
        {
          logHandler.Invoke(LogLevel.VERBOSE, $"No rule to cover '{pi.Name} ({pi.Id})', skipping.");
          pi.RuleTitle = "(none)";
        }
        GetAffinity(process, pi);
        GetPriority(process, pi);

        Application.Current.Dispatcher.Invoke(() => { this.processInfos.Add(pi); });
      }
      logHandler.Invoke(LogLevel.INFO, $"Affinity adjustment completed, " +
        $"changed {processInfos.Count(q => q.AffinitySetResult == ProcessInfo.EResult.Ok)}, " +
        $"failed {processInfos.Count(q => q.AffinitySetResult == ProcessInfo.EResult.Failed)}, " +
        $"skipped {processInfos.Count(q => q.AffinitySetResult == ProcessInfo.EResult.Unchanged)}.");
      logHandler.Invoke(LogLevel.INFO, $"Priority adjustment completed, " +
        $"changed {processInfos.Count(q => q.PrioritySetResult == ProcessInfo.EResult.Ok)}, " +
        $"failed {processInfos.Count(q => q.PrioritySetResult == ProcessInfo.EResult.Failed)}, " +
        $"skipped {processInfos.Count(q => q.PrioritySetResult == ProcessInfo.EResult.Unchanged)}.");
      isRunning = false;
      lock (this)
      {
        Monitor.PulseAll(this);
      }
      this.onAdjustmentCompleted();
    }

    private void GetPriority(Process process, ProcessInfo pi)
    {
      try
      {
        pi.Priority = process.PriorityClass;
        pi.PriorityGetResult = ProcessInfo.EResult.Ok;
      }
      catch (Exception)
      {
        pi.PriorityGetResult = ProcessInfo.EResult.Failed;
        pi.Priority = null;
      }
    }

    private void SetPriorityIfRequired(Process process, Rule rule, ProcessInfo pi)
    {
      if (rule.ShouldChangePriority)
      {
        try
        {
          process.PriorityClass = rule.PriorityClass;
          pi.PrioritySetResult = ProcessInfo.EResult.Ok;
        }
        catch (Exception ex)
        {
          pi.PrioritySetResult = ProcessInfo.EResult.Failed;
          logHandler.Invoke(LogLevel.VERBOSE, $"Adjusting '{pi.Name} ({pi.Id})' priority failed. " +
              $"Probably no rights to do this. {ex.Message}");
        }
      }
      else
        pi.PrioritySetResult = ProcessInfo.EResult.Unchanged;
    }

    private void SetAffinityIfRequired(Process process, Rule rule, ProcessInfo pi)
    {
      if (rule.ShouldChangeAffinity)
      {
        try
        {
          process.ProcessorAffinity = AffinityUtils.ToIntPtr(rule.CoreFlags.ToArray());
          pi.AffinitySetResult = ProcessInfo.EResult.Ok;
        }
        catch (Exception ex)
        {
          pi.AffinitySetResult = ProcessInfo.EResult.Failed;
          logHandler.Invoke(LogLevel.VERBOSE, $"Adjusting '{pi.Name} ({pi.Id})' affinity failed. " +
            $"Probably no rights to do this. {ex.Message}");
        }
      }
      else
        pi.AffinitySetResult = ProcessInfo.EResult.Unchanged;
    }

    private static void GetAffinity(Process process, ProcessInfo pi)
    {
      try
      {
        pi.Affinity = process.ProcessorAffinity;
        pi.AffinityGetResult = ProcessInfo.EResult.Ok;
      }
      catch (Exception)
      {
        pi.AffinityGetResult = ProcessInfo.EResult.Failed;
        pi.Affinity = null;
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