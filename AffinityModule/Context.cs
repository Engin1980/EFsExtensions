using ChlaotModuleBase;
using ChlaotModuleBase.ModuleUtils.Storable;
using ELogging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.AffinityModule
{
  public class Context : NotifyPropertyChangedBase
  {
    public Settings Settings
    {
      get => base.GetProperty<Settings>(nameof(Settings))!;
      set => base.UpdateProperty(nameof(Settings), value);
    }

    private readonly NewLogHandler logHandler;
    public Context()
    {
      this.Settings = Settings.CreateDefault();
      this.logHandler = Logger.RegisterSender(this);
    }

    public void LoadSettings()
    {
      this.logHandler.Invoke(LogLevel.VERBOSE, "Loading rules");
      try
      {
        this.Settings.Load();
        this.logHandler.Invoke(LogLevel.INFO, "Rules loaded");
      }
      catch (Exception ex)
      {
        this.logHandler.Invoke(LogLevel.ERROR, "Failed to load rules. " + ex.GetFullMessage("\n\t"));
      }
    }
    public void SaveSettings()
    {
      this.logHandler.Invoke(LogLevel.VERBOSE, "Saving rules");
      try
      {
        this.Settings.Save();
        this.logHandler.Invoke(LogLevel.INFO, "Rules saved");
      }
      catch (Exception ex)
      {
        this.logHandler.Invoke(LogLevel.ERROR, "Failed to save rules. " + ex.GetFullMessage("\n\t"));
      }
    }
  }
}
