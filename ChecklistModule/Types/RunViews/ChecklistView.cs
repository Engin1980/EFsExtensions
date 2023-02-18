using ChlaotModuleBase;
using System.Collections.Generic;

namespace ChecklistModule.Types.RunViews
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
  }
}
