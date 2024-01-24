using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.VMs;
using Eng.Chlaot.Modules.FailuresModule.Model.Incidents;
using Eng.Chlaot.Modules.FailuresModule.Model.VMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.FailuresModule.Model.VMs
{
  internal class IncidentDefinitionVM : IncidentVM
  {

    public VariableVMS Variables
    {
      get => base.GetProperty<VariableVMS>(nameof(Variables))!;
      set => base.UpdateProperty(nameof(Variables), value);
    }

    public TriggerVMS Triggers
    {
      get => base.GetProperty<TriggerVMS>(nameof(Triggers))!;
      set => base.UpdateProperty(nameof(Triggers), value);
    }

    public List<object> VMItems
    {
      get => base.GetProperty<List<object>>(nameof(VMItems))!;
      set => base.UpdateProperty(nameof(VMItems), value);
    }

    public IncidentDefinitionVM(IncidentDefinition incidentDefinition, Func<Dictionary<string, double>> propertyValuesProvider)
    {
      IncidentDefinition = incidentDefinition;
      this.Variables = VariableVMS.Create(this.IncidentDefinition.Variables);
      this.Triggers = new(incidentDefinition.Triggers.Select(q => new TriggerVM(q, this.Variables.GetAsDict, propertyValuesProvider)));

      this.VMItems = new List<object>
      {
        this.Variables,
        this.Triggers,
        this.IncidentDefinition.FailGroup
      };
    }

    public IncidentDefinition IncidentDefinition { get; }

    public List<TriggerVM> InvokedOneShotTriggers { get; } = new List<TriggerVM>();

  }
}
