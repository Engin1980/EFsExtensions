using ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.Modules.FailuresModule.Model.Incidents;
using ESystem.Asserting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.FailuresModule.Model.VMs
{
  public class TriggerVM : NotifyPropertyChangedBase
  {
    private readonly StateCheckEvaluator? evaluator;

    public Trigger Trigger
    {
      get => base.GetProperty<Trigger>(nameof(Trigger))!;
      set => base.UpdateProperty(nameof(Trigger), value);
    }


    public string InfoString
    {
      get => base.GetProperty<string>(nameof(InfoString))!;
      set => base.UpdateProperty(nameof(InfoString), value);
    }

    public BindingList<BindingKeyValue<DateTime, object>> Evaluations
    {
      get => base.GetProperty<BindingList<BindingKeyValue<DateTime, object>>>(nameof(Evaluations))!;
      set => base.UpdateProperty(nameof(Evaluations), value);
    }

    public TriggerVM(Trigger trigger, Func<Dictionary<string, double>> variableValuesProvider, Func<Dictionary<string, double>> propertyValuesProvider)
    {
      EAssert.IsNotNull(trigger, nameof(trigger));
      EAssert.IsNotNull(variableValuesProvider, nameof(variableValuesProvider));
      EAssert.IsNotNull(propertyValuesProvider, nameof(propertyValuesProvider));

      Trigger = trigger;
      if (trigger is CheckStateTrigger cst)
      {
        this.evaluator = new StateCheckEvaluator(variableValuesProvider, propertyValuesProvider);
        this.InfoString = cst.Condition.ToString()!;
      } else if (trigger is TimeTrigger tt)
      {
        this.InfoString = tt.Interval
      }
      this.Evaluations = new();
    }

    internal bool Evaluate()
    {
      bool ret;
      if (Trigger is TimeTrigger tt)
      {
        ret = tt.EvaluatingFunction();
        if (ret) this.Evaluations.Add(new BindingKeyValue<DateTime, object>(DateTime.Now, "Evaluated true."));
      }
      else if (Trigger is CheckStateTrigger csct)
      {
        EAssert.IsNotNull(evaluator);
        ret = evaluator.Evaluate(csct.Condition);
        if (ret) this.Evaluations.Add(new(DateTime.Now, evaluator.GetRecentResultSet()));
      }
      else
        throw new ApplicationException($"Unsupported type of trigger: {Trigger.GetType().Name}.");

      return ret;
    }
  }
}
