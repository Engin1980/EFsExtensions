using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils;
using EXmlLib;
using EXmlLib.Deserializers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Eng.Chlaot.Modules.AffinityModule
{
  public class Context : NotifyPropertyChangedBase
  {
    private const string SETTINGS_FILE = "affinity.sett.xml";

    private readonly Logger logger;

    private readonly Action<bool> setIsReadyFlagAction;

    private AffinityAdjuster? affinityAdjuster = null;

    private Timer? refreshTimer = null;

    public Context(Action<bool> setIsReadyFlagAction)
    {
      this.setIsReadyFlagAction = setIsReadyFlagAction
        ?? throw new ArgumentNullException(nameof(setIsReadyFlagAction));
      this.Settings = new();
      this.RuleBase = new();
      this.logger = Logger.Create(this);
    }

    public delegate void AdjustmentCompletedDelegate();
    public event AdjustmentCompletedDelegate? AdjustmentCompleted;
    public List<ProcessInfo> ProcessInfos { get; set; } = new();

    public RuleBase RuleBase
    {
      get => base.GetProperty<RuleBase>(nameof(RuleBase))!;
      set => base.UpdateProperty(nameof(RuleBase), value);
    }

    public MetaInfo MetaInfo
    {
      get => base.GetProperty<MetaInfo>(nameof(MetaInfo))!;
      set => base.UpdateProperty(nameof(MetaInfo), value);
    }

    public Settings Settings
    {
      get => base.GetProperty<Settings>(nameof(Settings))!;
      set => base.UpdateProperty(nameof(Settings), value);
    }
    public void LoadRuleBase(string xmlFile)
    {
      RuleBase tmp;
      MetaInfo tmpMeta;
      var factory = new XmlSerializerFactory();
      XDocument doc;

      try
      {
        logger.Invoke(LogLevel.INFO, $"Loading file '{xmlFile}'");
        try
        {
          doc = XDocument.Load(xmlFile);
          EXml<RuleBase> exml = Deserialization.CreateDeserializer();
          tmp = exml.Deserialize(doc);
          tmpMeta = MetaInfo.Deserialize(doc);
        }
        catch (Exception ex)
        {
          throw new ApplicationException($"Unable to load rule-base from {xmlFile}.", ex);
        }

        this.RuleBase = tmp;
        this.MetaInfo = tmpMeta;
        this.setIsReadyFlagAction(true);
      }
      catch (Exception ex)
      {
        this.setIsReadyFlagAction(false);
        logger.Invoke(LogLevel.ERROR, $"Failed to load checklist from '{xmlFile}'." + ex.GetFullMessage("\n\t"));
      }
    }

    public void LoadSettings()
    {
      this.logger.Invoke(LogLevel.VERBOSE, "Loading settings");
      try
      {
        this.Settings.Load(SETTINGS_FILE);
        this.logger.Invoke(LogLevel.INFO, "Settings saved");
      }
      catch (Exception ex)
      {
        this.logger.Invoke(LogLevel.ERROR, "Failed to load settings. " + ex.GetFullMessage("\n\t"));
      }
    }
    public void SaveSettings()
    {
      this.logger.Invoke(LogLevel.VERBOSE, "Saving settings");
      try
      {
        this.Settings.Save(SETTINGS_FILE);
        this.logger.Invoke(LogLevel.INFO, "Settings saved");
      }
      catch (Exception ex)
      {
        this.logger.Invoke(LogLevel.ERROR, "Failed to save settings. " + ex.GetFullMessage("\n\t"));
      }
    }
    internal void Run()
    {
      affinityAdjuster = new AffinityAdjuster(
        this.RuleBase.Rules, this.ProcessInfos, () => this.AdjustmentCompleted?.Invoke());
      affinityAdjuster.AdjustAffinityAsync();
      this.refreshTimer = new Timer(Settings.RefreshIntervalInSeconds * 1000)
      {
        AutoReset = true,
        Enabled = true
      };
      this.refreshTimer.Elapsed += RefreshTimer_Elapsed;
      this.refreshTimer.Start();
    }

    internal void Stop()
    {
      this.refreshTimer?.Stop();
      affinityAdjuster?.ResetAffinity();
    }

    private void RefreshTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
      affinityAdjuster!.AdjustAffinityAsync();
    }
  }
}
