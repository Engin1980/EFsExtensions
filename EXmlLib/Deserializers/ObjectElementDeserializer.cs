using EXmlLib.Attributes;
using EXmlLib.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EXmlLib.Deserializers
{
  public class ObjectElementDeserializer : IElementDeserializer
  {
    public delegate void CustomPropertyDeserializationDelegate();

    public Dictionary<string, CustomPropertyDeserializationDelegate> CustomProperties { get; private set; }
    public Predicate<Type> AcceptsTypePredicate { get; set; }

    public ObjectElementDeserializer()
    {
      this.CustomProperties = new();
      this.AcceptsTypePredicate = t => t.IsAssignableTo(typeof(object));
    }


    public ObjectElementDeserializer(
      Predicate<Type> acceptsType,
      Dictionary<string, CustomPropertyDeserializationDelegate> customPropertyDeserialization)
    {
      this.AcceptsTypePredicate = acceptsType ?? throw new ArgumentNullException(nameof(acceptsType));
      this.CustomProperties = customPropertyDeserialization ?? new();
    }


    public bool AcceptsType(Type type)
    {
      return AcceptsTypePredicate.Invoke(type);
    }

    public object Deserialize(XElement element, Type targetType, EXmlContext context)
    {
      object ret;
      IFactory factory = context.TryResolveFactory(targetType) ?? context.DefaultObjectFactory;
      ret = SafeUtils.CreateInstance(factory, targetType);
      var props = targetType.GetProperties(
        System.Reflection.BindingFlags.Instance
        | System.Reflection.BindingFlags.Public
        | System.Reflection.BindingFlags.NonPublic);
      foreach (var prop in props)
      {
        if (prop.GetCustomAttributes(true).Any(q => q is EXmlIgnore))
          continue;
        if (CustomProperties.ContainsKey(prop.Name))
        {
          try
          {
            CustomProperties[prop.Name].Invoke();
          }
          catch (Exception ex)
          {
            throw new EXmlException($"Failed to evaluate custom property '{targetType}.{prop.Name}' deserialization.", ex);
          }
        }
        else
        {
          var propName = context.ResolvePropertyName(prop);

          object val;
          XElement? elm;
          XAttribute? attr;

          if ((elm = Utils.TryGetElementByName(element, propName)) != null)
          {
            IElementDeserializer deserializer = context.ResolveElementDeserializer(prop.PropertyType)
              ?? throw new EXmlException($"Unable to find element deserializer for type '{prop.PropertyType}'");
            val = SafeUtils.Deserialize(elm, prop.PropertyType, deserializer, context);
            SafeUtils.SetPropertyValue(prop, ret, val);
          }
          else if ((attr = Utils.TryGetAttributeByName(element, prop.Name)) != null)
          {
            IAttributeDeserializer deserializer = context.ResolveAttributeDeserializer(prop.PropertyType)
              ?? throw new EXmlException($"Unable to find attribute deserializer for type '{prop.PropertyType}'.");
            val = SafeUtils.Deserialize(attr, prop.PropertyType, deserializer);
            SafeUtils.SetPropertyValue(prop, ret, val);
          }
          else
          {
            if (!context.IgnoreMissingProperties)
              throw new EXmlException($"Unable to find xml element or attribute for property '{prop.Name}'.");
          }
        }
      }

      return ret;
    }
  }
}
