using AffinityModule;
using ELogging;
using ESystem.Asserting;
using static ESystem.Functions;
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
  internal class ProcessAdjuster : IDisposable
  {
    private record RuleSet(AffinityRule? AffinityRule, PriorityRule? PriorityRule);

    private bool isRunning = false;
    private readonly List<AffinityRule> affinityRules;
    private readonly List<PriorityRule> priorityRules;
    private readonly List<ProcessAdjustResult> processAdjusts = new();
    private readonly Action onAdjustmentCompleted;
    private readonly Logger logger;

    public ProcessAdjuster(List<AffinityRule> affinityRules, List<PriorityRule> priorityRules, Action onAdjustmentCompleted)
    {
      EAssert.Argument.IsNotNull(affinityRules, nameof(affinityRules));
      EAssert.Argument.IsNotNull(priorityRules, nameof(priorityRules));

      this.logger = Logger.Create(this);
      this.affinityRules = affinityRules;
      this.priorityRules = priorityRules;
      this.onAdjustmentCompleted = onAdjustmentCompleted;
    }

    public void AdjustAsync()
    {
      Logger.Log(this, LogLevel.INFO, "AdjustAsync() invoked.");
      Action a = () => RunTaskIfPossible(ApplyRules, true);
      var task = new Task(a);
      task.Start();
    }

    public void ResetAsync()
    {
      this.logger.Log(LogLevel.INFO, "Reset() invoked.");
      Action a = () => RunTaskIfPossible(CancelRules, false);
      var task = new Task(a);
      task.Start();
    }

    private void CancelRules()
    {
      AffinityRule affinityAllRule = new()
      {
        Roll = "0-256"
      };

      Process[] processes = Process.GetProcesses();
      foreach (var process in processes)
      {
        if (processAdjusts.Any(q => process.Id == q.Id
            && q.AffinitySetResult != ProcessAdjustResult.EResult.Ok))
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
      processAdjusts.Clear();
    }

    public void RunTaskIfPossible(Action action, bool abortIfBlocked)
    {
      lock (this)
      {
        while (isRunning)
        {
          this.logger.Log(LogLevel.WARNING, $"Task invoked while previous not completed yet, aborting?={abortIfBlocked}.");
          if (abortIfBlocked)
            return;
          else
            Monitor.Wait(this);
        }
        isRunning = true;
      }

      action.Invoke();

      lock (this)
      {
        isRunning = false;
        Monitor.PulseAll(this);
      }
      this.onAdjustmentCompleted();
    }

    private void ApplyRules()
    {
      Dictionary<Process, RuleSet> mapping = MapNewProcessesToRules();
      logger.Invoke(LogLevel.VERBOSE, $"Analysis completed, adjusting {mapping.Count} processes.");

      foreach (var item in mapping)
      {
        Process process = item.Key;
        AffinityRule? affinityRule = item.Value.AffinityRule;
        PriorityRule? priorityRule = item.Value.PriorityRule;

        logger.Invoke(LogLevel.VERBOSE, $"Adjusting process {process.Id}/{process.ProcessName}");
        ProcessAdjustResult pi = new()
        {
          Id = process.Id,
          Name = process.ProcessName,
          WindowTitle = process.MainWindowTitle,
          ThreadCount = process.Threads.Count
        };


        if (affinityRule != null)
          SetAffinityIfRequired(process, affinityRule, pi);
        if (priorityRule != null)
          SetPriorityIfRequired(process, priorityRule, pi);

        if (affinityRule == null && priorityRule == null)
        {
          logger.Invoke(LogLevel.VERBOSE, $"No rule to cover '{pi.Name} ({pi.Id})', skipping.");
          pi.RuleTitle = "(none)";
        }

        Application.Current.Dispatcher.Invoke(() => { this.processAdjusts.Add(pi); });
      }

      logger.Invoke(LogLevel.INFO, $"Affinity adjustment completed, " +
        $"changed {processAdjusts.Count(q => q.AffinitySetResult == ProcessAdjustResult.EResult.Ok)}, " +
        $"failed {processAdjusts.Count(q => q.AffinitySetResult == ProcessAdjustResult.EResult.Failed)}, " +
        $"skipped {processAdjusts.Count(q => q.AffinitySetResult == ProcessAdjustResult.EResult.Unchanged)}.");
      logger.Invoke(LogLevel.INFO, $"Priority adjustment completed, " +
        $"changed {processAdjusts.Count(q => q.PrioritySetResult == ProcessAdjustResult.EResult.Ok)}, " +
        $"failed {processAdjusts.Count(q => q.PrioritySetResult == ProcessAdjustResult.EResult.Failed)}, " +
        $"skipped {processAdjusts.Count(q => q.PrioritySetResult == ProcessAdjustResult.EResult.Unchanged)}.");

    }

    private void SetPriorityIfRequired(Process process, PriorityRule rule, ProcessAdjustResult pi)
    {
      ProcessPriorityClass targetPriority = rule.Priority;
      ProcessPriorityClass? currentPriority;
      try
      {
        currentPriority = process.PriorityClass;
        pi.PriorityGetResult = ProcessAdjustResult.EResult.Ok;
      }
      catch (Exception ex)
      {
        currentPriority = null;
        pi.PriorityGetResult = ProcessAdjustResult.EResult.Failed;
        logger.Invoke(LogLevel.VERBOSE, $"Getting '{pi.Name} ({pi.Id})' priority failed. " +
              $"Probably no rights to do this. {ex.Message}");
      }

      if (currentPriority != null)
      {
        if (currentPriority == targetPriority)
          pi.PrioritySetResult = ProcessAdjustResult.EResult.Unchanged;
        else
          try
          {
            process.PriorityClass = targetPriority;
            pi.PrioritySetResult = ProcessAdjustResult.EResult.Ok;
          }
          catch (Exception ex)
          {
            pi.PrioritySetResult = ProcessAdjustResult.EResult.Failed;
            logger.Invoke(LogLevel.VERBOSE, $"Adjusting '{pi.Name} ({pi.Id})' priority failed. " +
              $"Probably no rights to do this. {ex.Message}");
          }
      }
    }

    private void SetAffinityIfRequired(Process process, AffinityRule rule, ProcessAdjustResult pi)
    {
      IntPtr targetAffinity = AffinityUtils.ToIntPtr(rule.CoreFlags.ToArray());
      IntPtr? currentAffinity;
      try
      {
        currentAffinity = process.ProcessorAffinity;
        pi.AffinityGetResult = ProcessAdjustResult.EResult.Ok;
      }
      catch (Exception ex)
      {
        currentAffinity = null;
        pi.AffinityGetResult = ProcessAdjustResult.EResult.Failed;
        logger.Invoke(LogLevel.VERBOSE, $"Getting '{pi.Name} ({pi.Id})' affinity failed. " +
              $"Probably no rights to do this. {ex.Message}");
      }

      if (currentAffinity != null)
      {
        if (currentAffinity == targetAffinity)
          pi.AffinitySetResult = ProcessAdjustResult.EResult.Unchanged;
        else
          try
          {
            process.ProcessorAffinity = targetAffinity;
            pi.AffinitySetResult = ProcessAdjustResult.EResult.Ok;
          }
          catch (Exception ex)
          {
            pi.AffinitySetResult = ProcessAdjustResult.EResult.Failed;
            logger.Invoke(LogLevel.VERBOSE, $"Adjusting '{pi.Name} ({pi.Id})' affinity failed. " +
              $"Probably no rights to do this. {ex.Message}");
          }
      }
    }

    private Dictionary<Process, RuleSet> MapNewProcessesToRules()
    {
      Dictionary<Process, RuleSet> ret = new();
      Process[] processes = Process.GetProcesses();

      foreach (var process in processes)
      {
        if (processAdjusts.Any(q => q.Id == process.Id)) continue; // already set process

        AffinityRule? affinityRule = this.affinityRules
          .FirstOrDefault(q => System.Text.RegularExpressions.Regex.IsMatch(
            process.ProcessName, q.Regex));
        PriorityRule? priorityRule = this.priorityRules
          .FirstOrDefault(q => System.Text.RegularExpressions.Regex.IsMatch(
            process.ProcessName, q.Regex));
        ret[process] = new(affinityRule, priorityRule);
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