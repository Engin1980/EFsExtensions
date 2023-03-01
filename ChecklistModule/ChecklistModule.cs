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

    private InitContext initContext;
    private RunContext runContext;
    private LogHandler? parentLogHandler;
    private readonly LogHandler logHandler;

    private Settings settings;

    public string Name => "Check-lists";

    public bool IsReady
    {
      get => base.GetProperty<bool>(nameof(IsReady))!;
      private set => base.UpdateProperty(nameof(IsReady), value);
    }

    public ChecklistModule()
    {
      this.IsReady = false;
      this.logHandler = (l, m) => parentLogHandler?.Invoke(l, m);
    }


    public void SetUp(ModuleSetUpInfo setUpInfo)
    {
      this.parentLogHandler = setUpInfo.LogHandler;
      try
      {
        settings = Settings.Load();
        logHandler.Invoke(LogLevel.INFO, "Settings loaded.");
      }
      catch (Exception ex)
      {
        logHandler.Invoke(LogLevel.ERROR, "Unable to load settings. " + ex.GetFullMessage());
        settings = new Settings();
      }
    }

    public void Init()
    {
      this.initContext = new InitContext(this.settings, logHandler, q => this.IsReady = q);
      this._InitControl = new CtrInit(this.initContext);
    }

    public void Run()
    {
      this.runContext = new(this.initContext, logHandler);
      this._RunControl = new CtrRun(this.runContext);

      this.initContext = null;
      this.initContext = null;
    }

    public void Stop()
    {
      this.runContext.Stop();
    }
  }
}
