using FailuresModule.Model.Incidents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Model.RunTime
{
    internal class IncidentDefinitionVM : RunIncident
  {
    internal static RunIncident Create(IncidentDefinition id)
    {
      IncidentDefinitionVM ret = new IncidentDefinitionVM(id);
      return ret;
    }

    private IncidentDefinitionVM(IncidentDefinition incidentDefinition)
    {
      IncidentDefinition = incidentDefinition;
    }

    public IncidentDefinition IncidentDefinition { get; }
    public List<RunTriggerEvaluation> TriggerEvaluations { get; } = new List<RunTriggerEvaluation>();
    public List<Trigger> OneShotTriggersInvoked { get; } = new List<Trigger>();
  }
}
