using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.Modules.SimVarTestModule;
using ESystem.Miscelaneous;
using System.Windows.Controls;

namespace Eng.Chlaot.Modules.SimVarTestModule
{
  public class SimVarTestModule : NotifyPropertyChanged, IModule
  {

    public Context Context
    {
      get => GetProperty<Context>(nameof(Context))!;
      set => UpdateProperty(nameof(Context), value);
    }


    public bool IsReady
    {
      get => base.GetProperty<bool>(nameof(IsReady))!;
      set => base.UpdateProperty(nameof(IsReady), value);
    }


    public Control InitControl { get; private set; } = null!;

    public Control RunControl { get; private set; } = null!;

    public string Name => "SimVar Tester";

    public void Init()
    {
      InitControl = new CtrInit(Context);
    }

    public void Run()
    {
      RunControl = new CtrRun(Context);
    }

    public void SetUp(ModuleSetUpInfo setUpInfo)
    {
      IsReady = false;
      Context = new Context(() => this.IsReady = true);
    }

    public void Stop()
    {
    }
  }

}
