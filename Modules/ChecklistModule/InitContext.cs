﻿using Eng.EFsExtensions.EFsExtensionsModuleBase;
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
using Eng.EFsExtensions.Modules.ChecklistModule.Types;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking.StateModel;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils;
using Eng.EFsExtensions.Modules.ChecklistModule.Types.Xml;
using static ESystem.Functions.TryCatch;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects;

using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking.VariableModel;
using ESystem;
using Eng.EFsExtensions.Modules.ChecklistModule.Types.VM;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.VMs;
using static Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking.StateCheckUtils;
using ESystem.Miscelaneous;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs.MsSapi;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.AudioPlaying;
using ESystem.Exceptions;
using ESystem.Logging;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.Globals;

namespace Eng.EFsExtensions.Modules.ChecklistModule
{
  public class InitContext : NotifyPropertyChanged
  {
    private readonly Logger logger;
    private readonly Action<bool> setIsReadyFlagAction;
    public string LastLoadedFile { get; private set; }

    public MetaInfo MetaInfo
    {
      get => base.GetProperty<MetaInfo>(nameof(MetaInfo))!;
      set => base.UpdateProperty(nameof(MetaInfo), value);
    }

    public List<CheckListVM> CheckListVMs
    {
      get => base.GetProperty<List<CheckListVM>>(nameof(CheckListVMs))!;
      set => base.UpdateProperty(nameof(CheckListVMs), value);
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
      this.SimPropertyGroup = GlobalProvider.Instance.SimPropertyGroup;
    }

    public void RebuildSoundStreams()
    {
      if (LastLoadedFile == null) return;
      var tmp = this.CheckListVMs.Select(q => q.CheckList).ToList();
      InitializeSoundStreams(
        tmp,
        System.IO.Path.GetDirectoryName(LastLoadedFile) ?? throw new UnexpectedNullException());
    }

    internal void LoadFile(string xmlFile)
    {
      CheckSet tmp;
      MetaInfo tmpMeta;
      SimPropertyGroup? tmpSpg = null;

      try
      {
        logger.Invoke(LogLevel.INFO, $"Checking file '{xmlFile}'");
        try
        {
          XmlUtils.ValidateXmlAgainstXsd(xmlFile, new string[] { @".\xmls\xsds\Global.xsd", @".\xmls\xsds\ChecklistSchema.xsd" }, out List<string> errors);
          if (errors.Any())
            throw new ApplicationException("XML does not match XSD: " + string.Join("; ", errors.Take(5)));
        }
        catch (Exception ex)
        {
          throw new ApplicationException($"Failed to validate XMl file against XSD. Error: " + ex.Message, ex);
        }

        logger.Invoke(LogLevel.INFO, $"Loading file '{xmlFile}'");
        try
        {
          XDocument doc = XDocument.Load(xmlFile);
          tmpMeta = MetaInfo.Deserialize(doc);
          if (doc.Root!.LElementOrNull("properties") is XElement pelm)
            // workaround due to WPF binding refresh
            tmpSpg = SimPropertyGroup.Deserialize(pelm);
          tmp = Deserializer.Deserialize(doc);
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Unable to read/deserialize checklist-set from '{xmlFile}'. Invalid file content?", ex);
        }

        // check duplicit property declarations
        if (tmpSpg != null)
        {
          logger.Invoke(LogLevel.INFO, "Checking property definition duplicity");
          var a = this.SimPropertyGroup.GetAllSimPropertiesRecursively();
          var b = tmpSpg.GetAllSimPropertiesRecursively();
          var duplicits = a.Select(q => q.Name).Intersect(b.Select(q => q.Name));
          if (duplicits.Any())
            throw new ApplicationException("There are duplicit property declarations: " + string.Join(", ", duplicits));
        }

        // check checkset sanity
        logger.Invoke(LogLevel.INFO, $"Checking sanity");
        var props = tmpSpg == null
          ? this.SimPropertyGroup.GetAllSimPropertiesRecursively()
          : tmpSpg.GetAllSimPropertiesRecursively().Union(this.SimPropertyGroup.GetAllSimPropertiesRecursively()).ToList();
        Try(() => CheckSanity(tmp, props), ex => new ApplicationException("Error loading checklist.", ex));

        // bind next-checklist references
        logger.Invoke(LogLevel.INFO, $"Binding checklist references");
        Try(() => BindNextChecklists(tmp), ex => new ApplicationException("Error binding checklist references.", ex));

        // initialize sound streams
        logger.Invoke(LogLevel.INFO, $"Loading/generating sounds");
        Try(() => InitializeSoundStreams(tmp.Checklists, System.IO.Path.GetDirectoryName(xmlFile)!),
          ex => new ApplicationException("Error creating sound streams for checklist.", ex));

        if (tmpSpg != null)
        {
          //FIXME todo here we extend sim-properties with those defined in checklist
          // this causes issue when the same checklist is loaded again
          // causing duplicit property definition
          var spg = new SimPropertyGroup();
          spg.Properties.AddRange(this.SimPropertyGroup.Properties);
          spg.Properties.Add(tmpSpg);
          this.SimPropertyGroup = spg;
        }
        this.PropertyUsageCounts = GetPropertyUsagesCounts(tmp, this.SimPropertyGroup.GetAllSimPropertiesRecursively());


        this.MetaInfo = tmpMeta;

        this.CheckListVMs = tmp.Checklists.Select(q => new CheckListVM()
        {
          CheckList = q,
          Variables = VariableVMS.Create(q.Variables),
          Items = new(q.Items.Select(p => new CheckItemVM() { CheckItem = p }))
        }).ToList();

        this.setIsReadyFlagAction(true);
        this.LastLoadedFile = xmlFile;
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
        checklist.NextChecklists = new();
        if (string.IsNullOrEmpty(checklist.NextChecklistIds))
        {
          if (i < tmp.Checklists.Count - 1)
            checklist.NextChecklists.Add(tmp.Checklists[i + 1]);
          else
            checklist.NextChecklists.Add(tmp.Checklists[0]);
        }
        else
        {
          var pts = checklist.NextChecklistIds.Split(";");
          foreach (var pt in pts)
          {
            try
            {
              var n = tmp.Checklists.Single(q => q.Id == pt);
              checklist.NextChecklists.Add(n);
            }
            catch (Exception ex)
            {
              throw new ApplicationException($"Failed to find checklist with id '{pt}'.", ex);
            }
          }
        }
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
        var definedVariables = item.Variables.Select(q => q.Name);
        var usedVariableNames = StateCheckUtils.ExtractVariables(item.Trigger!);
        var misses = usedVariableNames
          .Where(q => definedVariables.None(v => v == q));
        missingVariables.AddRange(misses);
      }
      if (missingVariables.Any())
        throw new ApplicationException($"Required variables not found in defined variables: {string.Join(", ", missingVariables.Distinct())}.");
    }

    private void InitializeSoundStreams(List<CheckList> checklists, string relativePath)
    {
      MsSapiModule module = new MsSapiModule();
      ITtsProvider synthetizer = module.GetProvider(Settings.Synthetizer);
      Dictionary<string, byte[]> generatedSounds = new();
      foreach (var checklist in checklists)
      {
        // TODO correct load meta data and checklist entry/exit speeches
        InitializeSoundStreamsForChecklist(checklist, generatedSounds, synthetizer, relativePath);

        foreach (var item in checklist.Items)
        {
          InitializeSoundStreamsForItems(item.Call,
            generatedSounds, synthetizer, relativePath,
            this.Settings.DelayAfterCall);
          InitializeSoundStreamsForItems(item.Confirmation,
            generatedSounds, synthetizer, relativePath,
            this.Settings.DelayAfterConfirmation);
        }
      }
    }

    private void InitializeSoundStreamsForChecklist(
      CheckList checklist,
      Dictionary<string, byte[]> generatedSounds,
      ITtsProvider synthetizer,
      string relativePath)
    {
      if (checklist.CustomEntrySpeech != null)
        InitializeSoundStreamsForItems(checklist.CustomEntrySpeech,
          generatedSounds, synthetizer, relativePath,
          this.Settings.DelayAfterNotification);
      if (checklist.CustomExitSpeech != null)
        InitializeSoundStreamsForItems(checklist.CustomExitSpeech,
          generatedSounds, synthetizer, relativePath,
          this.Settings.DelayAfterNotification);
      if (checklist.CustomPausedAlertSpeech != null)
        InitializeSoundStreamsForItems(checklist.CustomPausedAlertSpeech,
          generatedSounds, synthetizer, relativePath,
          this.Settings.DelayAfterNotification);

      checklist.EntrySpeechBytes =
        checklist.CustomEntrySpeech != null
        ? checklist.CustomEntrySpeech.Bytes
        : AudioUtils.AppendSilence(
          synthetizer.Convert($"{checklist.CallSpeech} checklist"),
          this.Settings.DelayAfterNotification);
      checklist.ExitSpeechBytes =
        checklist.CustomExitSpeech != null
        ? checklist.CustomExitSpeech.Bytes
        : AudioUtils.AppendSilence(
          synthetizer.Convert($"{checklist.CallSpeech} checklist completed"),
          this.Settings.DelayAfterNotification);
      checklist.PausedAlertSpeechBytes =
        checklist.CustomPausedAlertSpeech != null
        ? checklist.CustomPausedAlertSpeech.Bytes
        : AudioUtils.AppendSilence(
          synthetizer.Convert($"{checklist.CallSpeech} checklist pending"),
          this.Settings.DelayAfterNotification);
    }

    private void InitializeSoundStreamsForItems(
      CheckDefinition checkDefinition,
      Dictionary<string, byte[]> generatedSounds,
      ITtsProvider synthetizer,
      string relativePath,
      int delay)
    {
      if (checkDefinition.Type == CheckDefinition.CheckDefinitionType.File)
        try
        {
          checkDefinition.Bytes = System.IO.File.ReadAllBytes(
            System.IO.Path.Combine(relativePath, checkDefinition.Value));
        }
        catch (Exception ex)
        {
          throw new ApplicationException($"Unable to load sound file '{checkDefinition.Value}'.", ex);
        }
      else if (checkDefinition.Type == CheckDefinition.CheckDefinitionType.Speech)
        try
        {
          if (generatedSounds.ContainsKey($"{checkDefinition.Value};{delay}"))
            checkDefinition.Bytes = generatedSounds[$"{checkDefinition.Value};{delay}"];
          else
          {
            var tmp = synthetizer.Convert(checkDefinition.Value);
            tmp = AudioUtils.AppendSilence(tmp, delay);
            checkDefinition.Bytes = tmp;
            generatedSounds[$"{checkDefinition.Value};{delay}"] = checkDefinition.Bytes;
          }
        }
        catch (Exception ex)
        {
          throw new ApplicationException($"Unable to generated sound for speech '{checkDefinition.Value}'.", ex);
        }
      else
        throw new NotImplementedException($"Unknown type {checkDefinition.Type}.");
    }
  }
}
