using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using ESystem;
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
      this.BuildFailures();
    }

    public List<FailureDefinition> FailureDefinitionsFlat { get; set; }
    public List<FailureDefinitionBase> FailureDefinitions { get; set; }

    public IncidentTopGroup FailureSet
    {
      get => base.GetProperty<IncidentTopGroup>(nameof(FailureSet))!;
      set => base.UpdateProperty(nameof(FailureSet), value);
    }

    internal void BuildFailures()
    {
      var fdg = FailureDefinitionFactory.BuildFailures();
      FailureDefinitions = fdg.Items;
      FailureDefinitionsFlat = FailureDefinitions
        .FlattenRecursively((FailureDefinitionGroup q) => q.Items)
        .Cast<FailureDefinition>()
        .ToList();
    }

    public void LoadFile(string xmlFile)
    {
      var factory = new XmlSerializerFactory();
      IncidentTopGroup tmp;
      XDocument doc;

      try
      {
        logger.Invoke(LogLevel.INFO, $"Loading file '{xmlFile}'");
        try
        {
          doc = XDocument.Load(xmlFile, LoadOptions.SetLineInfo);
          EXml<IncidentTopGroup> exml = FailuresModule.Model.Incidents.Xml.Deserialization.CreateDeserializer(this.FailureDefinitionsFlat);
          tmp = exml.Deserialize(doc);
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Unable to read/deserialize copilot-set from '{xmlFile}'. Invalid file content?", ex);
        }

        logger.Invoke(LogLevel.INFO, $"Checking sanity");
        try
        {
          SanityChecker.CheckSanity(tmp, this.FailureDefinitionsFlat);
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Error loading failures.", ex);
        }

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
