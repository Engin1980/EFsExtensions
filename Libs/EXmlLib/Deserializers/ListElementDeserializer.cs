using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EXmlLib.Deserializers
{
  public class ListElementDeserializer : IElementDeserializer
  {
    public bool AcceptsType(Type type)
    {
      return type.IsAssignableTo(typeof(IList)) && type.IsGenericType;
    }

    public object Deserialize(XElement element, Type targetType, EXmlContext context)
    {
      var factory = context.ResolveFactory(targetType);
      var ret = EXmlHelper.CreateInstance(factory, targetType);

      var elms = element.Elements();
      var itemType = targetType.GetGenericArguments()[0];

      foreach (var elm in elms)
      {
        var des = context.ResolveElementDeserializer(itemType);
        var tmp = EXmlHelper.Deserialize(elm, itemType, des, context);
        AddItemToList(ret, tmp);
      }

      return ret;
    }

    private void AddItemToList(object lst, object item)
    {
      try
      {
        var method = lst.GetType().GetMethod("Add")
          ?? throw new EXmlException("Unable to find method 'Add' for list.");
        method.Invoke(lst, new object[] { item });
      }
      catch (Exception ex)
      {
        throw new EXmlException($"Failed to add item of type '{item}' into list of type '{lst.GetType()}'.", ex);
      }
    }
  }
}
