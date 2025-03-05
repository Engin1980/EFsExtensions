using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChecklistTTS.Model
{
  internal partial class RunVM : NotifyPropertyChanged
  {
    public string OutputPath
    {
      get => GetProperty<string>(nameof(OutputPath))!;
      set => UpdateProperty(nameof(OutputPath), value);
    }

    public MetaInfo? MetaInfo
    {
      get { return GetProperty<MetaInfo?>(nameof(MetaInfo))!; }
      set { UpdateProperty(nameof(MetaInfo), value); }
    }

    public List<CheckListVM> CheckLists
    {
      get => GetProperty<List<CheckListVM>>(nameof(CheckLists))!;
      set => UpdateProperty(nameof(CheckLists), value);
    }
  }
}
