using Eng.Chlaot.Modules.FailuresModule.Model.Incidents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.FailuresModule.Model.VMs
{
  internal class IncidentGroupVM : IncidentVM
  {
    public static IncidentGroupVM Create(IncidentGroup incidentGroup)
    {
      List<IncidentVM> incidents = new();

      foreach (var i in incidentGroup.Incidents)
      {
        IncidentVM ri;
        if (i is IncidentDefinition id)
          ri = new IncidentDefinitionVM(id);
        else if (i is IncidentGroup ig)
          ri = Create(ig);
        else
          throw new NotImplementedException();
        incidents.Add(ri);
      }

      IncidentGroupVM ret = new(incidentGroup.Title, incidents);
      return ret;
    }

    private IncidentGroupVM(string title, List<IncidentVM> incidents)
    {
      Title = title;
      Incidents = incidents;
    }
    public string Title { get; }
    public List<IncidentVM> Incidents { get; }
  }
}
