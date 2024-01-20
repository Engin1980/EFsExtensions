using ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel;
using Eng.Chlaot.Modules.CopilotModule.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.CopilotModule.Types.VMs
{
  public class SpeechDefinitionVM : NotifyPropertyChangedBase
  {
    public class RunTimeVM : NotifyPropertyChangedBase
    {
      private readonly Dictionary<string, double> variableValues;
      private readonly StateCheckEvaluator evaluator;

      public BindingList<StateCheckEvaluator.RecentResult> EvaluatorRecentResult
      {
        get => base.GetProperty<BindingList<StateCheckEvaluator.RecentResult>>(nameof(EvaluatorRecentResult))!;
        private set => base.UpdateProperty(nameof(EvaluatorRecentResult), value);
      }

      public DateTime EvaluatorRecentResultDateTime
      {
        get => base.GetProperty<DateTime>(nameof(EvaluatorRecentResultDateTime))!;
        private set => base.UpdateProperty(nameof(EvaluatorRecentResultDateTime), value);
      }

      public bool IsReadyToBeSpoken
      {
        get => base.GetProperty<bool>(nameof(IsReadyToBeSpoken))!;
        set => base.UpdateProperty(nameof(IsReadyToBeSpoken), value);
      }

      public RunTimeVM(List<Variable> variables, Func<Dictionary<string, double>> propertyValuesProvider)
      {
        this.variableValues = variables.ToDictionary(q => q.Name, q => q.Value);
        variables.ForEach(q => q.PropertyChanged += (s, e) =>
        {
          if (e.PropertyName == nameof(Variable.Value))
            variableValues[q.Name] = q.Value;
        });
        this.evaluator = new StateCheckEvaluator(() => variableValues, propertyValuesProvider);
      }

      public bool Evaluate(IStateCheckItem item)
      {
        bool ret = this.evaluator.Evaluate(item);
        this.EvaluatorRecentResultDateTime = DateTime.Now;
        this.EvaluatorRecentResult = this.evaluator.GetRecentResultSet().ToBindingList();
        return ret;
      }
    }

    public RunTimeVM RunTime { get; private set; } = null!;

    public SpeechDefinition SpeechDefinition
    {
      get => base.GetProperty<SpeechDefinition>(nameof(SpeechDefinition))!;
      set => base.UpdateProperty(nameof(SpeechDefinition), value);
    }

    public List<Variable> Variables
    {
      get => base.GetProperty<List<Variable>>(nameof(Variables))!;
      set => base.UpdateProperty(nameof(Variables), value);
    }

    public void CreateRunTime(Func<Dictionary<string, double>> propertyValuesProvider)
    {
      this.RunTime = new RunTimeVM(this.Variables, propertyValuesProvider);
    }
  }
}
