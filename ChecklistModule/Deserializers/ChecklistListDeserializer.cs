//using ChecklistModule.Types;
//using EXmlLib;
//using EXmlLib.Deserializers;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Linq;

//namespace ChecklistModule.Deserializers
//{
//  internal class ChecklistListDeserializer : IElementDeserializer
//  {
//    public bool AcceptsType(Type type)
//    {
//      return type == typeof(List<CheckList>);
//    }

//    public object Deserialize(XElement element, Type targetType, EXmlContext context)
//    {
//      List<CheckList> ret = new List<CheckList>();
//      var elms = element.LElements("checklist");
//      foreach (var elm in elms)
//      {
//        var deserializer = context.ResolveElementDeserializer(typeof(CheckList));
//        object tmp = SafeUtils.Deserialize(elm, typeof(CheckList), deserializer, context);
//        CheckList checkList = SafeUtils.Cast<CheckList>(tmp);
//        ret.Add(checkList);
//      }
//      return ret;
//    }
//  }
//}
