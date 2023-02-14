using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EXmlLib.Deserializers
{
  public class StringDeserializer : IElementDeserializer, IAttributeDeserializer
  {
    public bool AcceptsType(Type type)
    {
      return type == typeof(string);
    }

    public object Deserialize(XAttribute element, Type targetType)
    {
      return element.Value;
    }

    public object Deserialize(XElement element, Type targetType, EXmlContext context)
    {
      return element.Value;
    }
  }
}
