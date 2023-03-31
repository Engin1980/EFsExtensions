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
  public class FailuresModule : IModule
  {
    public bool IsReady { get => false; }

    private CtrInit? _InitControl;
    public Control InitControl => this._InitControl ?? throw new ApplicationException("InitControl is null.");

    public Control RunControl => throw new NotImplementedException();

    public string Name => "Failures";

    public event PropertyChangedEventHandler? PropertyChanged;

    public Context Context { get; set; }

    public void Init()
    {
      Context.BuildFailures();
      this._InitControl = new CtrInit(Context);
    }

    public void Run()
    {
      throw new NotImplementedException();
    }

    public void SetUp(ModuleSetUpInfo setUpInfo)
    {
      Context = new Context();
    }

    public void Stop()
    {
      throw new NotImplementedException();
    }
  }
}
