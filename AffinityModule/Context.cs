using ChlaotModuleBase;
using ChlaotModuleBase.ModuleUtils.Storable;
using ELogging;
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
    private readonly Action<bool> setIsReadyFlagAction;
    private readonly Dictionary<Process, string> applicationResult = new();
    private AffinityAdjuster? affinityAdjuster = null;
    private Timer? refreshTimer = null;
    public BindingList<ProcessInfo> ProcessInfos { get; set; } = new();

    public Settings Settings
    {
      get => base.GetProperty<Settings>(nameof(Settings))!;
      set => base.UpdateProperty(nameof(Settings), value);
    }

    public RuleBase RuleBase
    {
      get => base.GetProperty<RuleBase>(nameof(RuleBase))!;
      set => base.UpdateProperty(nameof(RuleBase), value);
    }

    private readonly NewLogHandler logHandler;
    public Context(Action<bool> setIsReadyFlagAction)
    {
      this.setIsReadyFlagAction = setIsReadyFlagAction
        ?? throw new ArgumentNullException(nameof(setIsReadyFlagAction));
      this.Settings = new();
      this.RuleBase = new();
      this.logHandler = Logger.RegisterSender(this);
    }

    public void LoadSettings()
    {
      this.logHandler.Invoke(LogLevel.VERBOSE, "Loading settings");
      try
      {
        this.Settings.Load(SETTINGS_FILE);
        this.logHandler.Invoke(LogLevel.INFO, "Settings saved");
      }
      catch (Exception ex)
      {
        this.logHandler.Invoke(LogLevel.ERROR, "Failed to load settings. " + ex.GetFullMessage("\n\t"));
      }
    }
    public void SaveSettings()
    {
      this.logHandler.Invoke(LogLevel.VERBOSE, "Saving settings");
      try
      {
        this.Settings.Save(SETTINGS_FILE);
        this.logHandler.Invoke(LogLevel.INFO, "Settings saved");
      }
      catch (Exception ex)
      {
        this.logHandler.Invoke(LogLevel.ERROR, "Failed to save settings. " + ex.GetFullMessage("\n\t"));
      }
    }

    public void LoadRuleBase(string xmlFile)
    {
      RuleBase tmp;
      var factory = new XmlSerializerFactory();
      XDocument doc;

      try
      {
        logHandler.Invoke(LogLevel.INFO, $"Loading file '{xmlFile}'");
        try
        {
          doc = XDocument.Load(xmlFile);
          EXml<RuleBase> exml = CreateDeserializer();
          tmp = exml.Deserialize(doc);
        }
        catch (Exception ex)
        {
          throw new ApplicationException($"Unable to load rule-base from {xmlFile}.", ex);
        }

        this.RuleBase = tmp;
        this.setIsReadyFlagAction(true);
      }
      catch (Exception ex)
      {
        this.setIsReadyFlagAction(false);
        logHandler.Invoke(LogLevel.ERROR, $"Failed to load checklist from '{xmlFile}'." + ex.GetFullMessage("\n\t"));
      }
    }

    private EXml<RuleBase> CreateDeserializer()
    {
      EXml<RuleBase> ret = new();

      ObjectElementDeserializer oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(RuleBase))
        .WithCustomPropertyDeserialization(
        nameof(RuleBase.Rules),
        (e, t, p, c) =>
        {
          var des = c.ResolveElementDeserializer(typeof(Rule));

          var lst = e.LElements("rule")
            .Select(q => des.Deserialize(q, typeof(Rule), c))
            .Cast<Rule>()
            .ToList();

          SafeUtils.SetPropertyValue(p, t, lst);
        });
      ret.Context.ElementDeserializers.Insert(0, oed);

      oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(Rule))
        .WithIgnoredProperty(nameof(Rule.CoreFlags));
      ret.Context.ElementDeserializers.Insert(0, oed);

      return ret;
    }

    internal void Run()
    {
      affinityAdjuster = new AffinityAdjuster(
        this.RuleBase.Rules, this.ProcessInfos);
      affinityAdjuster.AdjustAffinityAsync();
      this.refreshTimer = new Timer(Settings.RefreshIntervalInSeconds * 1000)
      {
        AutoReset = true,
        Enabled = true
      };
      this.refreshTimer.Elapsed += RefreshTimer_Elapsed;
      this.refreshTimer.Start();
    }

    private void RefreshTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
      affinityAdjuster!.AdjustAffinityAsync();
    }

    internal void Stop()
    {
      this.refreshTimer?.Stop();
      affinityAdjuster?.ResetAffinity();
    }
  }
}
