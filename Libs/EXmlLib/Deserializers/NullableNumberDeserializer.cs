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
  public class NullableNumberDeserializer : IElementDeserializer, IAttributeDeserializer
  {
    private static readonly Dictionary<Type, Func<double, object>> supportedTypes = new()
    {
      { typeof(int?), q => (int?) q },
      { typeof(long?), q => (long?) q },
      { typeof(float?), q => (float?) q },
      { typeof(double?), q => (double?) q },
    };
    public bool AcceptsType(Type type)
    {
      return supportedTypes.ContainsKey(type);
    }

    public object Deserialize(XAttribute attribute, Type targetType)
    {
      double? val = attribute.Value.ToLower().Contains("null") == false
        ? double.Parse(attribute.Value)
        : null;
      object? ret = ConvertToTargetType(val, targetType);
      return ret!;
    }

    public object Deserialize(XElement element, Type targetType, EXmlContext context)
    {
      double? val = element.Value.ToLower().Contains("null") == false
        ? double.Parse(element.Value)
        : null;
      object? ret = ConvertToTargetType(val, targetType);
      return ret!;
    }

    private static object? ConvertToTargetType(double? value, Type targetType)
    {
      object? ret = value.HasValue
        ? supportedTypes[targetType].Invoke(value.Value)
        : null;
      return ret;
    }
  }
}
