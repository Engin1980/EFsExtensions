using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types
{
  public class IncidentGroup
  {
        public List<IncidentGroup> Groups { get; set; }
        public List<Incident> Incidents { get; set; }
    }
}
