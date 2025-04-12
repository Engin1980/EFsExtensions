using ESystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FailuresModule.Model.Incidents
{
  public class IncidentGroup : Incident
  {
    public List<Incident> Incidents { get; set; } = null!;

    public List<IncidentDefinition> GetIncidentDefinitionsRecursively()
    {
      List<IncidentDefinition> ret = new List<IncidentDefinition>();

      foreach (var item in Incidents)
      {
        if (item is IncidentGroup ig)
        {
          var tmp = ig.GetIncidentDefinitionsRecursively();
          ret.AddRange(tmp);
        }
        else if (item is IncidentDefinition id)
          ret.Add(id);
      }
      return ret;
    }
  }
}
