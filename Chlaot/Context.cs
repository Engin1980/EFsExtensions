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
    public delegate void LogDelegate(LogLevel level, string message);
    public LogDelegate Log { get; set; }

    public BindingList<IModule> Modules { get; private set; } = new BindingList<IModule>();

    public Context(LogDelegate logFunction)
    {
      this.Log = logFunction;
    }

    internal void InitModules()
    {
      Log(LogLevel.INFO, "Loading modules...");
      IModule module;

      Log(LogLevel.INFO, "Loading check-list module...");
      module = new Eng.Chlaot.Modules.ChecklistModule.ChecklistModule();
      if (this.Modules.Any(q => q.Name == module.Name))
      {
        Log(LogLevel.ERROR, $"Unable to add '{module.Name}' module. Module with this name already exists.");
      }
      else
        Modules.Add(module);

      Log(LogLevel.INFO, "Modules loaded.");
    }
  }
}
