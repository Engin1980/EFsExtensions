using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.Modules.ChecklistModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chlaot
{
  public class Context
  {
    public BindingList<IModule> Modules { get; private set; } = new BindingList<IModule>();
    private readonly NewLogHandler logHandler;

    public Context()
    {
      this.logHandler = Logger.RegisterSender(this);
    }

    internal void SetUpModules()
    {
      logHandler.Invoke(LogLevel.INFO, "Loading modules...");
      IModule module;

      logHandler.Invoke(LogLevel.INFO, "Loading check-list module...");
      module = new Eng.Chlaot.Modules.ChecklistModule.ChecklistModule();
      TryAddModule(module);

      logHandler.Invoke(LogLevel.INFO, "Loading copilot module...");
      module = new Eng.Chlaot.Modules.CopilotModule.CopilotModule();
      TryAddModule(module);

      logHandler.Invoke(LogLevel.INFO, "Loading affinity module...");
      module = new Eng.Chlaot.Modules.AffinityModule.AffinityModule();
      TryAddModule(module);

      logHandler.Invoke(LogLevel.INFO, "Loading failures module...");
      module = new Eng.Chlaot.Modules.FailuresModule.FailuresModule();
      TryAddModule(module);

      logHandler.Invoke(LogLevel.INFO, "Loading sim-var-test module...");
      module = new Eng.Chlaot.Modules.SimVarTestModule.SimVarTestModule();
      TryAddModule(module);


      logHandler.Invoke(LogLevel.INFO, "Setting up modules...");
      Modules.ToList().ForEach(q => q.SetUp(
        new ModuleSetUpInfo()));

      logHandler.Invoke(LogLevel.INFO, "Modules loaded & set up.");
    }

    internal void InitModules()
    {
      logHandler.Invoke(LogLevel.INFO, "Initializing modules");
      Modules.ToList().ForEach(q => q.Init());
      logHandler.Invoke(LogLevel.INFO, "Modules initialized");
    }

    private void TryAddModule(IModule module)
    {
      if (this.Modules.Any(q => q.Name == module.Name))
      {
        logHandler.Invoke(LogLevel.ERROR, $"Unable to add '{module.Name}' module. Module with this name already exists.");
      }
      else
        Modules.Add(module);
    }

    internal void RunModules()
    {
      logHandler.Invoke(LogLevel.INFO, "Starting modules");
      Modules.ToList().ForEach(q => q.Run());
      logHandler.Invoke(LogLevel.INFO, "Modules running");
    }

    internal void RemoveUnreadyModules()
    {
      var modules = Modules.Where(q => !q.IsReady).ToList();
      foreach (var module in modules)
      {
        logHandler.Invoke(LogLevel.INFO, $"Module '{module.Name}' removed as it is not-ready.");
        Modules.Remove(module);
      }
    }
  }
}
