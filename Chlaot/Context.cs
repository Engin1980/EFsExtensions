using ChlaotModuleBase;
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
    private LogHandler? LogHandler { get; set; }

    internal void SetLogHandler(LogHandler logHandler)
    {
      this.LogHandler = logHandler;
    }

    internal void SetUpModules()
    {
      LogHandler?.Invoke(LogLevel.INFO, "Loading modules...");
      IModule module;

      LogHandler?.Invoke(LogLevel.INFO, "Loading check-list module...");
      module = new Eng.Chlaot.Modules.ChecklistModule.ChecklistModule();
      TryAddModule(module);


      LogHandler?.Invoke(LogLevel.INFO, "Setting up modules...");
      Modules.ToList().ForEach(q => q.SetUp(
        (level, message) => this.LogHandler?.Invoke(level, $"[{module.Name}] {message}")
        ));

      LogHandler?.Invoke(LogLevel.INFO, "Modules loaded & set up.");
    }

    internal void InitModules()
    {
      LogHandler?.Invoke(LogLevel.INFO, "Initializing modules");
      Modules.ToList().ForEach(q => q.Init());
      LogHandler?.Invoke(LogLevel.INFO, "Modules initialized");
    }

    private void TryAddModule(IModule module)
    {
      if (this.Modules.Any(q => q.Name == module.Name))
      {
        LogHandler?.Invoke(LogLevel.ERROR, $"Unable to add '{module.Name}' module. Module with this name already exists.");
      }
      else
        Modules.Add(module);
    }

    internal void RunModules()
    {
      LogHandler?.Invoke(LogLevel.INFO, "Starting modules");
      Modules.ToList().ForEach(q => q.Run());
      LogHandler?.Invoke(LogLevel.INFO, "Modules running");
    }

    internal void RemoveUnreadyModules()
    {
      var modules = Modules.Where(q => !q.IsReady).ToList();
      foreach (var module in modules)
      {
        LogHandler?.Invoke(LogLevel.INFO, $"Module '{module.Name}' removed as it is not-ready.");
        Modules.Remove(module);
      }
    }
  }
}
