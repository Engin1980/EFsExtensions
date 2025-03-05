using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.VMs;
using Eng.EFsExtensions.Modules.FailuresModule.Model.Incidents;
using Eng.EFsExtensions.Modules.FailuresModule.Model.VMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FailuresModule.Model.VMs
{
  internal class IncidentDefinitionVM : IncidentVM
  {

    public VariableVMS Variables
    {
      get => base.GetProperty<VariableVMS>(nameof(Variables))!;
      set => base.UpdateProperty(nameof(Variables), value);
    }

    public TriggerVM Trigger
    {
      get => base.GetProperty<TriggerVM>(nameof(Trigger))!;
      set => base.UpdateProperty(nameof(Trigger), value);
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
      this.Trigger = new TriggerVM(incidentDefinition.Trigger, this.Variables.GetAsDict, propertyValuesProvider);

      this.VMItems = new List<object>
      {
        this.Variables,
        this.Trigger,
        this.IncidentDefinition.FailGroup
      };
    }

    public IncidentDefinition IncidentDefinition { get; }

    public bool IsOneShotTriggerInvoked { get; set; } = false;

  }
}
