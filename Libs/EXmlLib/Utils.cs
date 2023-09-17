using EXmlLib.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EXmlLib
{
  public static class Utils
  {
    //public static void DeserializeProperties(XElement element, object target, string[] propertyNames, EXmlContext context)
    //{
    //  var props = target.GetType().GetProperties(System.Reflection.BindingFlags.Instance
    //    | System.Reflection.BindingFlags.Public
    //    | System.Reflection.BindingFlags.NonPublic);
    //  foreach (var propertyName in propertyNames)
    //  {
    //    var prop = props.First(q => q.Name == propertyName);
    //    var elm = element.Element(propertyName);
    //    var attr = element.Attribute(propertyName);
    //    if (elm != null)
    //    {
    //      var des = context.ResolveElementDeserializer(prop.PropertyType);
    //      var val = SafeUtils.Deserialize(element, prop.PropertyType, des, context);
    //      SafeUtils.SetPropertyValue(prop, target, val);
    //    }
    //    else if (attr != null)
    //    {
    //      IAttributeDeserializer deserializer = context.ResolveAttributeDeserializer(prop.PropertyType)
    //        ?? throw new EXmlException($"Unable to find attribute deserializer for type '{prop.PropertyType}'.");
    //      var val = SafeUtils.Deserialize(attr, prop.PropertyType, deserializer);
    //      SafeUtils.SetPropertyValue(prop, target, val);
    //    }
    //  }
    //}

    internal static XAttribute? TryGetAttributeByName(XElement element, string name)
    {
      var ret = element.Attributes()
        .FirstOrDefault(q => string.Equals(q.Name.LocalName, name, StringComparison.OrdinalIgnoreCase));
      return ret;
    }

    internal static XElement? TryGetElementByName(XElement element, string name)
    {
      var ret = element.Elements()
        .FirstOrDefault(q => string.Equals(q.Name.LocalName, name, StringComparison.OrdinalIgnoreCase));
      return ret;
    }
  }
}
