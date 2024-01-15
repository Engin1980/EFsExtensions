using ELogging;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.Modules.ChecklistModule.Types;
using System.Threading;

namespace Eng.Chlaot.Modules.ChecklistModule
{
  public partial class RunContext
  {
    public class AutoplayChecklistEvaluator
    {
      private readonly StateCheckEvaluator evaluator;
      private readonly object lck = new();
      private readonly RunContext parent;
      private bool autoplaySuppressed = false;
      private CheckList? prevList = null;

      public AutoplayChecklistEvaluator(RunContext parent)
      {
        this.parent = parent;
        this.evaluator = new StateCheckEvaluator(parent.variableValues, parent.propertyValues);
      }

      public bool EvaluateIfShouldPlay(CheckList checkList)
      {
        if (this.parent.simObject.IsSimPaused) return false;
        if (Monitor.TryEnter(lck) == false) return false;

        this.parent.logger.Invoke(LogLevel.VERBOSE, $"Evaluation started for {checkList.Id}");

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

        this.parent.logger.Invoke(LogLevel.VERBOSE,
          $"Evaluation finished for {checkList.Id} as={ret}, autoplaySupressed={autoplaySuppressed}");
        Monitor.Exit(lck);
        return ret;
      }

      internal void SuppressAutoplayForCurrentChecklist()
      {
        this.autoplaySuppressed = true;
      }
    }
  }
}
