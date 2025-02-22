using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.Modules.ChecklistModule.Types;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ChecklistTTS.Model
{
  [Serializable]
  public class InitVM : NotifyPropertyChanged
  {

    public string OutputPath
    {
      get => GetProperty<string>(nameof(OutputPath))!;
      set => UpdateProperty(nameof(OutputPath), value);
    }

    public string ChecklistFileName
    {
      get => GetProperty<string>(nameof(ChecklistFileName))!;
      set => UpdateProperty(nameof(ChecklistFileName), value);
    }


    public List<CheckList> Checklists
    {
      get { return GetProperty<List<CheckList>>(nameof(Checklists))!; }
      set { UpdateProperty(nameof(Checklists), value); }
    }

    public InitVM()
    {
      OutputPath = ".";
    }
  }
}
