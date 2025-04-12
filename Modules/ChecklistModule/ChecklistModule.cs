using ESystem.Logging;
using Eng.EFsExtensions.EFsExtensionsModuleBase;
using ESystem;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static Eng.EFsExtensions.EFsExtensionsModuleBase.IModule;

namespace Eng.EFsExtensions.Modules.ChecklistModule
{
  public class ChecklistModule : NotifyPropertyChanged, IModule
  {
    private Control? _InitControl;
    public Control InitControl => _InitControl ?? throw new ApplicationException("Control not provided.");

    private Control? _RunControl;
    public Control RunControl => _RunControl ?? throw new ApplicationException("Control not provided.");

    private InitContext? initContext;
    private RunContext? runContext;
    private readonly Logger logger;

    private Settings? settings;

    public string Name => "Check-lists";

    public bool IsReady
    {
      get => base.GetProperty<bool>(nameof(IsReady))!;
      private set => base.UpdateProperty(nameof(IsReady), value);
    }

    public ChecklistModule()
    {
      this.IsReady = false;
      this.logger = Logger.Create("EFSE.Modules.Checklist");
    }


    public void SetUp(ModuleSetUpInfo setUpInfo)
    {
      try
      {
        settings = Settings.Load();
        logger.Log(LogLevel.INFO, "Settings loaded.");
      }
      catch (Exception ex)
      {
        logger.Log(LogLevel.ERROR, "Unable to load settings. " + ex.GetFullMessage());
        settings = new Settings();
      }
    }

    public void Init()
    {
      this.initContext = new InitContext(this.settings!, q => this.IsReady = q);
      this._InitControl = new CtrInit(this.initContext);
    }

    public void Run()
    {
      this.runContext = new(this.initContext!);
      this._RunControl = new CtrRun(this.runContext);

      this.initContext = null;
      this._InitControl = null;
    }

    public void Stop()
    {
      this.runContext?.Stop();
    }

    public Dictionary<string, string>? TryGetRestoreData()
    {
      if (initContext != null && initContext.LastLoadedFile != null)
        return new Dictionary<string, string> { { "fileName", initContext.LastLoadedFile } };
      else
        return null;
    }

    public void Restore(Dictionary<string, string> restoreData)
    {
      try
      {
        string xmlName = restoreData["fileName"];
        initContext!.LoadFile(xmlName);
      }
      catch (Exception ex)
      {
        throw new ApplicationException("Failed to restore.", ex);
      }
    }
  }
}
