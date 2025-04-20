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
using Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog;
using ESystem.Logging;
using ESystem;
using System.Windows;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  public class FlightLogModule : NotifyPropertyChanged, IModule
  {
    private Settings settings = new();
    private readonly Logger logger = Logger.Create("EFSE.Modules.FlightLog");

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
      InitControl = new CtrInit(InitContext, this.settings);

      // map Settings changes to converters
      LongDistanceConverter.DefaultUnit = this.settings.LongDistanceUnit;
      ShortDistanceConverter.DefaultUnit = this.settings.ShortDistanceUnit;
      WeightConverter.DefaultUnit = this.settings.WeightUnit;
      SpeedConverter.DefaultUnit = this.settings. SpeedUnit;
      MapSettingsToConverters();
    }

    private void MapSettingsToConverters()
    {
      this.settings.PropertyChanged += (s, e) =>
      {
        bool refreshNeeded = false;
        if (e.PropertyName == nameof(Settings.LongDistanceUnit))
        {
          LongDistanceConverter.DefaultUnit = settings.LongDistanceUnit;
          refreshNeeded = true;
        }
        if (e.PropertyName == nameof(Settings.ShortDistanceUnit))
        {
          ShortDistanceConverter.DefaultUnit = settings.ShortDistanceUnit;
          refreshNeeded = true;
        }
        if (e.PropertyName == nameof(Settings.WeightUnit))
        {
          WeightConverter.DefaultUnit = settings.WeightUnit;
          refreshNeeded = true;
        }
        if (e.PropertyName == nameof(Settings.SpeedUnit))
        {
          SpeedConverter.DefaultUnit = settings.SpeedUnit;
          refreshNeeded = true;
        }

        if (refreshNeeded)
          this.UpdateBindings();
      };
    }

    private void UpdateBindings()
    {
      this.InitControl?.RefreshBindings();
      this.RunControl?.RefreshBindings();
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
      try
      {
        settings = Settings.Load();
        logger.Log(LogLevel.INFO, "Settings loaded.");
      }
      catch (Exception ex)
      {
        logger.Log(LogLevel.ERROR, "Unable to load settings. " + ex.GetFullMessage());
        logger.Log(LogLevel.INFO, "Default settings used.");
        settings = new Settings();
      }
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
