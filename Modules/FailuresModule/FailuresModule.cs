using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using FailuresModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Eng.Chlaot.Modules.FailuresModule
{
  public class FailuresModule : NotifyPropertyChangedBase, IModule
  {
    public bool IsReady
    {
      get => base.GetProperty<bool>(nameof(IsReady))!;
      private set => base.UpdateProperty(nameof(IsReady), value);
    }
    private readonly NewLogHandler logHandler;

    private CtrInit? _InitControl;
    private CtrRun? _RunControl;

    public FailuresModule()
    {
      this.IsReady = false;
      this.logHandler = Logger.RegisterSender(this);
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
      RunContext runContext = RunContext.Create(InitContext.FailureDefinitions, InitContext.FailureSet);
      runContext.Init();
      this._RunControl = new CtrRun(runContext);
    }

    public void SetUp(ModuleSetUpInfo setUpInfo)
    {
      InitContext = new InitContext(this.logHandler, q => this.IsReady = q);
    }

    public void Stop()
    {
      throw new NotImplementedException();
    }
  }
}
