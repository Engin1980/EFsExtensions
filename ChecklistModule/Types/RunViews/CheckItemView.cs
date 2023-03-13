
using Eng.Chlaot.ChlaotModuleBase;

namespace Eng.Chlaot.Modules.ChecklistModule.Types.RunViews
{
  public class CheckItemView : NotifyPropertyChangedBase
  {

    public RunState State
    {
      get => base.GetProperty<RunState>(nameof(State))!;
      set => base.UpdateProperty(nameof(State), value);
    }

    public CheckItem CheckItem
    {
      get => base.GetProperty<CheckItem>(nameof(CheckItem))!;
      set => base.UpdateProperty(nameof(CheckItem), value);
    }
  }
}
