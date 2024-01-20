using ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Eng.Chlaot.Modules.ChecklistModule.Types.VM
{
  public class CheckListVM : NotifyPropertyChangedBase
  {
    public class RunTimeVM : NotifyPropertyChangedBase
    {
      private readonly StateCheckEvaluator evaluator;

      public RunState State
      {
        get => base.GetProperty<RunState>(nameof(State))!;
        set => base.UpdateProperty(nameof(State), value);
      }

      public BindingList<StateCheckEvaluator.RecentResult> EvaluatorRecentResultVM
      {
        get => base.GetProperty<BindingList<StateCheckEvaluator.RecentResult>>(nameof(EvaluatorRecentResultVM))!;
        set => base.UpdateProperty(nameof(EvaluatorRecentResultVM), value);
      }

      public DateTime EvaluatorRecentResultDateTime
      {
        get => base.GetProperty<DateTime>(nameof(EvaluatorRecentResultDateTime))!;
        set => base.UpdateProperty(nameof(EvaluatorRecentResultDateTime), value);
      }

      public bool IsActive
      {
        get => base.GetProperty<bool>(nameof(IsActive))!;
        set
        {
          base.UpdateProperty(nameof(IsActive), value);
          this.IsActivePostfixString = value ? " (Active)" : "";
        }
      }

      public string IsActivePostfixString
      {
        get => base.GetProperty<string>(nameof(IsActivePostfixString))!;
        set => base.UpdateProperty(nameof(IsActivePostfixString), value);
      }

      public RunTimeVM(List<Variable> variables, Func<Dictionary<string, double>> propertyValuesProvider)
      {
        var dict = variables.ToDictionary(q => q.Name, q => q.Value);
        this.evaluator = new StateCheckEvaluator(() => dict, propertyValuesProvider);
        this.State = RunState.NotYet;
      }

      public bool Evaluate(IStateCheckItem item)
      {
        bool ret = this.evaluator.Evaluate(item);
        this.EvaluatorRecentResultDateTime = DateTime.Now;
        this.EvaluatorRecentResultVM = this.evaluator.GetRecentResultSet().ToBindingList();
        return ret;
      }

      public void ResetEvaluator()
      {
        this.evaluator.Reset();
      }
    }

    public CheckList CheckList
    {
      get => base.GetProperty<CheckList>(nameof(CheckList))!;
      set => base.UpdateProperty(nameof(CheckList), value);
    }

    public BindingList<BindingKeyValue<string, double>> Variables
    {
      get => base.GetProperty<BindingList<BindingKeyValue<string, double>>>(nameof(Variables))!;
      set => base.UpdateProperty(nameof(Variables), value);
    }

    public List<CheckItemVM> Items
    {
      get => base.GetProperty<List<CheckItemVM>>(nameof(Items))!;
      set => base.UpdateProperty(nameof(Items), value);
    }

    public string DisplayString
    {
      get => base.GetProperty<string>(nameof(DisplayString))!;
      set => base.UpdateProperty(nameof(DisplayString), value);
    }

    public RunTimeVM RunTime { get; private set; } = null!;

    public void CreateRuntime(Func<Dictionary<string, double>> propertyValuesProvider)
    {
      this.RunTime = new RunTimeVM(this.CheckList.Variables, propertyValuesProvider);
    }
  }
}
