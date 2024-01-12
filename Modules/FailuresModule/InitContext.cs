using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using ESystem;
using static ESystem.Functions;
using EXmlLib;
using EXmlLib.Deserializers;
using FailuresModule.Model.Incidents;
using FailuresModule.Model.Failures;
using FailuresModule.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace FailuresModule
{
  public class InitContext : NotifyPropertyChangedBase
  {
    private readonly Logger logger;
    private readonly Action<bool> setIsReadyFlagAction;

    public InitContext(Action<bool> setIsReadyFlagAction)
    {
      this.logger = Logger.Create(this);
      this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
      this.setIsReadyFlagAction = setIsReadyFlagAction ?? throw new ArgumentNullException(nameof(setIsReadyFlagAction));
      this.FailureDefinitionsFlat = new();
      this.LoadDefaultFailures();
    }

    public List<FailureDefinition> FailureDefinitionsFlat { get; set; }
    public List<FailureDefinitionBase> FailureDefinitions { get; set; }

    public IncidentTopGroup FailureSet
    {
      get => base.GetProperty<IncidentTopGroup>(nameof(FailureSet))!;
      set => base.UpdateProperty(nameof(FailureSet), value);
    }

    internal void LoadDefaultFailures()
    {
      this.logger.Log(LogLevel.INFO, "Loading default failures...");
      var fdg = FailuresModule.Model.Failures.Xml.Deserialization.Deserialize(@".\Xmls\FailureDefinitions.xml");
      FailureDefinitions = fdg.Items;
      FailureDefinitionsFlat = FailureDefinition.Flatten(fdg.Items);
      this.logger.Log(LogLevel.INFO, "Loading default failures - done...");
    }

    public void LoadFile(string xmlFile)
    {
      try
      {
        logger.Invoke(LogLevel.INFO, $"Loading file '{xmlFile}'");
        XDocument doc = Try(() => XDocument.Load(xmlFile, LoadOptions.SetLineInfo),
          ex => throw new ApplicationException($"Unable to load xml file '{xmlFile}'.", ex));

        IncidentTopGroup tmp = Try(() => FailuresModule.Model.Incidents.Xml.Deserialization.Deserialize(doc.Root!, this.FailureDefinitionsFlat),
          ex => throw new ApplicationException("Unable to read/deserialize copilot-set from '{xmlFile}'. Invalid file content?", ex));

        logger.Invoke(LogLevel.INFO, $"Aplying file-defined failure definitions");
        if (doc.Root!.LElementOrNull("definitions") is XElement elm) //non-null check
          Try(() =>
          {
            var failDefs = FailuresModule.Model.Failures.Xml.Deserialization.Deserialize(elm);
            FailureDefinition.MergeFailureDefinitions(this.FailureDefinitions, failDefs);
            FailureDefinitionsFlat = FailureDefinition.Flatten(this.FailureDefinitions);
          }, 
          ex => throw new ApplicationException("Failed to analyse or apply file-defined failures.", ex));

        logger.Invoke(LogLevel.INFO, $"Checking sanity");
        Try(
          () => SanityChecker.CheckSanity(tmp, this.FailureDefinitionsFlat),
          ex => throw new ApplicationException("Error loading failures.", ex));

        this.FailureSet = tmp;
        UpdateReadyFlag();
        logger.Invoke(LogLevel.INFO, $"Failure set file '{xmlFile}' successfully loaded.");
        this.setIsReadyFlagAction(true);
      }
      catch (Exception ex)
      {
        this.setIsReadyFlagAction(false);
        logger.Invoke(LogLevel.ERROR, $"Failed to load failure set from '{xmlFile}'." + ex.GetFullMessage());
      }
    }

    private void UpdateReadyFlag()
    {
      logger.Invoke(LogLevel.WARNING, "UpdateReadyFlag() NotImplemented");
    }
  }
}
