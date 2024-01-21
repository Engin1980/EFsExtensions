using ChlaotModuleBase;
using ChlaotModuleBase.ModuleUtils.WPF.VMs;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.VMs
{
  public class PropertyVMS : GenericVMS<SimProperty>
  {
    public PropertyVMS(IEnumerable<SimProperty> properties) : base(properties, (SimProperty p) => p.Name) { }

    public void UpdateBySimObject(SimObject simObject)
    {
      var tmp = simObject.GetAllPropertiesWithValues();
      tmp.ForEach(q => this[q.Key] = q.Value);
    }
  }
}
