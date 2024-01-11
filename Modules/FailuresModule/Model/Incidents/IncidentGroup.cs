using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Model.Incidents
{
    public class IncidentGroup : Incident
    {
        public List<Incident> Incidents { get; set; }
    }
}
