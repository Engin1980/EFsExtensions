using Eng.Chlaot.ChlaotModuleBase;
using EXmlLib;
using EXmlLib.Deserializers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Reflection.Metadata.Ecma335;
using System.Security.Policy;
using ELogging;
using Eng.Chlaot.Modules.ChecklistModule.Types;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Synthetization;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.StateModel;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils;
using Eng.Chlaot.Modules.ChecklistModule.Types.Xml;
using static ESystem.Functions;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimObjects;
using static ChlaotModuleBase.ModuleUtils.StateChecking.StateCheckUtils;
using ChlaotModuleBase.ModuleUtils.StateChecking;
using static System.Net.WebRequestMethods;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel;
using ESystem;

namespace Eng.Chlaot.Modules.ChecklistModule
{
  public class InitContext : NotifyPropertyChangedBase
  {
    private readonly Logger logger;
    private readonly Action<bool> setIsReadyFlagAction;

    public MetaInfo MetaInfo
    {
      get => base.GetProperty<MetaInfo>(nameof(MetaInfo))!;
      set => base.UpdateProperty(nameof(MetaInfo), value);
    }

    public CheckSet ChecklistSet
    {
      get => base.GetProperty<CheckSet>(nameof(ChecklistSet))!;
      set => base.UpdateProperty(nameof(ChecklistSet), value);
    }

    public SimPropertyGroup SimPropertyGroup
    {
      get => base.GetProperty<SimPropertyGroup>(nameof(SimPropertyGroup))!;
      set => base.UpdateProperty(nameof(SimPropertyGroup), value);
    }

    public record PropertyUsageCount(SimProperty Property, int Count);

    public List<PropertyUsageCount> PropertyUsageCounts
    {
      get => base.GetProperty<List<PropertyUsageCount>>(nameof(PropertyUsageCounts))!;
      set => base.UpdateProperty(nameof(PropertyUsageCounts), value);
    }

    public Settings Settings { get; private set; } = null!;

    public InitContext(Settings settings, Action<bool> setIsReadyFlagAction)
    {
      Settings = settings ?? throw new ArgumentNullException(nameof(settings));
      this.logger = Logger.Create(this, "Checklist.InitContext");
      this.setIsReadyFlagAction = setIsReadyFlagAction ?? throw new ArgumentNullException(nameof(setIsReadyFlagAction));
      this.SimPropertyGroup = LoadDefaultSimProperties();
    }

    private SimPropertyGroup LoadDefaultSimProperties()
    {
      SimPropertyGroup ret;
      try
      {
        XDocument doc = XDocument.Load(@"Xmls\SimProperties.xml", LoadOptions.SetLineInfo);
        ret = SimPropertyGroup.Deserialize(doc.Root!);
      }
      catch (Exception ex)
      {
        throw new ApplicationException("Failed to load global sim properties.", ex);
      }
      return ret;
    }

    internal void LoadFile(string xmlFile)
    {
      CheckSet tmp;
      MetaInfo tmpMeta;
      SimPropertyGroup? tmpSpg = null;

      try
      {
        logger.Invoke(LogLevel.INFO, $"Loading file '{xmlFile}'");
        try
        {
          XDocument doc = XDocument.Load(xmlFile);
          tmpMeta = MetaInfo.Deserialize(doc);
          if (doc.Root!.LElement("properties") is XElement pelm)
            // workaround due to WPF binding refresh
            tmpSpg = SimPropertyGroup.Deserialize(pelm);
          tmp = Deserializer.Deserialize(doc);
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Unable to read/deserialize checklist-set from '{xmlFile}'. Invalid file content?", ex);
        }

        logger.Invoke(LogLevel.INFO, $"Checking sanity");
        var props = tmpSpg == null
          ? this.SimPropertyGroup.GetAllSimPropertiesRecursively()
          : tmpSpg.GetAllSimPropertiesRecursively().Union(this.SimPropertyGroup.GetAllSimPropertiesRecursively()).ToList();
        Try(() => CheckSanity(tmp, props), ex => new ApplicationException("Error loading checklist.", ex));

        logger.Invoke(LogLevel.INFO, $"Binding checklist references");
        Try(() => BindNextChecklists(tmp), ex => new ApplicationException("Error binding checklist references.", ex));

        logger.Invoke(LogLevel.INFO, $"Loading/generating sounds");
        Try(() => InitializeSoundStreams(tmp, System.IO.Path.GetDirectoryName(xmlFile)!),
          ex => new ApplicationException("Error creating sound streams for checklist.", ex));


        if (tmpSpg != null)
        {
          var spg = new SimPropertyGroup();
          spg.Properties.AddRange(this.SimPropertyGroup.Properties);
          spg.Properties.Add(tmpSpg);
          this.SimPropertyGroup = spg;
        }
        this.PropertyUsageCounts = GetPropertyUsagesCounts(tmp, this.SimPropertyGroup.GetAllSimPropertiesRecursively());
        this.ChecklistSet = tmp;
        this.MetaInfo = tmpMeta;
        this.setIsReadyFlagAction(true);
        logger.Invoke(LogLevel.INFO, $"Checklist file '{xmlFile}' successfully loaded.");
      }
      catch (Exception ex)
      {
        this.setIsReadyFlagAction(false);
        logger.Invoke(LogLevel.ERROR, $"Failed to load checklist from '{xmlFile}'." + ex.GetFullMessage("\n\t"));
      }
    }

    private List<PropertyUsageCount> GetPropertyUsagesCounts(CheckSet tmp, List<SimProperty> simProperties)
    {
      Dictionary<string, int> dct = new();

      foreach (var item in tmp.Checklists.Where(q => q.Trigger != null))
      {
        var pus = StateCheckUtils.ExtractProperties(item.Trigger!);
        foreach (var pu in pus)
        {
          var pn = pu.PropertyName;
          if (dct.ContainsKey(pn))
            dct[pn]++;
          else
            dct[pn] = 1;
        }
      }

      List<PropertyUsageCount> ret = dct
        .Select(q => new PropertyUsageCount(simProperties.First(p => p.Name == q.Key), q.Value))
        .OrderBy(q => q.Property.Name)
        .ToList();
      return ret;
    }

    private static void BindNextChecklists(CheckSet tmp)
    {
      for (int i = 0; i < tmp.Checklists.Count; i++)
      {
        var checklist = tmp.Checklists[i];
        if (checklist.NextChecklistId is null)
        {
          if (i < tmp.Checklists.Count - 1)
            checklist.NextChecklist = tmp.Checklists[i + 1];
          else
            checklist.NextChecklist = tmp.Checklists[0];
        }
        else
          checklist.NextChecklist = tmp.Checklists.Single(q => q.Id == checklist.NextChecklistId);
      }
    }

    private static void CheckSanity(CheckSet tmp, List<SimProperty> definedProperties)
    {
      // check no duplicit
      var ids = tmp.Checklists.Select(q => q.Id);
      var dids = ids.Distinct();
      var exc = ids.Except(dids);
      if (exc.Any())
        throw new ApplicationException("There are repeated checklist id definitions: " + string.Join(", ", exc));

      // all property conditions has values
      IStateCheckItem[] triggers = tmp.Checklists.Where(q => q.Trigger != null).Select(q => q.Trigger).ToArray()!;
      List<StateCheckProperty> triggerProps = StateCheckUtils.ExtractStateCheckProperties(triggers);
      triggerProps
        .Where(q => q.Expression == null)
        .ForEach(q => throw new ApplicationException($"Expression of checked property {q.DisplayString} not set."));

      // check all properties are defined
      List<PropertyUsage> propertyUsages = StateCheckUtils.ExtractProperties(triggers);
      List<string> missingProperties = propertyUsages
        .Where(q => definedProperties.None(p => p.Name == q.PropertyName))
        .Select(q => q.PropertyName)
        .Distinct()
        .ToList();
      if (missingProperties.Any())
        throw new ApplicationException($"Required properties not found in defined properties: {string.Join(", ", missingProperties)}");

      // check all variables are defined
      List<string> missingVariables = new();
      foreach (var item in tmp.Checklists.Where(q => q.Trigger != null))
      {
        List<Variable> definedVariables = item.Variables;
        List<VariableUsage> variableUsages = StateCheckUtils.ExtractVariables(item.Trigger!);
        var vmp = variableUsages
          .Where(q => definedVariables.None(v => v.Name == q.VariableName))
          .Select(q => q.VariableName)
          .ToList();
        missingVariables.AddRange(vmp);
      }
      if (missingVariables.Any())
        throw new ApplicationException($"Required variables not found in defined variables: {string.Join(", ", missingVariables.Distinct())}.");
    }

    private void InitializeSoundStreams(CheckSet checkSet, string relativePath)
    {
      Synthetizer synthetizer = new(Settings.Synthetizer);
      Dictionary<string, byte[]> generatedSounds = new();
      foreach (var checklist in checkSet.Checklists)
      {
        // TODO correct load meta data and checklist entry/exit speeches
        InitializeSoundStreamsForChecklist(checklist, generatedSounds, synthetizer, relativePath);

        foreach (var item in checklist.Items)
        {
          InitializeSoundStreamsForItems(item.Call, generatedSounds, synthetizer, relativePath);
          InitializeSoundStreamsForItems(item.Confirmation, generatedSounds, synthetizer, relativePath);
        }
      }
    }

    private void InitializeSoundStreamsForChecklist(
      CheckList checklist,
      Dictionary<string, byte[]> generatedSounds,
      Synthetizer synthetizer,
      string relativePath)
    {
      if (checklist.CustomEntrySpeech != null)
        InitializeSoundStreamsForItems(checklist.CustomEntrySpeech, generatedSounds, synthetizer, relativePath);
      if (checklist.CustomExitSpeech != null)
        InitializeSoundStreamsForItems(checklist.CustomExitSpeech, generatedSounds, synthetizer, relativePath);

      checklist.EntrySpeechBytes =
        checklist.CustomEntrySpeech != null
        ? checklist.CustomEntrySpeech.Bytes
        : synthetizer.Generate($"{checklist.CallSpeech} checklist");
      checklist.ExitSpeechBytes =
        checklist.CustomExitSpeech != null
        ? checklist.CustomExitSpeech.Bytes
        : synthetizer.Generate($"{checklist.CallSpeech} checklist completed");
    }

    private void InitializeSoundStreamsForItems(
      CheckDefinition checkDefinition,
      Dictionary<string, byte[]> generatedSounds,
      Synthetizer synthetizer,
      string relativePath)
    {
      if (checkDefinition.Type == CheckDefinition.CheckDefinitionType.File)
        try
        {
          checkDefinition.Bytes = System.IO.File.ReadAllBytes(
            System.IO.Path.Combine(relativePath, checkDefinition.Value));
        }
        catch (Exception ex)
        {
          throw new EXmlException($"Unable to load sound file '{checkDefinition.Value}'.", ex);
        }
      else if (checkDefinition.Type == CheckDefinition.CheckDefinitionType.Speech)
        try
        {
          if (generatedSounds.ContainsKey(checkDefinition.Value))
            checkDefinition.Bytes = generatedSounds[checkDefinition.Value];
          else
          {
            checkDefinition.Bytes = synthetizer.Generate(checkDefinition.Value);
            generatedSounds[checkDefinition.Value] = checkDefinition.Bytes;
          }
        }
        catch (Exception ex)
        {
          throw new EXmlException($"Unable to generated sound for speech '{checkDefinition.Value}'.", ex);
        }
      else
        throw new NotImplementedException($"Unknown type {checkDefinition.Type}.");
    }
  }
}
