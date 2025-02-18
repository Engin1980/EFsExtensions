using Eng.Chlaot.Modules.ChecklistModule.Types;
using ESystem.Miscelaneous;

namespace ChecklistTTS
{
  public class CheckItemVM : NotifyPropertyChanged
  {
    public CheckItemVM(CheckItem checkitem)
    {
      this.CheckItem = checkitem;
      this.State = ProcessState.NotProcessed;
    }

    public CheckItem CheckItem
    {
      get { return base.GetProperty<CheckItem>(nameof(CheckItem))!; }
      set { base.UpdateProperty(nameof(CheckItem), value); }
    }

    public ProcessState State
    {
      get { return base.GetProperty<ProcessState>(nameof(State))!; }
      set { base.UpdateProperty(nameof(State), value); }
    }

  }
}
