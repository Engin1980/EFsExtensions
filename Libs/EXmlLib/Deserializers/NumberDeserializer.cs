using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EXmlLib.Deserializers
{
  public class NumberDeserializer : IElementDeserializer, IAttributeDeserializer
  {
    private static Dictionary<Type, Func<double, object>> supportedTypes = new()
    {
      { typeof(int), q => (int) q },
      { typeof(long), q => (long) q },
      { typeof(float), q => (float) q },
      { typeof(double), q => (double) q },
    };
    public bool AcceptsType(Type type)
    {
      return supportedTypes.ContainsKey(type);
    }

    public object Deserialize(XAttribute attribute, Type targetType)
    {
      double val = double.Parse(attribute.Value, System.Globalization.CultureInfo.GetCultureInfo("en-US"));
      object ret = ConvertToTargetType(val, targetType);
      return ret;
    }

    public object Deserialize(XElement element, Type targetType, EXmlContext context)
    {
      double val = double.Parse(element.Value, System.Globalization.CultureInfo.GetCultureInfo("en-US"));
      object ret = ConvertToTargetType(val, targetType);
      return ret;
    }

    private object ConvertToTargetType(double value, Type targetType)
    {
      object ret = supportedTypes[targetType].Invoke(value);
      return ret;
    }
  }
}
