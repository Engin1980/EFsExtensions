
using Eng.Chlaot.ChlaotModuleBase;
using ESystem.Miscelaneous;

namespace Eng.Chlaot.Modules.ChecklistModule.Types.VM
{
  public class CheckItemVM : NotifyPropertyChanged
  {
    public class RunTimeVM : NotifyPropertyChanged
    {
      public RunState State
      {
        get => base.GetProperty<RunState>(nameof(State))!;
        set => base.UpdateProperty(nameof(State), value);
      }

      public RunTimeVM()
      {
        this.State = RunState.NotYet;
      }
    }

    public CheckItem CheckItem
    {
      get => base.GetProperty<CheckItem>(nameof(CheckItem))!;
      set => base.UpdateProperty(nameof(CheckItem), value);
    }

    public RunTimeVM RunTime { get; } = new();
  }
}