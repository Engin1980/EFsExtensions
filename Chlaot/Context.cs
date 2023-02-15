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

    internal void InitModules()
    {
      LogHandler?.Invoke(LogLevel.INFO, "Loading modules...");
      IModule module;

      LogHandler?.Invoke(LogLevel.INFO, "Loading check-list module...");
      module = new Eng.Chlaot.Modules.ChecklistModule.ChecklistModule();
      module.Init((level, message) => this.LogHandler?.Invoke(level, $"[{module.Name}] {message}"));
      if (this.Modules.Any(q => q.Name == module.Name))
      {
        LogHandler?.Invoke(LogLevel.ERROR, $"Unable to add '{module.Name}' module. Module with this name already exists.");
      }
      else
        Modules.Add(module);

      LogHandler?.Invoke(LogLevel.INFO, "Modules loaded.");
    }
  }
}
