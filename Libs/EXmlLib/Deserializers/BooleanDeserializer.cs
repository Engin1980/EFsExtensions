using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EXmlLib.Deserializers
{
  public class BooleanDeserializer : IElementDeserializer, IAttributeDeserializer
  {
    public bool AcceptsType(Type type)
    {
      return type == typeof(bool);
    }

    public object Deserialize(XAttribute attribute, Type targetType)
    {
      bool ret = ConverToBool(attribute.Value);
      return ret;
    }

    public object Deserialize(XElement element, Type targetType, EXmlContext context)
    {
      bool ret = ConverToBool(element.Value);
      return ret;
    }

    private bool ConverToBool(string value)
    {
      if (value == "1" || value.ToLower() == "true")
        return true;
      else if (value == "0" || value.ToLower().Equals("false"))
        return false;
      else
        throw new EXmlException($"Failed to convert value {value} to bool using BooleanDeserializer.");
    }
  }
}
