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

    public void Restore(Dictionary<string, string> restoreData)
    {
      try
      {
        string state = restoreData["state"];
        switch (state)
        {
          case "null":
            this.Context.IsEnabled = null;
            break;
          case "true":
            this.Context.IsEnabled = true;
            break;
          case "false":
            this.Context.IsEnabled = false;
            break;
        }
      }
      catch (Exception ex)
      {
        throw new ApplicationException("Failed to restore.", ex);
      }
    }

    public Dictionary<string, string>? TryGetRestoreData()
    {
      if (this.Context == null)
        return null;
      else
      {
        string state;
        if (this.Context.IsEnabled == null)
          state = "null";
        else if (this.Context.IsEnabled.Value)
          state = "true";
        else
          state = "false";
        return new Dictionary<string, string> { { "state", state } };
      }
    }
  }
}
