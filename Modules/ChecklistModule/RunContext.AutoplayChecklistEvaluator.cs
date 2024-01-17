using ELogging;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.Modules.ChecklistModule.Types;
using ESystem.Asserting;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Eng.Chlaot.Modules.ChecklistModule
{
  public partial class RunContext
  {
    public class AutoplayChecklistEvaluator
    {
      private readonly Logger logger;
      private readonly StateCheckEvaluator evaluator;
      private readonly object lck = new();
      private bool autoplaySuppressed = false;
      private CheckList? prevList = null;

      public AutoplayChecklistEvaluator(Func<Dictionary<string, double>> variableValuesProvider, Func<Dictionary<string, double>> propertyValuesProvider)
      {
        EAssert.Argument.IsNotNull(variableValuesProvider, nameof(variableValuesProvider));
        EAssert.Argument.IsNotNull(propertyValuesProvider, nameof(propertyValuesProvider));
        this.logger = Logger.Create(this);
        this.evaluator = new StateCheckEvaluator(variableValuesProvider, propertyValuesProvider);
      }

      public bool EvaluateIfShouldPlay(CheckList checkList)
      {
        if (Monitor.TryEnter(lck) == false) return false;
        logger.Invoke(LogLevel.VERBOSE, $"Evaluation started for {checkList.Id}");

        if (prevList != checkList)
        {
          this.evaluator.Reset();
          this.autoplaySuppressed = false;
          prevList = checkList;
        }

        bool ret;
        if (this.autoplaySuppressed)
          ret = false;
        else
          ret = checkList.Trigger != null && this.evaluator.Evaluate(checkList.Trigger);

        this.logger.Invoke(LogLevel.VERBOSE,
          $"Evaluation finished for {checkList.Id} as={ret}, autoplaySupressed={autoplaySuppressed}");
        Monitor.Exit(lck);
        return ret;
      }

      internal IList<StateCheckEvaluator.RecentResult> GetRecentResults() => evaluator.GetRecentResultSet();

      internal void SuppressAutoplayForCurrentChecklist()
      {
        this.autoplaySuppressed = true;
      }
    }
  }
}
