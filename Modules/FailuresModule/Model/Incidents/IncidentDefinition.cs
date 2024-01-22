using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.FailuresModule.Model.Incidents
{
    public class IncidentDefinition : Incident
    {
        public List<Variable> Variables { get; set; } = new List<Variable>();
        public List<Trigger> Triggers { get; set; } = new List<Trigger>();
        public FailGroup FailGroup { get; set; }
        public bool Repetitive { get; set; }
    }
}
