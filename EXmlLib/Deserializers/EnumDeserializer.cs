using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EXmlLib.Deserializers
{
  public class EnumDeserializer : IElementDeserializer, IAttributeDeserializer
  {
    public bool AcceptsType(Type type)
    {
      return type.IsEnum;
    }

    public object Deserialize(XAttribute attribute, Type targetType)
    {
      string val = attribute.Value;
      object ret = ConvertStringToEnum(val, targetType);
      return ret;
    }

    public object Deserialize(XElement element, Type targetType, EXmlContext context)
    {
      string val = element.Value;
      object ret = ConvertStringToEnum(val, targetType);
      return ret;
    }

    private object ConvertStringToEnum(string val, Type targetType)
    {
      object ret = Enum.Parse(targetType, val, true);
      return ret;
    }
  }
}
