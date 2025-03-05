using Eng.EFsExtensions.Modules.ChecklistModule.Types;
using ESystem.Miscelaneous;

namespace ChecklistTTS.Model
{
  public class CheckItemVM : NotifyPropertyChanged
  {
    public CheckItemVM(CheckItem checkitem)
    {
      CheckItem = checkitem;
      State = ProcessState.NotProcessed;
    }

    public CheckItem CheckItem
    {
      get { return GetProperty<CheckItem>(nameof(CheckItem))!; }
      set { UpdateProperty(nameof(CheckItem), value); }
    }

    public ProcessState State
    {
      get { return GetProperty<ProcessState>(nameof(State))!; }
      set { UpdateProperty(nameof(State), value); }
    }

  }
}
