using AffinityModule;
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
    private ProcessAdjuster? processAdjuster = null;
    private Timer? refreshTimer = null;
    public delegate void AdjustmentCompletedDelegate();
    public event AdjustmentCompletedDelegate? AdjustmentCompleted;

    public Context(Action<bool> setIsReadyFlagAction)
    {
      this.setIsReadyFlagAction = setIsReadyFlagAction
        ?? throw new ArgumentNullException(nameof(setIsReadyFlagAction));
      this.RuleBase = new();
      this.logger = Logger.Create(this);
    }

    public List<ProcessAdjustResult> ProcessInfos { get; set; } = new();

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

    internal void Run()
    {
      processAdjuster = new ProcessAdjuster(
        this.RuleBase.AffinityRules, this.RuleBase.PriorityRules, () => this.AdjustmentCompleted?.Invoke());
      processAdjuster.AdjustAsync();
      this.refreshTimer = new Timer(this.RuleBase.ResetIntervalInS * 1000)
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
      this.processAdjuster?.ResetAsync();
    }

    private void RefreshTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
      processAdjuster!.AdjustAsync();
    }
  }
}
