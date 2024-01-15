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
        throw new ApplicationException("Failed to load global sim properties.",ex);
      }
      return ret;
    }

    internal void LoadFile(string xmlFile)
    {
      CheckSet tmp;
      try
      {
        logger.Invoke(LogLevel.INFO, $"Loading file '{xmlFile}'");
        try
        {
          XDocument doc = XDocument.Load(xmlFile);
          this.MetaInfo = MetaInfo.Deserialize(doc);
          if (doc.Root!.LElement("properties") is XElement pelm)
          {
            // workaround due to WPF binding refresh
            var customProps = SimPropertyGroup.Deserialize(pelm);
            var spg = new SimPropertyGroup();
            spg.Properties.AddRange(this.SimPropertyGroup.Properties);
            spg.Properties.Add(customProps);
            this.SimPropertyGroup = spg;
          }
          tmp = Deserializer.Deserialize(doc);
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Unable to read/deserialize checklist-set from '{xmlFile}'. Invalid file content?", ex);
        }

        logger.Invoke(LogLevel.INFO, $"Checking sanity");
        Try(() => CheckSanity(tmp), ex => new ApplicationException("Error loading checklist.", ex));

        logger.Invoke(LogLevel.INFO, $"Binding checklist references");
        Try(() => BindNextChecklists(tmp), ex => new ApplicationException("Error binding checklist references.", ex));

        logger.Invoke(LogLevel.INFO, $"Loading/generating sounds");
        Try(() => InitializeSoundStreams(tmp, System.IO.Path.GetDirectoryName(xmlFile)!),
          ex => new ApplicationException("Error creating sound streams for checklist.", ex));

        this.ChecklistSet = tmp;
        this.setIsReadyFlagAction(true);
        logger.Invoke(LogLevel.INFO, $"Checklist file '{xmlFile}' successfully loaded.");

      }
      catch (Exception ex)
      {
        this.setIsReadyFlagAction(false);
        logger.Invoke(LogLevel.ERROR, $"Failed to load checklist from '{xmlFile}'." + ex.GetFullMessage("\n\t"));
      }
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

    private static void CheckSanity(CheckSet tmp)
    {
      // check no duplicit
      var ids = tmp.Checklists.Select(q => q.Id);
      var dids = ids.Distinct();
      var exc = ids.Except(dids);
      if (exc.Any())
      {
        throw new ApplicationException("There are repeated checklist id definitions: " + string.Join(", ", exc));
      }

      // all property conditions has values
      Stack<IStateCheckItem> stck = new();
      void checkStateCheckItem(IStateCheckItem sti)
      {
        stck.Push(sti);
        if (sti is StateCheckCondition stc)
          stc.Items.ForEach(q => checkStateCheckItem(q));
        else if (sti is StateCheckDelay std)
          checkStateCheckItem(std.Item);
        else if (sti is StateCheckProperty stp)
        {
          if (stp.Expression == null)
          {
            throw new ApplicationException($"Expression of checked property {stp.DisplayString} not set." +
              $"Location: {string.Join(" ==> ", stck.Reverse().ToList().Select(q => q.DisplayString))}");
          }
        }
        else if (sti is StateCheckTrueFalse sttf)
        {
          // intentionally blank
        }
        else
          throw new ApplicationException($"Unsupported type of '{nameof(IStateCheckItem)}'.");
        stck.Pop();
      }

      tmp.Checklists.Where(q => q.Trigger != null).ForEach(q => checkStateCheckItem(q.Trigger));
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
