using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using ESystem;
using static ESystem.Functions;
using EXmlLib;
using EXmlLib.Deserializers;
using Eng.Chlaot.Modules.FailuresModule.Model.Incidents;
using Eng.Chlaot.Modules.FailuresModule.Model.Failures;
using Eng.Chlaot.Modules.FailuresModule.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.VMs;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using System.Drawing.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO.IsolatedStorage;

namespace Eng.Chlaot.Modules.FailuresModule
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


    public Percentage EstimatedProbabilityPerFlight
    {
      get => base.GetProperty<Percentage>(nameof(EstimatedProbabilityPerFlight))!;
      set => base.UpdateProperty(nameof(EstimatedProbabilityPerFlight), value);
    }

    public double EstimatedFlighstPerFailure
    {
      get => base.GetProperty<double>(nameof(EstimatedFlighstPerFailure))!;
      set => base.UpdateProperty(nameof(EstimatedFlighstPerFailure), value);
    }

    public PropertyVMS PropertyVMs
    {
      get => base.GetProperty<PropertyVMS>(nameof(PropertyVMs))!;
      set => base.UpdateProperty(nameof(PropertyVMs), value);
    }

    public MetaInfo MetaInfo
    {
      get => base.GetProperty<MetaInfo>(nameof(MetaInfo))!;
      set => base.UpdateProperty(nameof(MetaInfo), value);
    }

    public IncidentGroup FailureSet
    {
      get => base.GetProperty<IncidentGroup>(nameof(FailureSet))!;
      set => base.UpdateProperty(nameof(FailureSet), value);
    }

    internal void LoadDefaultFailures()
    {
      this.logger.Log(LogLevel.INFO, "Loading default failures...");
      var fdg = Eng.Chlaot.Modules.FailuresModule.Model.Failures.Xml.Deserialization.Deserialize(@".\Xmls\FailureDefinitions.xml");
      FailureDefinitions = fdg.Items;
      FailureDefinitionsFlat = FailureDefinition.Flatten(fdg.Items);
      this.logger.Log(LogLevel.INFO, "Loading default failures - done...");
    }

    public void LoadFile(string xmlFile)
    {
      try
      {
        logger.Invoke(LogLevel.INFO, $"Checking file '{xmlFile}'");
        try
        {
          XmlUtils.ValidateXmlAgainstXsd(xmlFile, new string[] {
            @".\xmls\xsds\Global.xsd",
            @".\xmls\xsds\FailureSchema.xsd",
            @".\xmls\xsds\FailureDefinitionSchema.xsd"}, out List<string> errors);
          if (errors.Any())
            throw new ApplicationException("XML does not match XSD: " + string.Join("; ", errors.Take(5)));
        }
        catch (Exception ex)
        {
          throw new ApplicationException($"Failed to validate XML file against XSD. Error: " + ex.Message, ex);
        }

        logger.Invoke(LogLevel.INFO, $"Loading file '{xmlFile}'");
        XDocument doc = Try(() => XDocument.Load(xmlFile, LoadOptions.SetLineInfo),
          ex => throw new ApplicationException($"Unable to load xml file '{xmlFile}'.", ex));

        MetaInfo tmpMeta = MetaInfo.Deserialize(doc);
        IncidentGroup tmpData = Try(() => Eng.Chlaot.Modules.FailuresModule.Model.Incidents.Xml.Deserialization.Deserialize(doc.Root!, this.FailureDefinitionsFlat),
          ex => throw new ApplicationException("Unable to read/deserialize copilot-set from '{xmlFile}'. Invalid file content?", ex));

        logger.Invoke(LogLevel.INFO, $"Aplying file-defined failure definitions");
        if (doc.Root!.LElementOrNull("definitions") is XElement elm) //non-null check
          Try(() =>
          {
            var failDefs = Eng.Chlaot.Modules.FailuresModule.Model.Failures.Xml.Deserialization.Deserialize(elm);
            FailureDefinition.MergeFailureDefinitions(this.FailureDefinitions, failDefs);
            FailureDefinitionsFlat = FailureDefinition.Flatten(this.FailureDefinitions);
          },
          ex => throw new ApplicationException("Failed to analyse or apply file-defined failures.", ex));

        logger.Invoke(LogLevel.INFO, $"Checking sanity");
        Try(
          () => SanityChecker.CheckSanity(tmpData, this.FailureDefinitionsFlat),
          ex => throw new ApplicationException("Error loading failures.", ex));

        this.FailureSet = tmpData;
        this.CalculateEstimations();
        this.FailureSet.GetIncidentDefinitionsRecursively().Select(q => q.Trigger).ForEach(q =>
        {
          q.PropertyChanged += (s, e) =>
          {
            if (e.PropertyName == nameof(Trigger.Probability)) CalculateEstimations();
          };
        });
        this.MetaInfo = tmpMeta;
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

    private void CalculateEstimations()
    {
      const double estimatedFlightLengthInHours = 2;
      const double estimatedOnceEventRepetitionsPerFlight = 2;
      List<double> negativeProbabilities = new();

      foreach (var id in this.FailureSet.GetIncidentDefinitionsRecursively())
      {
        double p;
        if (id.Trigger is CheckStateTrigger cst)
          p = (1 - cst.Probability);
        else if (id.Trigger is TimeTrigger tt)
          p = (1 - (estimatedFlightLengthInHours / tt.MtbfHours));
        else
          throw new NotImplementedException();
        for (int i = 0; i < estimatedOnceEventRepetitionsPerFlight; i++)
          negativeProbabilities.Add(p);
      }

      double m = 1 - negativeProbabilities.Aggregate(1.0, (a, b) => a * b);
      this.EstimatedProbabilityPerFlight = (Percentage)m;
      this.EstimatedFlighstPerFailure = 1 / m;
    }

    private void UpdateReadyFlag()
    {
      logger.Invoke(LogLevel.WARNING, "UpdateReadyFlag() NotImplemented");
    }
  }
}
