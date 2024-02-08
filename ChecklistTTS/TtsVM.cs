using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.Modules.ChecklistModule.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChecklistTTS
{
  internal class TtsVM : NotifyPropertyChangedBase
  {
    public class CheckItemVM : NotifyPropertyChangedBase
    {
      public CheckItem CheckItem
      {
        get => base.GetProperty<CheckItem>(nameof(CheckItem))!;
        set => base.UpdateProperty(nameof(CheckItem), value);
      }
    }
    public class CheckListVM : NotifyPropertyChangedBase
    {

      public CheckList CheckList
      {
        get => base.GetProperty<CheckList>(nameof(CheckList))!;
        set => base.UpdateProperty(nameof(CheckList), value);
      }
      public List<CheckItemVM> CheckItemVMs
      {
        get => base.GetProperty<List<CheckItemVM>>(nameof(CheckItemVMs))!;
        set => base.UpdateProperty(nameof(CheckItemVMs), value);
      }
    }
    public string OutputPath
    {
      get => base.GetProperty<string>(nameof(OutputPath))!;
      set => base.UpdateProperty(nameof(OutputPath), value);
    }

    public List<CheckListVM> CheckListVMs
    {
      get => base.GetProperty<List<CheckListVM>>(nameof(CheckListVMs))!;
      set => base.UpdateProperty(nameof(CheckListVMs), value);
    }
  }
}
