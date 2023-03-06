using AffinityModule;
using ChlaotModuleBase;
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


    public bool IsReady
    {
      get => base.GetProperty<bool>(nameof(IsReady))!;
      set => base.UpdateProperty(nameof(IsReady), value);
    }

    public string Name => "Affinity Module";

    public Control RunControl => this.ctrInit ?? throw new ApplicationException("CtrRun is null");

    public Settings Settings
    {
      get => base.GetProperty<Settings>(nameof(Settings))!;
      set => base.UpdateProperty(nameof(Settings), value);
    }

    public AffinityModule()
    {
      this.context = new Context(q => this.IsReady = q);
    }
    public void Init()
    {
      this.ctrInit = new CtrInit(this.context);
      this.context.LoadSettings();
    }

    public void Run()
    {
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
