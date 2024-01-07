using ELogging;
using EXmlLib.Attributes;
using EXmlLib.Factories;
using EXmlLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
          val = EXmlHelper.Deserialize(elm, propertyInfo.PropertyType, deserializer, context);
        }
        else // if (attr != null)
        {
          IAttributeDeserializer deserializer = context.ResolveAttributeDeserializer(propertyInfo.PropertyType)
            ?? throw new EXmlException($"Unable to find attribute deserializer for type '{propertyInfo.PropertyType}'.");
          val = EXmlHelper.Deserialize(attr!, propertyInfo.PropertyType, deserializer);
        }
        EXmlHelper.SetPropertyValue(propertyInfo, target, val);

        if (propertyInfo.PropertyType == typeof(string) && propertyInfo.GetCustomAttribute(typeof(EXmlNonemptyString)) != null)
        {
          string vals = (string)val;
          if (string.IsNullOrEmpty(vals))
            throw new EXmlException(
              $"Property {propertyInfo.DeclaringType?.Name}.{propertyInfo.Name} needs to be non-empty string, but provided value is {val}.");
        }
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
      IXmlLineInfo xmlLineInfo = element;
      Logger.Log(this, LogLevel.INFO, $"Loading {targetType.FullName} from {element.Name} (row {xmlLineInfo.LineNumber})");
      object ret;
      string targetTypeName = targetType.Name;
      IFactory factory = context.TryResolveFactory(targetType) ?? context.DefaultObjectFactory;
      ret = EXmlHelper.CreateInstance(factory, targetType);
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

      if (ret is IXmlObjectPostDeserialize iopd)
        try
        {
          iopd.PostDeserialize();
        }
        catch (Exception ex)
        {
          throw new EXmlException($"Invocation of post-deserialize of {iopd.GetType().Name} failed.", ex);
        }

      return ret;
    }
  }
}
