﻿using ESystem.Logging;
using Eng.EFsExtensions.EFsExtensionsModuleBase;
using Eng.EFsExtensions.Modules.ChecklistModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.App
{
  public class Context
  {
    public BindingList<IModule> Modules { get; private set; } = new BindingList<IModule>();
    private readonly Logger logger;

    public Context()
    {
      this.logger = Logger.Create(this);
    }

    internal void SetUpModules()
    {
      logger.Log(LogLevel.INFO, "Loading modules...");
      IModule module;

      logger.Log(LogLevel.INFO, "Loading check-list module...");
      module = new Eng.EFsExtensions.Modules.ChecklistModule.ChecklistModule();
      TryAddModule(module);

      logger.Log(LogLevel.INFO, "Loading copilot module...");
      module = new Eng.EFsExtensions.Modules.CopilotModule.CopilotModule();
      TryAddModule(module);

      logger.Log(LogLevel.INFO, "Loading failures module...");
      module = new Eng.EFsExtensions.Modules.FailuresModule.FailuresModule();
      TryAddModule(module);

      logger.Log(LogLevel.INFO, "Loading RaaS module...");
      module = new Eng.EFsExtensions.Modules.RaaSModule.RaaSModule();
      TryAddModule(module);

      logger.Log(LogLevel.INFO, "Loading sim-var-test module...");
      module = new Eng.EFsExtensions.Modules.SimVarTestModule.SimVarTestModule();
      TryAddModule(module);

      logger.Log(LogLevel.INFO, "Loading affinity module...");
      module = new Eng.EFsExtensions.Modules.AffinityModule.AffinityModule();
      TryAddModule(module);

      logger.Log(LogLevel.INFO, "Loading flight-log module...");
      module = new Eng.EFsExtensions.Modules.FlightLogModule.FlightLogModule();
      TryAddModule(module);

      logger.Log(LogLevel.INFO, "Setting up modules...");
      Modules.ToList().ForEach(q => q.SetUp(
        new ModuleSetUpInfo()));

      logger.Log(LogLevel.INFO, "Modules loaded & set up.");
    }

    internal void InitModules()
    {
      logger.Log(LogLevel.INFO, "Initializing modules");
      Modules.ToList().ForEach(q => q.Init());
      logger.Log(LogLevel.INFO, "Modules initialized");
    }

    private void TryAddModule(IModule module)
    {
      if (this.Modules.Any(q => q.Name == module.Name))
      {
        logger.Log(LogLevel.ERROR, $"Unable to add '{module.Name}' module. Module with this name already exists.");
      }
      else
        Modules.Add(module);
    }

    internal void RunModules()
    {
      logger.Log(LogLevel.INFO, "Starting modules");
      Modules.ToList().ForEach(q => q.Run());
      logger.Log(LogLevel.INFO, "Modules running");
    }

    internal void RemoveUnreadyModules()
    {
      var modules = Modules.Where(q => !q.IsReady).ToList();
      foreach (var module in modules)
      {
        logger.Log(LogLevel.INFO, $"Module '{module.Name}' removed as it is not-ready.");
        Modules.Remove(module);
      }
    }
  }
}
