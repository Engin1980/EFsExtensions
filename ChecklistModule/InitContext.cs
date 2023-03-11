using ChecklistModule.Types;
using ChlaotModuleBase;
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
using ChlaotModuleBase.ModuleUtils.Synthetization;
using ChlaotModuleBase.ModuleUtils.StateChecking;
using ELogging;

namespace ChecklistModule
{
  public class InitContext : NotifyPropertyChangedBase
  {
    private readonly NewLogHandler logHandler;
    private readonly Action<bool> setIsReadyFlagAction;

    public CheckSet ChecklistSet
    {
      get => base.GetProperty<CheckSet>(nameof(ChecklistSet))!;
      set => base.UpdateProperty(nameof(ChecklistSet), value);
    }

    public Settings Settings { get; private set; }

    public InitContext(Settings settings, Action<bool> setIsReadyFlagAction)
    {
      Settings = settings ?? throw new ArgumentNullException(nameof(settings));
      this.logHandler = Logger.RegisterSender(this, "[Checklist.InitContext]");
      this.setIsReadyFlagAction = setIsReadyFlagAction ?? throw new ArgumentNullException(nameof(setIsReadyFlagAction));
    }

    internal void LoadFile(string xmlFile)
    {
      CheckSet tmp;
      var factory = new XmlSerializerFactory();
      XDocument doc;

      try
      {
        logHandler.Invoke(LogLevel.INFO, $"Loading file '{xmlFile}'");
        try
        {
          doc = XDocument.Load(xmlFile);
          EXml<CheckSet> exml = CreateDeserializer();
          tmp = exml.Deserialize(doc);
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Unable to read/deserialize checklist-set from '{xmlFile}'. Invalid file content?", ex);
        }

        logHandler.Invoke(LogLevel.INFO, $"Checking sanity");
        try
        {
          CheckSanity(tmp);
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Error loading checklist.", ex);
        }

        logHandler.Invoke(LogLevel.INFO, $"Binding checklist references");
        try
        {
          BindNextChecklists(tmp);
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Error binding checklist references.", ex);
        }

        logHandler.Invoke(LogLevel.INFO, $"Loading/generating sounds");
        try
        {
          InitializeSoundStreams(tmp, System.IO.Path.GetDirectoryName(xmlFile)!);
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Error creating sound streams for checklist.", ex);
        }

        this.ChecklistSet = tmp;
        this.setIsReadyFlagAction(true);
        logHandler.Invoke(LogLevel.INFO, $"Checklist file '{xmlFile}' successfully loaded.");

      }
      catch (Exception ex)
      {
        this.setIsReadyFlagAction(false);
        logHandler.Invoke(LogLevel.ERROR, $"Failed to load checklist from '{xmlFile}'." + ex.GetFullMessage("\n\t"));
      }


    }

    private static void BindNextChecklists(CheckSet tmp)
    {
      for (int i = 0; i < tmp.Checklists.Count; i++)
      {
        var checklist = tmp.Checklists[i];
        if (checklist.NextChecklistId is null)
        {
          if (i < tmp.Checklists.Count - 1) checklist.NextChecklist = tmp.Checklists[i + 1];
        }
        else
          checklist.NextChecklist = tmp.Checklists.Single(q => q.Id == checklist.NextChecklistId);
      }
    }

    private static EXml<CheckSet> CreateDeserializer()
    {
      EXml<CheckSet> ret = new();

      var oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(CheckDefinition))
        .WithIgnoredProperty(nameof(CheckDefinition.Bytes));
      ret.Context.ElementDeserializers.Insert(0, oed);

      oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(CheckSet))
        .WithCustomPropertyDeserialization(
          nameof(CheckSet.Checklists),
          (e, t, f, c) =>
          {
            var deser = c.ResolveElementDeserializer(typeof(CheckList));
            var items = e.LElements("checklist")
              .Select(q => SafeUtils.Deserialize(q, typeof(CheckList), deser, c))
              .Cast<CheckList>()
              .ToList();
            SafeUtils.SetPropertyValue(f, t, items);
          });
      ret.Context.ElementDeserializers.Insert(0, oed);

      oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(CheckList))
        .WithIgnoredProperty(nameof(CheckList.EntrySpeechBytes))
        .WithIgnoredProperty(nameof(CheckList.ExitSpeechBytes))
        .WithCustomPropertyDeserialization(
        nameof(CheckList.Items),
        (e, t, p, c) =>
        {
          var deser = c.ResolveElementDeserializer(typeof(CheckItem));
          var items = e.LElements("item")
          .Select(q => SafeUtils.Deserialize(q, typeof(CheckItem), deser, c))
          .Cast<CheckItem>()
          .ToList();
          SafeUtils.SetPropertyValue(p, t, items);
        })
        .WithCustomPropertyDeserialization(
        nameof(CheckList.NextChecklistId),
        (e, t, p, c) =>
        {
          string? val = e.LElementOrNull("nextChecklistId")?.Attribute("id")?.Value;
          SafeUtils.SetPropertyValue(p, t, val);
        });
      ret.Context.ElementDeserializers.Insert(0, oed);

      oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(CheckSet))
      .WithCustomPropertyDeserialization(
        "Checklists",
        (e, t, f, c) =>
        {
          var deser = c.ResolveElementDeserializer(typeof(CheckList));
          var val = e.LElements("checklist")
          .Select(q => SafeUtils.Deserialize(q, typeof(CheckList), deser, c))
          .Cast<CheckList>()
          .ToList();
          SafeUtils.SetPropertyValue(f, t, val);
        });
      ret.Context.ElementDeserializers.Insert(0, oed);

      ret.Context.ElementDeserializers.Insert(0, new StateCheckDeserializer());

      return ret;
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
          if (stp.Value == null)
          {
            throw new ApplicationException($"Value of checked property {stp.DisplayName} not set." +
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

      tmp.Checklists
        .Where(q => q.MetaInfo != null)
        .Where(q => q.MetaInfo.Autostart != null)
        .ToList()
        .ForEach(q => checkStateCheckItem(q.MetaInfo.Autostart));
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
      if (checklist.MetaInfo?.CustomEntrySpeech != null)
        InitializeSoundStreamsForItems(checklist.MetaInfo.CustomEntrySpeech, generatedSounds, synthetizer, relativePath);
      if (checklist.MetaInfo?.CustomExitSpeech != null)
        InitializeSoundStreamsForItems(checklist.MetaInfo.CustomExitSpeech, generatedSounds, synthetizer, relativePath);

      checklist.EntrySpeechBytes =
        checklist.MetaInfo?.CustomEntrySpeech != null
        ? checklist.MetaInfo.CustomEntrySpeech.Bytes
        : synthetizer.Generate($"{checklist.CallSpeech} checklist");
      checklist.ExitSpeechBytes =
        checklist.MetaInfo?.CustomExitSpeech != null
        ? checklist.MetaInfo.CustomExitSpeech.Bytes
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
