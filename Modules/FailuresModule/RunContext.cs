using FailuresModule.Types;
using FailuresModule.Types.RunVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule
{
  public class RunContext
  {
    public List<FailureDefinition> FailureDefinitions { get; }

    public List<RunIncident> Incidents { get; }

    public RunContext(List<FailureDefinition> failureDefinitions, List<RunIncident> incidents)
    {
      FailureDefinitions = failureDefinitions;
      Incidents = incidents;
    }
    public static RunContext Create(List<FailureDefinition> failureDefinitions, FailureSet failureSet)
    {
      IncidentGroup ig = new IncidentGroup();
      ig.Incidents = failureSet.Incidents;
      RunIncidentGroup top = RunIncidentGroup.Create(ig);

      RunContext ret = new(failureDefinitions, top.Incidents);
      return ret;
    }
    public void Init()
    {
      // trigger-variables are initialized automatically
    }
  }
}
