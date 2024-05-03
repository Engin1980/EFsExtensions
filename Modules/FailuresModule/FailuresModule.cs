using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.Modules.FailuresModule;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Eng.Chlaot.Modules.FailuresModule
{
  public class FailuresModule : NotifyPropertyChanged, IModule
  {
    public bool IsReady
    {
      get => base.GetProperty<bool>(nameof(IsReady))!;
      private set => base.UpdateProperty(nameof(IsReady), value);
    }
    private readonly Logger logger;

    private CtrInit? _InitControl;
    private CtrRun? _RunControl;

    public FailuresModule()
    {
      this.IsReady = false;
      this.logger = Logger.Create(this);
    }

    public Control InitControl => this._InitControl ?? throw new ApplicationException("InitControl is null.");

    public Control RunControl => this._RunControl ?? throw new ApplicationException("RunControl is null.");

    public string Name => "Failures";

    public InitContext InitContext { get; set; }

    public void Init()
    {
      this._InitControl = new CtrInit(InitContext);
    }

    public void Run()
    {
      RunContext runContext = new RunContext(InitContext);
      this._RunControl = new CtrRun(runContext);
      runContext.Start();
    }

    public void SetUp(ModuleSetUpInfo setUpInfo)
    {
      InitContext = new InitContext(q => this.IsReady = q);
      Logger.RegisterSenderName(InitContext, "FailuresModule.InitContext");
    }

    public void Stop()
    {
      throw new NotImplementedException();
    }
  }
}
