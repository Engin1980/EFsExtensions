using ChlaotModuleBase;
using ChlaotModuleBase.ModuleUtils.WPF.VMs;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static ESystem.Functions;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.VMs
{
  public class PropertyVMS : GenericVMS<PropertyVM, SimProperty>
  {
    public PropertyVMS(IEnumerable<PropertyVM> properties)
      : base(properties, p => p.Property.Name, p => p.Property) { }

    public static PropertyVMS Create(IEnumerable<SimProperty> properties)
    {
      var tmp = properties.Select(q => new PropertyVM()
      {
        Property = q,
        Value = double.NaN
      });
      PropertyVMS ret = new(tmp);
      return ret;
    }
  }
}
