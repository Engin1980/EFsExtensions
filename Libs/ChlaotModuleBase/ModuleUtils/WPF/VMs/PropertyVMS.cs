using ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimObjects;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.VMs
{
  public class PropertyVMS : BindingList<BindingKeyValue<SimProperty, double>>, IPropertyValuesProvider
  {
    public static PropertyVMS Create(IEnumerable<SimProperty> simProperties)
    {
      PropertyVMS ret = new();
      foreach (var simProperty in simProperties)
      {
        ret.Add(new BindingKeyValue<SimProperty, double>(simProperty, double.NaN));
      }
      return ret;
    }

    public Dictionary<string, double> GetPropertyValues()
    {
      return this.ToDictionary(q => q.Key.Name, q => q.Value);
    }

    public void UpdateBySimObject(SimObject simObject)
    {
      var tmp = simObject.GetAllPropertiesWithValues();
      foreach (var item in tmp)
      {
        this[item.Key.Name] = item.Value;
      }
    }
  }
}
