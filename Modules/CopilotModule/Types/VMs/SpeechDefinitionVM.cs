using ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.Interfaces;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.VMs;
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

      public RunTimeVM(IVariableValuesProvider variableValuesProvider, IPropertyValuesProvider propertyValuesProvider)
      {
        this.evaluator = new StateCheckEvaluator(variableValuesProvider, propertyValuesProvider);
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

    public void CreateRunTime(IPropertyValuesProvider propertyValuesProvider)
    {
      this.RunTime = new RunTimeVM(this.Variables, propertyValuesProvider);
    }
  }
}
