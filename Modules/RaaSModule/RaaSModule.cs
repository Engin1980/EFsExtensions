﻿using ESystem.Logging;
using Eng.EFsExtensions.EFsExtensionsModuleBase;
using ESystem;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Eng.EFsExtensions.Modules.RaaSModule
{
  public class RaaSModule : NotifyPropertyChanged, IModule
  {
    private readonly Context context;
    private readonly Logger logger = Logger.Create("EFSE.Modules.RaaS");
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
      this.context = new Context(this.logger, q => this.IsReady = q);
    }

    public void Init()
    {

      this.ctrInit = new(this.context);
      try
      {
        if (this.context.Settings.AutoLoadedRaasFile != null)
        {
          this.context.LoadRaasFile(this.context.Settings.AutoLoadedRaasFile);
          logger.Log(LogLevel.INFO, "Default RaaS loaded.");
        }
      }
      catch
      {
        logger.Log(LogLevel.ERROR, "Unable to load RaaS file.");
      }
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
      try
      {
        this.context.Settings = Settings.Load();
        logger.Log(LogLevel.INFO, "Settings loaded.");
      }
      catch (Exception ex)
      {
        logger.Log(LogLevel.ERROR, "Unable to load settings. " + ex.GetFullMessage());
        logger.Log(LogLevel.INFO, "Default settings used.");
      }

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
