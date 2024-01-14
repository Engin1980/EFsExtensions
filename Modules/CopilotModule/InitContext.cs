using CopilotModule;
using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.StateModel;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Synthetization;
using Eng.Chlaot.Modules.CopilotModule.Types;
using EXmlLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Eng.Chlaot.Modules.CopilotModule
{
  public class InitContext : NotifyPropertyChangedBase
  {
    private readonly Logger logger;
    private readonly Action<bool> setIsReadyFlagAction;
    private readonly Dictionary<Variable, SpeechDefinition> variableToSpeechDefinitionMapping = new();

    public CopilotSet Set
    {
      get => base.GetProperty<CopilotSet>(nameof(Set))!;
      set => base.UpdateProperty(nameof(Set), value);
    }

    internal Settings Settings { get; private set; }

    internal InitContext(Settings settings, Action<bool> setIsReadyFlagAction)
    {
      Settings = settings ?? throw new ArgumentNullException(nameof(settings));
      this.logger = Logger.Create(this, "Copilot.InitContext");
      this.setIsReadyFlagAction = setIsReadyFlagAction ?? throw new ArgumentNullException(nameof(setIsReadyFlagAction));
    }

    internal void LoadFile(string xmlFile)
    {
      MetaInfo tmpMeta;
      CopilotSet tmp;
      var factory = new XmlSerializerFactory();

      try
      {
        logger.Invoke(LogLevel.INFO, $"Loading file '{xmlFile}'");
        try
        {
          XDocument doc = XDocument.Load(xmlFile, LoadOptions.SetLineInfo);
          tmp = Eng.Chlaot.CopilotModule.Types.Xml.Deserializer.Deserialize(xmlFile);
          tmpMeta = MetaInfo.Deserialize(doc);
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Unable to read/deserialize copilot-set from '{xmlFile}'. Invalid file content?", ex);
        }

        logger.Invoke(LogLevel.INFO, $"Checking sanity");
        try
        {
          CheckSanity(tmp);
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Error loading checklist.", ex);
        }

        logger.Invoke(LogLevel.INFO, $"Analysing variables");
        try
        {
          AnalyseForUsedVariables(tmp);
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Error analysing variables.", ex);
        }

        logger.Invoke(LogLevel.INFO, $"Loading/generating sounds");
        try
        {
          InitializeSoundStreams(tmp, System.IO.Path.GetDirectoryName(xmlFile)!);
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Error creating sound streams.", ex);
        }

        this.Set = tmp;
        UpdateReadyFlag();
        logger.Invoke(LogLevel.INFO, $"Copilot set file '{xmlFile}' successfully loaded.");

      }
      catch (Exception ex)
      {
        this.setIsReadyFlagAction(false);
        logger.Invoke(LogLevel.ERROR, $"Failed to load copilot set from '{xmlFile}'." + ex.GetFullMessage());
      }
    }

    private void CheckSanity(CopilotSet tmp)
    {
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
            throw new ApplicationException($"Expression of checked property {stp.DisplayString} not set." +
              $"Location: {string.Join(" ==> ", stck.Reverse().ToList().Select(q => q.DisplayString))}");
        }
        else if (sti is StateCheckTrueFalse sttf)
        {
          // intentionally blank
        }
        else
          throw new ApplicationException($"Unsupported type of '{nameof(IStateCheckItem)}'.");
        stck.Pop();
      }

      tmp.SpeechDefinitions.ForEach(q => checkStateCheckItem(q.Trigger));
      tmp.SpeechDefinitions.ForEach(q => checkStateCheckItem(q.ReactivationTrigger));
    }

    private void UpdateReadyFlag()
    {
      bool ready = this.Set != null && this.Set.SpeechDefinitions.SelectMany(q => q.Variables).All(q => !double.IsNaN(q.Value));
      this.setIsReadyFlagAction(ready);
    }

    private void AnalyseForUsedVariables(CopilotSet set)
    {
      foreach (var sd in set.SpeechDefinitions)
      {
        sd.FillVariablesWithUndeclaredOnes();
        sd.Variables.ForEach(q => variableToSpeechDefinitionMapping[q] = sd);
        sd.Variables.Where(q => q is Variable).ForEach(q => q.PropertyChanged += Variable_PropertyChanged);
      }
    }

    private void Variable_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      Variable variable = (Variable)sender!;
      SpeechDefinition sd = variableToSpeechDefinitionMapping[variable];
      if (sd.Speech.Type == Speech.SpeechType.Speech
        && sd.Speech.GetUsedVariables().Any(q => q == variable.Name))
      {
        BuildSpeech(sd, new(), new Synthetizer(this.Settings.Synthetizer), "");
      }

      UpdateReadyFlag();
    }

    private void InitializeSoundStreams(CopilotSet set, string relativePath)
    {
      Synthetizer synthetizer = new(Settings.Synthetizer);
      Dictionary<string, byte[]> generatedSounds = new();
      foreach (var sd in set.SpeechDefinitions)
      {
        BuildSpeech(sd, generatedSounds, synthetizer, relativePath);
      }
    }

    private void BuildSpeech(
      SpeechDefinition speechDefinition,
      Dictionary<string, byte[]> generatedSounds,
      Synthetizer synthetizer,
      string relativePath)
    {
      Speech speech = speechDefinition.Speech;
      if (speech.Type == Speech.SpeechType.File)
        try
        {
          speech.Bytes = System.IO.File.ReadAllBytes(
            System.IO.Path.Combine(relativePath, speech.Value));
        }
        catch (Exception ex)
        {
          throw new EXmlException($"Unable to load sound file '{speech.Value}'.", ex);
        }
      else if (speech.Type == Speech.SpeechType.Speech)
      {
        string txt = speech.GetEvaluatedValue(speechDefinition.Variables);
        try
        {
          if (generatedSounds.ContainsKey(txt))
            speech.Bytes = generatedSounds[txt];
          else
          {
            speech.Bytes = synthetizer.Generate(txt);
            generatedSounds[txt] = speech.Bytes;
          }
        }
        catch (Exception ex)
        {
          throw new EXmlException($"Unable to generated sound for speech '{speech.Value}'.", ex);
        }
      }
      else
        throw new NotImplementedException($"Unknown type {speech.Type}.");
    }
  }
}
