using AffinityModule;
using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Eng.Chlaot.Modules.AffinityModule
{
  public class AffinityModule : NotifyPropertyChangedBase, IModule
  {

    private readonly Context context;

    private CtrInit? ctrInit;

    public Control InitControl => this.ctrInit ?? throw new ApplicationException("CtrInit is null");

    private CtrRun? ctrRun;

    public bool IsReady
    {
      get => base.GetProperty<bool>(nameof(IsReady))!;
      set => base.UpdateProperty(nameof(IsReady), value);
    }

    public string Name => "Affinity";

    public Control RunControl => this.ctrRun ?? throw new ApplicationException("CtrRun is null");

    public AffinityModule()
    {
      this.context = new Context(q => this.IsReady = q);
    }
    public void Init()
    {
      this.ctrInit = new CtrInit(this.context);
    }

    public void Run()
    {
      this.ctrRun = new CtrRun(context);
      this.context.Run();
    }

    public void SetUp(ModuleSetUpInfo setUpInfo)
    {
      // intentionally blank
    }

    public void Stop()
    {
      this.context.Stop();
    }
  }
}
