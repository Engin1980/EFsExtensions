using ChecklistModule;
using ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static Eng.Chlaot.ChlaotModuleBase.IModule;

namespace Eng.Chlaot.Modules.ChecklistModule
{
  public class ChecklistModule : NotifyPropertyChangedBase, IModule
  {
    private Control _InitControl;
    public Control InitControl => _InitControl;

    private Control _RunControl;
    public Control RunControl => _RunControl;

    private Context _Context;

    public string Name => "Check-lists";

    public bool IsReady
    {
      get => base.GetProperty<bool>(nameof(IsReady))!;
      private set => base.UpdateProperty(nameof(IsReady), value);
    }

    public ChecklistModule()
    {
      this.IsReady = false;
    }
    public void Init(IModule.LogHandler logHandler)
    {
      Settings settings;
      try
      {
        settings = Settings.Load();
      }
      catch (Exception ex)
      {
        settings = new Settings();
      }


      this._Context = new Context(settings, logHandler, q => this.IsReady = q);
      this._InitControl = new CtrInit(this._Context);
      this._RunControl = new CtrRun(this._Context);
    }
  }
}
