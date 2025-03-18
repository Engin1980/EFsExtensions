using Eng.EFsExtensions.Modules.AffinityModule;
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
using ESystem.Logging;

namespace Eng.EFsExtensions.Modules.AffinityModule
{
  internal class ProcessAdjuster : IDisposable
  {
    private record RuleSet(AffinityRule? AffinityRule, PriorityRule? PriorityRule);

    private bool isRunning = false;
    private readonly List<AffinityRule> affinityRules;
    private readonly List<PriorityRule> priorityRules;
    private readonly List<ProcessAdjustResult> processAdjusts = new();
    private readonly Logger logger;

    public delegate void SingleProcessCompletedHandler(ProcessAdjustResult processAdjustResult);
    public delegate void AllProcessesCompletedHandler(List<ProcessAdjustResult> allResults);
    public event SingleProcessCompletedHandler? SingleProcessCompleted;
    public event AllProcessesCompletedHandler? AllProcessesCompleted;

    public ProcessAdjuster(List<AffinityRule> affinityRules, List<PriorityRule> priorityRules)
    {
      EAssert.Argument.IsNotNull(affinityRules, nameof(affinityRules));
      EAssert.Argument.IsNotNull(priorityRules, nameof(priorityRules));

      this.logger = Logger.Create(this);
      this.affinityRules = affinityRules;
      this.priorityRules = priorityRules;
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
      this.AllProcessesCompleted?.Invoke(this.processAdjusts.ToList());
    }

    private void ApplyRules()
    {
      Dictionary<Process, RuleSet> mapping = MapNewProcessesToRules();
      logger.Invoke(LogLevel.DEBUG, $"Analysis completed, adjusting {mapping.Count} processes.");

      foreach (var item in mapping)
      {
        Process process = item.Key;
        AffinityRule? affinityRule = item.Value.AffinityRule;
        PriorityRule? priorityRule = item.Value.PriorityRule;

        logger.Invoke(LogLevel.DEBUG, $"Adjusting process {process.Id}/{process.ProcessName}");
        ProcessAdjustResult pi = new()
        {
          Id = process.Id,
          Name = process.ProcessName,
          WindowTitle = process.MainWindowTitle,
          ThreadCount = process.Threads.Count,
          AffinityRule = affinityRule,
          PriorityRule = priorityRule
        };

        if (affinityRule != null)
          SetAffinityIfRequired(process, affinityRule, pi);
        else
          pi.AffinitySetResult = pi.AffinityGetResult = ProcessAdjustResult.EResult.Unchanged;
        if (priorityRule != null)
          SetPriorityIfRequired(process, priorityRule, pi);
        else
          pi.PrioritySetResult = pi.PriorityGetResult = ProcessAdjustResult.EResult.Unchanged;

        if (affinityRule == null && priorityRule == null)
          logger.Invoke(LogLevel.DEBUG, $"No rule to cover '{pi.Name} ({pi.Id})', skipping.");

        this.processAdjusts.Add(pi);
        this.SingleProcessCompleted?.Invoke(pi);
        //Application.Current.Dispatcher.Invoke(() => { this.processAdjusts.Add(pi); });
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
      logger.Invoke(LogLevel.DEBUG, $"Evaluating priority for '{pi.Name} ({pi.Id})'.");
      ProcessPriorityClass targetPriority = rule.Priority;
      ProcessPriorityClass? currentPriority;
      try
      {
        pi.PriorityPre = currentPriority = process.PriorityClass;
        pi.PriorityGetResult = ProcessAdjustResult.EResult.Ok;
      }
      catch (Exception ex)
      {
        currentPriority = null;
        pi.PriorityGetResult = ProcessAdjustResult.EResult.Failed;
        logger.Invoke(LogLevel.DEBUG, $"Getting '{pi.Name} ({pi.Id})' priority failed. " +
              $"Probably no rights to do this. {ex.Message}");
      }

      if (currentPriority != null)
      {
        if (currentPriority == targetPriority)
          pi.PrioritySetResult = ProcessAdjustResult.EResult.Unchanged;
        else
          try
          {
            pi.PriorityPost = process.PriorityClass = targetPriority;
            pi.PrioritySetResult = ProcessAdjustResult.EResult.Ok;
          }
          catch (Exception ex)
          {
            pi.PrioritySetResult = ProcessAdjustResult.EResult.Failed;
            logger.Invoke(LogLevel.DEBUG, $"Adjusting '{pi.Name} ({pi.Id})' priority failed. " +
              $"Probably no rights to do this. {ex.Message}");
          }
      }
    }

    private void SetAffinityIfRequired(Process process, AffinityRule rule, ProcessAdjustResult pi)
    {
      logger.Invoke(LogLevel.DEBUG, $"Evaluating affinity for '{pi.Name} ({pi.Id})'.");
      IntPtr targetAffinity = AffinityUtils.ToIntPtr(rule.CoreFlags.ToArray());
      IntPtr? currentAffinity;
      try
      {
        pi.AffinityPre = currentAffinity = process.ProcessorAffinity;
        pi.AffinityGetResult = ProcessAdjustResult.EResult.Ok;
      }
      catch (Exception ex)
      {
        currentAffinity = null;
        pi.AffinityGetResult = ProcessAdjustResult.EResult.Failed;
        logger.Invoke(LogLevel.DEBUG, $"Getting '{pi.Name} ({pi.Id})' affinity failed. " +
              $"Probably no rights to do this. {ex.Message}");
      }

      if (currentAffinity != null)
      {
        if (currentAffinity == targetAffinity)
          pi.AffinitySetResult = ProcessAdjustResult.EResult.Unchanged;
        else
          try
          {
            pi.AffinityPost = process.ProcessorAffinity = targetAffinity;
            pi.AffinitySetResult = ProcessAdjustResult.EResult.Ok;
          }
          catch (Exception ex)
          {
            pi.AffinitySetResult = ProcessAdjustResult.EResult.Failed;
            logger.Invoke(LogLevel.DEBUG, $"Adjusting '{pi.Name} ({pi.Id})' affinity failed. " +
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