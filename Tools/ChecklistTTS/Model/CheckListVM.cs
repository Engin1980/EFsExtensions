using Eng.EFsExtensions.Modules.ChecklistModule.Types;
using ESystem.Miscelaneous;

namespace ChecklistTTS.Model
{
  public class CheckListVM : NotifyPropertyChanged
  {
    public CheckListVM(CheckList checklist)
    {
      CheckList = checklist;
      State = ProcessState.NotProcessed;
      CheckItems = checklist.Items
        .Select(q => new CheckItemVM(q))
        .ToList();
    }

    public CheckList CheckList
    {
      get { return GetProperty<CheckList>(nameof(CheckList))!; }
      set { UpdateProperty(nameof(CheckList), value); }
    }


    public List<CheckItemVM> CheckItems
    {
      get { return GetProperty<List<CheckItemVM>>(nameof(CheckItems))!; }
      set { UpdateProperty(nameof(CheckItems), value); }
    }

    public ProcessState State
    {
      get { return GetProperty<ProcessState>(nameof(State))!; }
      set { UpdateProperty(nameof(State), value); }
    }
  }
}
