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
    public Trigger Trigger { get; set; } = null!;
    public FailGroup FailGroup { get; set; } = null!;
    public bool Repetitive { get; set; }
  }
}
