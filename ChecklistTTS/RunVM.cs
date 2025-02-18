using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChecklistTTS
{
  internal partial class RunVM : NotifyPropertyChanged
  {
    public string OutputPath
    {
      get => base.GetProperty<string>(nameof(OutputPath))!;
      set => base.UpdateProperty(nameof(OutputPath), value);
    }

    public MetaInfo? MetaInfo
    {
      get { return base.GetProperty<MetaInfo?>(nameof(MetaInfo))!; }
      set { base.UpdateProperty(nameof(MetaInfo), value); }
    }

    public List<CheckListVM> CheckLists
    {
      get => base.GetProperty<List<CheckListVM>>(nameof(CheckLists))!;
      set => base.UpdateProperty(nameof(CheckLists), value);
    }
  }
}
