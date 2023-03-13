using ChecklistModule;
using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static Eng.Chlaot.ChlaotModuleBase.IModule;

namespace Eng.Chlaot.Modules.ChecklistModule
{
  public class ChecklistModule : NotifyPropertyChangedBase, IModule
  {
    private Control? _InitControl;
    public Control InitControl => _InitControl ?? throw new ApplicationException("Control not provided.");

    private Control? _RunControl;
    public Control RunControl => _RunControl ?? throw new ApplicationException("Control not provided.");

    private InitContext? initContext;
    private RunContext? runContext;
    private readonly NewLogHandler logHandler;

    private Settings? settings;

    public string Name => "Check-lists";

    public bool IsReady
    {
      get => base.GetProperty<bool>(nameof(IsReady))!;
      private set => base.UpdateProperty(nameof(IsReady), value);
    }

    public ChecklistModule()
    {
      this.IsReady = false;
      this.logHandler = Logger.RegisterSender(this);
    }


    public void SetUp(ModuleSetUpInfo setUpInfo)
    {
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
      this.initContext = new InitContext(this.settings!, q => this.IsReady = q);
      this._InitControl = new CtrInit(this.initContext);
    }

    public void Run()
    {
      this.runContext = new(this.initContext!);
      this._RunControl = new CtrRun(this.runContext);

      this.initContext = null;
      this._InitControl = null;
    }

    public void Stop()
    {
      this.runContext?.Stop();
    }
  }
}
