using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimObjects;
using ESystem.Asserting;
using EXmlLib;
using EXmlLib.Deserializers;
using EXmlLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimObjects
{
  public class SimPropertyGroup : SimPropertyBase, IXmlObjectPostDeserialize
  {
    public List<SimPropertyBase> Properties { get; set; } = new();
    public string Title { get; set; } = string.Empty;

    public static SimPropertyGroup Deserialize(XElement elm)
    {
      EXml<SimPropertyGroup> des = new();

      ObjectElementDeserializer oed;
      oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(SimPropertyGroup))
        .WithCustomPropertyDeserialization(
        nameof(SimPropertyGroup.Properties),
        EXmlHelper.List.CreateForFlat<SimPropertyBase>(new EXmlHelper.List.DT[]
        {
          new EXmlHelper.List.DT("property", typeof(SimProperty)),
          new EXmlHelper.List.DT("properties", typeof(SimPropertyGroup))
        }));
      des.Context.ElementDeserializers.Insert(0, oed);

      SimPropertyGroup ret = des.Deserialize(elm);
      return ret;
    }

    public List<SimProperty> GetAllSimPropertiesRecursively()
    {
      List<SimProperty> ret = this.Properties
        .Where(q => q is SimProperty)
        .Cast<SimProperty>()
        .ToList();
      List<SimProperty> xps = this.Properties
        .Where(q => q is SimPropertyGroup)
        .Cast<SimPropertyGroup>()
        .Select(q => q.GetAllSimPropertiesRecursively())
        .Cast<SimProperty>()
        .ToList();
      ret = ret.Union(xps).ToList();
      return ret;
    }

    public void PostDeserialize()
    {
      EAssert.IsNotNull(Properties);
    }
  }
}
