using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using System.Collections.Generic;

namespace Eng.Chlaot.Modules.ChecklistModule.Types.RunViews
{
  public class CheckListView : NotifyPropertyChangedBase
  {

    public RunState State
    {
      get => base.GetProperty<RunState>(nameof(State))!;
      set => base.UpdateProperty(nameof(State), value);
    }

    public CheckList CheckList
    {
      get => base.GetProperty<CheckList>(nameof(CheckList))!;
      set => base.UpdateProperty(nameof(CheckList), value);
    }

    public List<CheckItemView> Items
    {
      get => base.GetProperty<List<CheckItemView>>(nameof(Items))!;
      set => base.UpdateProperty(nameof(Items), value);
    }

    public StateCheckEvaluator Evaluator
    {
      get => base.GetProperty<StateCheckEvaluator>(nameof(Evaluator))!;
      set => base.UpdateProperty(nameof(Evaluator), value);
    }
  }
}
