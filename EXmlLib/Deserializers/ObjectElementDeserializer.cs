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
    public delegate void PropertyDeserializeHandler(XElement element, object target, PropertyInfo propertyInfo, EXmlContext context);
    public Dictionary<string, PropertyDeserializeHandler> CustomProperties { get; private set; }
    public Predicate<Type> AcceptsTypePredicate { get; set; }


    public ObjectElementDeserializer()
    {
      this.AcceptsTypePredicate = t => t.IsAssignableTo(typeof(object));
      this.CustomProperties = new();
    }

    public ObjectElementDeserializer(
      Predicate<Type> acceptsType, Dictionary<string, PropertyDeserializeHandler> customPropertiesDeserializers)
    {
      this.AcceptsTypePredicate = acceptsType ?? throw new ArgumentNullException(nameof(acceptsType));
      this.CustomProperties = customPropertiesDeserializers ?? throw new ArgumentNullException(nameof(customPropertiesDeserializers));
    }

    public bool AcceptsType(Type type)
    {
      return AcceptsTypePredicate.Invoke(type);
    }

    protected void DeserializeProperty(XElement element, object target, PropertyInfo propertyInfo, EXmlContext context)
    {
      var propName = context.ResolvePropertyName(propertyInfo);
      XElement? elm = Utils.TryGetElementByName(element, propName);
      XAttribute? attr = Utils.TryGetAttributeByName(element, propertyInfo.Name);
      if (elm == null || attr == null)
      {
        object val;
        if (elm != null)
        {
          IElementDeserializer deserializer = context.ResolveElementDeserializer(propertyInfo.PropertyType)
            ?? throw new EXmlException($"Unable to find element deserializer for type '{propertyInfo.PropertyType}'");
          val = SafeUtils.Deserialize(elm, propertyInfo.PropertyType, deserializer, context);
        }
        else // if (attr != null)
        {
          IAttributeDeserializer deserializer = context.ResolveAttributeDeserializer(propertyInfo.PropertyType)
            ?? throw new EXmlException($"Unable to find attribute deserializer for type '{propertyInfo.PropertyType}'.");
          val = SafeUtils.Deserialize(attr!, propertyInfo.PropertyType, deserializer);
        }
        SafeUtils.SetPropertyValue(propertyInfo, target, val);
      }

      else
      {
        if (!context.IgnoreMissingProperties)
          throw new EXmlException($"Unable to find xml element or attribute for property '{propertyInfo.Name}'.");
        return;
      }
    }

    public object Deserialize(XElement element, Type targetType, EXmlContext context)
    {
      object ret;
      IFactory factory = context.TryResolveFactory(targetType) ?? context.DefaultObjectFactory;
      ret = SafeUtils.CreateInstance(factory, targetType);
      var props = targetType.GetProperties(
        BindingFlags.Instance
        | BindingFlags.Public
        | BindingFlags.NonPublic);
      foreach (var prop in props)
      {
        if (prop.GetCustomAttributes(true).Any(q => q is EXmlIgnore))
          continue;

        if (CustomProperties.ContainsKey(prop.Name))
        {
          try
          {
            CustomProperties[prop.Name].Invoke(element, ret, prop, context);
          }
          catch (Exception ex)
          {
            throw new EXmlException($"Failed to deserialize property '{prop.Name}' using custom deserializer.", ex);
          }
        }
        else
        {
          try
          {
            DeserializeProperty(element, ret, prop, context);
          }
          catch (Exception ex)
          {
            throw new EXmlException($"Failed to deserialize property '{prop.Name}'.", ex);
          }
        }
      }

      return ret;
    }
  }
}
