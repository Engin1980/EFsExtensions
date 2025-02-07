using AffinityModule;
using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Eng.Chlaot.Modules.AffinityModule
{
  public class AffinityModule : NotifyPropertyChanged, IModule
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

    public string Name => "Affi + Prio";

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

    public Dictionary<string, string>? TryGetRestoreData()
    {
      Dictionary<string, string>? ret;
      if (this.context.LastLoadedFileName != null)
        ret = new Dictionary<string, string> { { "fileName", this.context.LastLoadedFileName } };
      else
        ret = null;
      return ret;
    }

    public void Restore(Dictionary<string, string> restoreData)
    {
      try
      {
        string file = restoreData["fileName"];
        this.context.LoadRuleBase(file);
      }
      catch (Exception ex)
      {
        throw new ApplicationException("Failed to restore.", ex);
      }
    }
  }
}
