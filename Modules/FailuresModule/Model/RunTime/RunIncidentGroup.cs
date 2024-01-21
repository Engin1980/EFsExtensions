using FailuresModule.Model.Incidents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Model.RunTime
{
    internal class RunIncidentGroup : RunIncident
  {
    public static RunIncidentGroup Create(IncidentGroup incidentGroup)
    {
      List<RunIncident> incidents = new();

      foreach (var i in incidentGroup.Incidents)
      {
        RunIncident ri;
        if (i is IncidentDefinition id)
          ri = IncidentDefinitionVM.Create(id);
        else if (i is IncidentGroup ig)
          ri = RunIncidentGroup.Create(ig);
        else
          throw new NotImplementedException();
        incidents.Add(ri);
      }

      RunIncidentGroup ret = new RunIncidentGroup(incidentGroup.Title, incidents);
      return ret;
    }

    private RunIncidentGroup(string title, List<RunIncident> incidents)
    {
      this.Title = title;
      this.Incidents = incidents;
    }
    public string Title { get; }
    public List<RunIncident> Incidents { get; }
  }
}
