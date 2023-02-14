using ChecklistModule.Types;
using EXmlLib;
using EXmlLib.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ChecklistModule.Deserializers
{
  public class CheckSetDeserializer : IElementDeserializer
  {
    public bool AcceptsType(Type type)
    {
      return type == typeof(CheckSet);
    }

    public object Deserialize(XElement element, Type targetType, EXmlContext context)
    {
      CheckSet ret = new CheckSet();

      //Utils.DeserializeProperties(element, ret, new string[] { nameof(CheckSet.Meta) });
      ObjectUtils.DeserializeProperty(element, ret, "Meta", context);

      ret.Checklists = new();
      var elms = element.LElements("checklist");
      var des = context.ResolveElementDeserializer(typeof(CheckList));
      foreach (XElement elm in elms)
      {
        object tmp = SafeUtils.Deserialize(elm, typeof(CheckList), des, context);
        CheckList checkList = SafeUtils.Cast<CheckList>(tmp);
        ret.Checklists.Add(checkList);
      }

      return ret;
    }
  }
}
