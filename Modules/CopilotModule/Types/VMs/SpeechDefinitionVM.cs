using ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.VMs;
using Eng.Chlaot.Modules.CopilotModule.Types;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.CopilotModule.Types.VMs
{
  public class SpeechDefinitionVM : NotifyPropertyChanged
  {
    public class RunTimeVM : NotifyPropertyChanged
    {
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

      public RunTimeVM(VariableVMS variables, PropertyVMS propertyVMs)
      {
        this.evaluator = new StateCheckEvaluator(variables.GetAsDict, propertyVMs.GetAsDict);
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

    public VariableVMS Variables
    {
      get => base.GetProperty<VariableVMS>(nameof(Variables))!;
      set => base.UpdateProperty(nameof(Variables), value);
    }

    internal void CreateRunTime(PropertyVMS propertyVMs)
    {
      this.RunTime = new RunTimeVM(this.Variables, propertyVMs)
      {
        IsReadyToBeSpoken=true
      };
    }

    public override string ToString() => $"{SpeechDefinition.ToString} {{SpeechDefinitionVM}}";
  }
}
