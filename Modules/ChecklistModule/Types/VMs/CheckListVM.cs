using ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.VMs;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Eng.Chlaot.Modules.ChecklistModule.Types.VM
{
  internal class CheckListVM : NotifyPropertyChanged
  {
    public class RunTimeVM : NotifyPropertyChanged
    {
      private readonly StateCheckEvaluator evaluator;

      public bool CanBeAutoplayed
      {
        get => base.GetProperty<bool>(nameof(CanBeAutoplayed))!;
        set => base.UpdateProperty(nameof(CanBeAutoplayed), value);
      }

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

      public RunTimeVM(VariableVMS variables, PropertyVMS propertyVMs)
      {
        this.evaluator = new StateCheckEvaluator(variables.GetAsDict, propertyVMs.GetAsDict);
        this.State = RunState.NotYet;
        this.CanBeAutoplayed = true;
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


    public VariableVMS Variables
    {
      get => base.GetProperty<VariableVMS>(nameof(Variables))!;
      set => base.UpdateProperty(nameof(Variables), value);
    }


    public List<CheckItemVM> Items
    {
      get => base.GetProperty<List<CheckItemVM>>(nameof(Items))!;
      set => base.UpdateProperty(nameof(Items), value);
    }

    public string DisplayString
    {
      get => base.GetProperty<string>(nameof(DisplayString)) ?? this.CheckList.Id;
      set => base.UpdateProperty(nameof(DisplayString), value);
    }

    public RunTimeVM RunTime { get; private set; } = null!;

    internal void CreateRuntime(PropertyVMS propertyVMs)
    {
      this.RunTime = new RunTimeVM(Variables, propertyVMs);
    }

    public override string ToString() => $"{this.DisplayString} {{CheckListVM}}";
  }
}
