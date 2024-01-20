using ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using System.Collections.Generic;
using System.ComponentModel;

namespace Eng.Chlaot.Modules.ChecklistModule.Types.VM
{
  public class CheckListRunVM : CheckListVM
  {
    public RunState State
    {
      get => base.GetProperty<RunState>(nameof(State))!;
      set => base.UpdateProperty(nameof(State), value);
    }
    public StateCheckEvaluator Evaluator
    {
      get => base.GetProperty<StateCheckEvaluator>(nameof(Evaluator))!;
      set => base.UpdateProperty(nameof(Evaluator), value);
    }

    public CheckListRunVM()
    {
      this.State = RunState.NotYet;
    }
  }
  public abstract class CheckListVM : NotifyPropertyChangedBase
  {
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

    public List<CheckItemRunVM> Items
    {
      get => base.GetProperty<List<CheckItemRunVM>>(nameof(Items))!;
      set => base.UpdateProperty(nameof(Items), value);
    }


  }
}
