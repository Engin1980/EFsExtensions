using Eng.Chlaot.Modules.ChecklistModule.Types;
using ESystem.Miscelaneous;

namespace ChecklistTTS
{
  public class CheckListVM : NotifyPropertyChanged
  {
    public CheckListVM(CheckList checklist)
    {
      this.CheckList = checklist;
      this.State = ProcessState.NotProcessed;
      this.CheckItems = checklist.Items
        .Select(q => new CheckItemVM(q))
        .ToList();
    }

    public CheckList CheckList
    {
      get { return base.GetProperty<CheckList>(nameof(CheckList))!; }
      set { base.UpdateProperty(nameof(CheckList), value); }
    }


    public List<CheckItemVM> CheckItems
    {
      get { return base.GetProperty<List<CheckItemVM>>(nameof(CheckItems))!; }
      set { base.UpdateProperty(nameof(CheckItems), value); }
    }

    public ProcessState State
    {
      get { return base.GetProperty<ProcessState>(nameof(State))!; }
      set { base.UpdateProperty(nameof(State), value); }
    }
  }
}
