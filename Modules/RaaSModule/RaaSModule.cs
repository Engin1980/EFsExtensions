using Eng.Chlaot.ChlaotModuleBase;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Eng.Chlaot.Modules.RaaSModule
{
  public class RaaSModule : NotifyPropertyChanged, IModule
  {
    private readonly Context context;
    private CtrInit? ctrInit;
    private CtrRun? ctrRun;

    public Control InitControl => this.ctrInit ?? throw new ApplicationException("CtrInit is null");
    public Control RunControl => this.ctrRun ?? throw new ApplicationException("CtrRun is null");
    public string Name => "RaaS";
    public bool IsReady
    {
      get => base.GetProperty<bool>(nameof(IsReady))!;
      set => base.UpdateProperty(nameof(IsReady), value);
    }

    public RaaSModule()
    {
      this.context = new Context(q => this.IsReady = q);
    }

    public void Init()
    {
      this.ctrInit = new(this.context);
    }

    public void Restore(Dictionary<string, string> restoreData)
    {
      throw new NotImplementedException();
    }

    public void Run()
    {
      this.ctrRun = new(this.context);
      this.context.Start();
    }

    public void SetUp(ModuleSetUpInfo setUpInfo)
    {
      this.context.Airports = new();

    }

    public void Stop()
    {
      this.context.Stop();
    }

    public Dictionary<string, string>? TryGetRestoreData()
    {
      //TODO implement if required
      //throw new NotImplementedException();
      return new Dictionary<string, string>();
    }
  }
}
