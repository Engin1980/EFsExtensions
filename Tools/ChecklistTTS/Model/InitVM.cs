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
      ChecklistStartupSpeech = "%id checklist";
      ChecklistCompletedSpeech = "%id checklist completed";
      ChecklistPausedAlertSpeech = "%id checklist pending";
    }


    public string ChecklistStartupSpeech
    {
      get { return base.GetProperty<string>(nameof(ChecklistStartupSpeech))!; }
      set { base.UpdateProperty(nameof(ChecklistStartupSpeech), value); }
    }


    public string ChecklistCompletedSpeech
    {
      get { return base.GetProperty<string>(nameof(ChecklistCompletedSpeech))!; }
      set { base.UpdateProperty(nameof(ChecklistCompletedSpeech), value); }
    }


    public string ChecklistPausedAlertSpeech
    {
      get { return base.GetProperty<string>(nameof(ChecklistPausedAlertSpeech))!; }
      set { base.UpdateProperty(nameof(ChecklistPausedAlertSpeech), value); }
    }
  }
}
