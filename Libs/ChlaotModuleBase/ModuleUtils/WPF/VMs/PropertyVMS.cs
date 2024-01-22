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
  public class PropertyVMS : GenericVMS<PropertyVM>
  {
    public PropertyVMS(IEnumerable<PropertyVM> properties) : base(properties, (PropertyVM p) => p.Property.Name) { }

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

    public double this[SimProperty property]
    {
      get => this.Single(q => q.Property.Equals(property)).Value;
      set => Try(
        () => this.Single(q => q.Property.Equals(property)).Value = value, 
        ex => new ApplicationException($"Property '{property.Name}' not found.", ex));
    }

    public void UpdateBySimObject(SimObject simObject)
    {
      var tmp = simObject.GetAllPropertiesWithValues();
      tmp.ForEach(q => this[q.Key] = q.Value);
    }
  }
}
