using ChecklistModule.Types;
using ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChecklistModule
{
  public class RunContext :NotifyPropertyChangedBase
  {

    public delegate void CurrentChangedDelegate();
    public event CurrentChangedDelegate? CurrentChanged;
    public CheckSet ChecklistSet
    {
      get => base.GetProperty<CheckSet>(nameof(ChecklistSet))!;
      set => base.UpdateProperty(nameof(ChecklistSet), value);
    }

    private readonly Settings settings;
    private readonly LogHandler logHandler;
    public CheckList CurrentChecklist { get; private set; }
    public CheckItem CurrentCheckItem { get; private set; }
    public static LogHandler EmptyLogHandler { get => (l, m) => { }; }

    public RunContext(InitContext initContext, LogHandler logHandler)
    {
      this.ChecklistSet = initContext.ChecklistSet;
      this.settings = initContext.Settings;
      this.logHandler = logHandler ?? EmptyLogHandler;

      this.CurrentChecklist = ChecklistSet.Checklists.First();
      this.CurrentCheckItem = this.CurrentChecklist.Items.First();
    }
  }
}
