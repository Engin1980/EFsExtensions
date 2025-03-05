using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using Eng.EFsExtensions.Modules.FailuresModule.Model.Incidents;

namespace Eng.EFsExtensions.Modules.FailuresModule
{
  public class TriggerTemplateSelector : DataTemplateSelector
  {
    public DataTemplate StateCheckTriggerTemplate { get; set; } = null!;
    public DataTemplate TimeTriggerTemplate { get; set; } = null!;

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      DataTemplate ret;

      Model.Incidents.Trigger trigger = (Model.Incidents.Trigger)item;
      if (trigger is CheckStateTrigger)
        ret = StateCheckTriggerTemplate;
      else if (trigger is TimeTrigger)
        ret = TimeTriggerTemplate;
      else
        throw new NotImplementedException();

      return ret;
    }
  }
}
