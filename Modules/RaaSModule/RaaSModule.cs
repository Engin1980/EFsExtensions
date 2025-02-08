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
    private readonly Context context = new();
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

    public void Init()
    {
      this.ctrInit = new(this.context, () => this.IsReady = true);
    }

    public void Restore(Dictionary<string, string> restoreData)
    {
      throw new NotImplementedException();
    }

    public void Run()
    {
      throw new NotImplementedException();
    }

    public void SetUp(ModuleSetUpInfo setUpInfo)
    {
      this.context.Airports = new();

    }

    public void Stop()
    {
      throw new NotImplementedException();
    }

    public Dictionary<string, string>? TryGetRestoreData()
    {
      throw new NotImplementedException();
    }
  }
}
