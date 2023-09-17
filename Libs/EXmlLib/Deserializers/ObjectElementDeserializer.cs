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
    private readonly Dictionary<string, PropertyDeserializeHandler> customProperties = new();
    private Predicate<Type> predicate;

    public ObjectElementDeserializer WithCustomTargetType(Type type, bool includeInhreited = false)
    {
      if (type == null) throw new ArgumentNullException(nameof(type));
      Predicate<Type> predicate = includeInhreited
        ? q => type.IsAssignableFrom(q)
        : q => q == type;
      return this.WithCustomTargetTypeAcceptancy(predicate);
    }

    public ObjectElementDeserializer WithCustomTargetTypeAcceptancy(Predicate<Type> targetTypePredicate)
    {
      this.predicate = targetTypePredicate ?? throw new ArgumentNullException(nameof(targetTypePredicate));
      return this;
    }


    public ObjectElementDeserializer WithIgnoredProperty(
      string propertyName)
    {
      this.customProperties[propertyName] = (e, t, p, c) => { };
      return this;
    }

    public ObjectElementDeserializer WithIgnoredProperty(
      PropertyInfo propertyInfo)
    {
      return this.WithIgnoredProperty(propertyInfo.Name);
    }

    public ObjectElementDeserializer WithCustomPropertyDeserialization(
      PropertyInfo propertyInfo, PropertyDeserializeHandler handler)
    {
      return this.WithCustomPropertyDeserialization(propertyInfo.Name, handler);
    }

    public ObjectElementDeserializer WithCustomPropertyDeserialization(
      string propertyName, PropertyDeserializeHandler handler)
    {
      this.customProperties[propertyName] = handler ?? throw new ArgumentNullException(nameof(handler));
      return this;
    }

    public ObjectElementDeserializer()
    {
      this.predicate = t => t.IsAssignableTo(typeof(object));
    }

    public bool AcceptsType(Type type)
    {
      return predicate.Invoke(type);
    }

    protected void DeserializeProperty(XElement element, object target, PropertyInfo propertyInfo, EXmlContext context)
    {
      var propName = context.ResolvePropertyName(propertyInfo);
      XElement? elm = Utils.TryGetElementByName(element, propName);
      XAttribute? attr = Utils.TryGetAttributeByName(element, propName);
      if (elm != null || attr != null)
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

        if (customProperties.TryGetValue(prop.Name, out PropertyDeserializeHandler? pdh))
        {
          try
          {
            pdh.Invoke(element, ret, prop, context);
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
