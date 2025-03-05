using ELogging;
using Eng.EFsExtensions.EFsExtensionsModuleBase;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking.StateModel;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking.VariableModel;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.Synthetization;
using Eng.EFsExtensions.Modules.CopilotModule.Types;
using EXmlLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using static ESystem.Functions.TryCatch;
using System.Xml.Linq;
using System.Xml.Serialization;
using ESystem;
using static Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking.StateCheckUtils;
using Eng.EFsExtensions.Modules.CopilotModule.Types.VMs;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.VMs;
using ESystem.Miscelaneous;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs.MsSapi;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs;

namespace Eng.EFsExtensions.Modules.CopilotModule
{
  public class InitContext : NotifyPropertyChanged
  {
    #region Fields

    private readonly Logger logger;
    private readonly Action<bool> setIsReadyFlagAction;

    #endregion Fields

    #region Properties

    public string? LastLoadedFile { get; private set; }
    public MetaInfo MetaInfo
    {
      get => base.GetProperty<MetaInfo>(nameof(MetaInfo))!;
      set => base.UpdateProperty(nameof(MetaInfo), value);
    }

    public List<PropertyUsageCount> PropertyUsageCounts
    {
      get => base.GetProperty<List<PropertyUsageCount>>(nameof(PropertyUsageCounts))!;
      set => base.UpdateProperty(nameof(PropertyUsageCounts), value);
    }

    public SimPropertyGroup SimPropertyGroup
    {
      get => base.GetProperty<SimPropertyGroup>(nameof(SimPropertyGroup))!;
      set => base.UpdateProperty(nameof(SimPropertyGroup), value);
    }

    public BindingList<SpeechDefinitionVM> SpeechDefinitionVMs
    {
      get => base.GetProperty<BindingList<SpeechDefinitionVM>>(nameof(SpeechDefinitionVMs))!;
      set => base.UpdateProperty(nameof(SpeechDefinitionVMs), value);
    }
    internal Settings Settings { get; private set; }

    #endregion Properties

    public record PropertyUsageCount(SimProperty Property, int Count);

    #region Constructors

    internal InitContext(Settings settings, Action<bool> setIsReadyFlagAction)
    {
      Settings = settings ?? throw new ArgumentNullException(nameof(settings));
      this.logger = Logger.Create(this, "Copilot.InitContext");
      this.setIsReadyFlagAction = setIsReadyFlagAction ?? throw new ArgumentNullException(nameof(setIsReadyFlagAction));
      this.SimPropertyGroup = LoadDefaultSimProperties();
    }

    #endregion Constructors

    #region Methods

    internal void LoadFile(string xmlFile)
    {
      MetaInfo tmpMeta;
      CopilotSet tmp;
      SimPropertyGroup? tmpSpg = null;

      try
      {
        logger.Invoke(LogLevel.INFO, $"Checking file '{xmlFile}'");
        try
        {
          XmlUtils.ValidateXmlAgainstXsd(xmlFile, new string[] { @".\xmls\xsds\Global.xsd", @".\xmls\xsds\CopilotSchema.xsd" }, out List<string> errors);
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
          XDocument doc = XDocument.Load(xmlFile, LoadOptions.SetLineInfo);
          tmpMeta = MetaInfo.Deserialize(doc);
          if (doc.Root!.LElementOrNull("properties") is XElement pelm)
            // workaround due to WPF binding refresh
            tmpSpg = SimPropertyGroup.Deserialize(pelm);
          tmp = Types.Xml.Deserializer.Deserialize(doc);
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Unable to read/deserialize copilot-set from '{xmlFile}'. Invalid file content?", ex);
        }

        logger.Invoke(LogLevel.INFO, $"Checking sanity");
        var props = tmpSpg == null
          ? this.SimPropertyGroup.GetAllSimPropertiesRecursively()
          : tmpSpg.GetAllSimPropertiesRecursively().Union(this.SimPropertyGroup.GetAllSimPropertiesRecursively()).ToList();
        Try(() => CheckSanity(tmp, props), ex => new ApplicationException("Error loading copilot data.", ex));


        if (tmpSpg != null)
        {
          var spg = new SimPropertyGroup();
          spg.Properties.AddRange(this.SimPropertyGroup.Properties);
          spg.Properties.Add(tmpSpg);
          this.SimPropertyGroup = spg;
        }
        this.PropertyUsageCounts = GetPropertyUsagesCounts(tmp, this.SimPropertyGroup.GetAllSimPropertiesRecursively());
        this.MetaInfo = tmpMeta;

        this.SpeechDefinitionVMs = tmp.SpeechDefinitions
          .Select(q => new SpeechDefinitionVM()
          {
            SpeechDefinition = q,
            Variables = VariableVMS.Create(q.Variables)
          })
          .ToBindingList();

        logger.Invoke(LogLevel.INFO, $"Loading/generating sounds");
        Try(() => InitializeSoundStreams(this.SpeechDefinitionVMs, System.IO.Path.GetDirectoryName(xmlFile)!),
          ex => new ApplicationException("Error creating sound streams.", ex));

        logger.Invoke(LogLevel.INFO, "Binding property changed events");
        BindPropertyChangedEvents();

        UpdateReadyFlag();
        this.LastLoadedFile = xmlFile;
        logger.Invoke(LogLevel.INFO, $"Copilot set file '{xmlFile}' successfully loaded.");

      }
      catch (Exception ex)
      {
        this.setIsReadyFlagAction(false);
        logger.Invoke(LogLevel.ERROR, $"Failed to load copilot set from '{xmlFile}'." + ex.GetFullMessage());
      }
    }

    private void BindPropertyChangedEvents()
    {
      this.SpeechDefinitionVMs
        .SelectMany(q => q.Variables)
        .Where(q => !q.IsReadOnly)
        .ForEach(q => q.PropertyChanged += Variable_PropertyChanged);
    }

    private void BuildSpeech(
      SpeechDefinitionVM speechDefinitionVM,
      Dictionary<string, byte[]> generatedSounds,
      ITtsProvider synthetizer,
      string relativePath)
    {
      Speech speech = speechDefinitionVM.SpeechDefinition.Speech;
      if (speech.Type == Speech.SpeechType.File)
        try
        {
          speech.Bytes = System.IO.File.ReadAllBytes(
            System.IO.Path.Combine(relativePath, speech.Value));
        }
        catch (Exception ex)
        {
          throw new ApplicationException($"Unable to load sound file '{speech.Value}'.", ex);
        }
      else if (speech.Type == Speech.SpeechType.Speech)
      {
        string txt = speech.GetEvaluatedValue(speechDefinitionVM.Variables);
        try
        {
          if (generatedSounds.ContainsKey(txt))
            speech.Bytes = generatedSounds[txt];
          else
          {
            speech.Bytes = synthetizer.Convert(txt);
            generatedSounds[txt] = speech.Bytes;
          }
        }
        catch (Exception ex)
        {
          throw new ApplicationException($"Unable to generated sound for speech '{speech.Value}'.", ex);
        }
      }
      else
        throw new NotImplementedException($"Unknown type {speech.Type}.");
    }

    private List<PropertyUsageCount> GetPropertyUsagesCounts(CopilotSet tmp, List<SimProperty> simProperties)
    {
      Dictionary<string, int> dct = new();

      var lst = tmp.SpeechDefinitions
        .Select(q => q.Trigger)
        .Union(tmp.SpeechDefinitions
          .Where(q => q.ReactivationTrigger != null)
          .Select(q => q.ReactivationTrigger!));
      foreach (var item in lst)
      {
        var pus = StateCheckUtils.ExtractProperties(item);
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

    private void CheckSanity(CopilotSet tmp, List<SimProperty> definedProperties)
    {
      // all property conditions has values
      IStateCheckItem[] triggers = tmp.SpeechDefinitions.Select(q => q.Trigger).ToArray();
      IStateCheckItem[] reactivationTriggers = tmp.SpeechDefinitions.Where(q => q.ReactivationTrigger != null).Select(q => q.ReactivationTrigger!).ToArray();
      IStateCheckItem[] allTriggers = triggers.Union(reactivationTriggers).ToArray();
      List<StateCheckProperty> allTriggerProps = StateCheckUtils.ExtractStateCheckProperties(allTriggers);
      allTriggerProps
        .Where(q => q.Expression == null)
        .ForEach(q => throw new ApplicationException($"Expression of checked property {q.DisplayString} not set."));

      // check all properties defined
      List<PropertyUsage> propertyUsages = StateCheckUtils.ExtractProperties(allTriggers);
      List<string> missingProperties = propertyUsages
        .Where(q => definedProperties.None(p => p.Name == q.PropertyName))
        .Select(q => q.PropertyName)
        .Distinct()
        .ToList();
      if (missingProperties.Any())
        throw new ApplicationException($"Required properties not found in defined properties: {string.Join(", ", missingProperties)}");

      // all variables are defined
      // check all variables are defined
      List<string> missingVariables = new();
      foreach (var item in tmp.SpeechDefinitions)
      {
        List<Variable> definedVariables = item.Variables;
        List<VariableUsage> variableUsages = StateCheckUtils.ExtractVariablesFromProperties(item.Trigger);
        var vmp = variableUsages
          .Where(q => definedVariables.None(v => v.Name == q.VariableName))
          .Select(q => q.VariableName)
          .ToList();
        missingVariables.AddRange(vmp);
      }
      foreach (var item in tmp.SpeechDefinitions.Where(q => q.ReactivationTrigger != null))
      {
        List<Variable> definedVariables = item.Variables;
        List<VariableUsage> variableUsages = StateCheckUtils.ExtractVariablesFromProperties(item.ReactivationTrigger!);
        var vmp = variableUsages
          .Where(q => definedVariables.None(v => v.Name == q.VariableName))
          .Select(q => q.VariableName)
          .ToList();
        missingVariables.AddRange(vmp);
      }
      if (missingVariables.Any())
        throw new ApplicationException($"Required variables not found in defined variables: {string.Join(", ", missingVariables.Distinct())}.");
    }

    private void InitializeSoundStreams(BindingList<SpeechDefinitionVM> speechDefinitions, string relativePath)
    {
      MsSapiModule module = new MsSapiModule();
      ITtsProvider synthetizer = module.GetProvider(Settings.Synthetizer);
      Dictionary<string, byte[]> generatedSounds = new();
      foreach (var sd in speechDefinitions)
      {
        BuildSpeech(sd, generatedSounds, synthetizer, relativePath);
      }
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
    private void UpdateReadyFlag()
    {
      bool ready = this.SpeechDefinitionVMs != null && this.SpeechDefinitionVMs.SelectMany(q => q.Variables).All(q => !double.IsNaN(q.Value));
      this.setIsReadyFlagAction(ready);
    }

    private void Variable_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      MsSapiModule module = new MsSapiModule();
      ITtsProvider synthetizer = module.GetProvider(Settings.Synthetizer);
      VariableVM vvm = (VariableVM)sender!;
      UserVariable variable = (UserVariable)vvm.Variable;
      SpeechDefinitionVM sd = this.SpeechDefinitionVMs.First(q => q.Variables.Any(q => q.Variable == variable));
      if (sd.SpeechDefinition.Speech.Type == Speech.SpeechType.Speech
        && sd.SpeechDefinition.Speech.GetUsedVariables().Any(q => q == variable.Name))
      {
        BuildSpeech(sd, new(), synthetizer, "");
      }

      UpdateReadyFlag();
    }

    #endregion Methods
  }
}
