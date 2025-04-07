using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Eng.EFsExtensions.EFsExtensionsModuleBase;
using Eng.EFsExtensions.Libs.AirportsLib;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  public class FlightLogModule : NotifyPropertyChanged, IModule
  {
    private readonly Settings settings = new();

    public bool IsReady
    {
      get { return base.GetProperty<bool>(nameof(IsReady))!; }
      set { base.UpdateProperty(nameof(IsReady), value); }
    }

    public InitContext InitContext
    {
      get => GetProperty<InitContext>(nameof(InitContext))!;
      set => UpdateProperty(nameof(InitContext), value);
    }

    public RunContext RunContext
    {
      get => GetProperty<RunContext>(nameof(RunContext))!;
      set => UpdateProperty(nameof(RunContext), value);
    }

    public Control InitControl { get; private set; } = null!;

    public Control RunControl { get; private set; } = null!;

    public string Name => "Flight Module";

    public void Init()
    {
      this.InitContext = new InitContext(this.settings, q => this.IsReady = q);
      InitControl = new CtrInit(InitContext);
    }

    public void Restore(Dictionary<string, string> restoreData)
    {
      //throw new NotImplementedException();
    }

    public void Run()
    {
      var runContext = new RunContext(this.InitContext, this.settings);
      this.RunControl = new CtrRun(runContext);

      runContext.Start();
    }

    public void SetUp(ModuleSetUpInfo setUpInfo)
    {
      //throw new NotImplementedException();
    }

    public void Stop()
    {
      //throw new NotImplementedException();
    }

    public Dictionary<string, string>? TryGetRestoreData()
    {
      return null; //TODO
    }
  }
}
