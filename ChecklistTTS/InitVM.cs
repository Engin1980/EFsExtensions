using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.Modules.ChecklistModule.Types;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ChecklistTTSNew
{
  [Serializable]
  public class InitVM : NotifyPropertyChanged
  {

    public string OutputPath
    {
      get => base.GetProperty<string>(nameof(OutputPath))!;
      set => base.UpdateProperty(nameof(OutputPath), value);
    }

    public string ChecklistFileName
    {
      get => base.GetProperty<string>(nameof(ChecklistFileName))!;
      set => base.UpdateProperty(nameof(ChecklistFileName), value);
    }


    public List<CheckList> Checklists
    {
      get { return base.GetProperty<List<CheckList>>(nameof(Checklists))!; }
      set { base.UpdateProperty(nameof(Checklists), value); }
    }

    public InitVM()
    {
      OutputPath = ".";
    }
  }
}
