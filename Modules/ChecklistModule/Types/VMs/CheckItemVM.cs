
using Eng.Chlaot.ChlaotModuleBase;

namespace Eng.Chlaot.Modules.ChecklistModule.Types.VM
{
  public abstract class CheckItemVM : NotifyPropertyChangedBase
  {
    public CheckItem CheckItem
    {
      get => base.GetProperty<CheckItem>(nameof(CheckItem))!;
      set => base.UpdateProperty(nameof(CheckItem), value);
    }
  }

  public class CheckItemRunVM : CheckItemVM
  {
    public RunState State
    {
      get => base.GetProperty<RunState>(nameof(State))!;
      set => base.UpdateProperty(nameof(State), value);
    }

    public CheckItemRunVM()
    {
      this.State = RunState.NotYet;
    }
  }
}
