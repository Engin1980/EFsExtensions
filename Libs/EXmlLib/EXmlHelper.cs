using EXmlLib.Deserializers;
using EXmlLib.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EXmlLib
{
  public class EXmlHelper
  {
    public static class Property
    {
      public static ObjectElementDeserializer.PropertyDeserializeHandler Create(string elementName, Type targetType)
      {
        ObjectElementDeserializer.PropertyDeserializeHandler ret = (e, t, f, c) =>
        {
          XElement sourceElement = e.LElement(elementName);
          var deser = c.ResolveElementDeserializer(targetType);
          object obj = deser.Deserialize(sourceElement, targetType, c);

          EXmlHelper.SetPropertyValue(f, t, obj);
        };

        return ret;
      }
    }

    public static class List
    {
      public delegate T Producer<T>();

      public record DT(string ElementName, Type TargetType);

      public static ObjectElementDeserializer.PropertyDeserializeHandler CreateForFlat<T>(DT[] elementTypeMappings)
      {
        ObjectElementDeserializer.PropertyDeserializeHandler ret = (e, t, f, c) =>
        {
          List<T>? targetValue = LoadListFromElements<T>(elementTypeMappings, e, c);
          EXmlHelper.SetPropertyValue(f, t, targetValue);
        };

        return ret;
      }

      public static ObjectElementDeserializer.PropertyDeserializeHandler CreateForNested<T>(
        string collectionPath,
        DT[] elementTypeMappings,
        Producer<List<T>>? defaultProducerIfCollectionPathNotFound = null)
      {
        ObjectElementDeserializer.PropertyDeserializeHandler ret = (e, t, f, c) =>
        {
          List<T>? targetValue;
          XElement? sourceElement;
          sourceElement = e.LElementOrNull(collectionPath);

          if (sourceElement != null)
          {
            targetValue = LoadListFromElements<T>(elementTypeMappings, sourceElement, c);
            EXmlHelper.SetPropertyValue(f, t, targetValue);
          }
          else if (defaultProducerIfCollectionPathNotFound != null)
            targetValue = defaultProducerIfCollectionPathNotFound.Invoke();
          else
            throw new EXmlException(
              $"Trying to deserialize XML nested-collection, but source element '{collectionPath}' " +
              $"not found in the parent element '{e.Name}'.");

        };
        return ret;
      }

      public static ObjectElementDeserializer.PropertyDeserializeHandler CreateForNested<T>(
        string collectionPath, DT elementTypeMapping, Producer<List<T>>? defaultProducerIfCollectionPathNotFound = null)
      {
        var ret = CreateForNested<T>(collectionPath, new DT[] { elementTypeMapping }, defaultProducerIfCollectionPathNotFound);
        return ret;
      }

      public static ObjectElementDeserializer.PropertyDeserializeHandler CreateForFlat<T>(DT elementTypeMapping)
      {
        return CreateForFlat<T>(new DT[] { elementTypeMapping });
      }

      public static ObjectElementDeserializer.PropertyDeserializeHandler CreateForFlat<T>(string elementName)
      {
        return CreateForFlat<T>(new DT(elementName, typeof(T)));
      }

      private static List<T> LoadListFromElements<T>(DT[] mappings, XElement listElement, EXmlContext c)
      {
        List<T> ret = new List<T>();
        foreach (var mapping in mappings)
        {
          var tmp = LoadListFromElements<T>(mapping, listElement, c);
          //tmp.ForEach(q => ret.Add(q));
          ret.AddRange(tmp);
        }
        return ret;
      }

      private static List<T> LoadListFromElements<T>(DT mapping, XElement listElement, EXmlContext c)
      {
        var deser = c.ResolveElementDeserializer(mapping.TargetType);
        var elements = listElement.LElements(mapping.ElementName);
        var tmp = elements
        .Select(q => EXmlHelper.Deserialize(q, mapping.TargetType, deser, c))
        .Cast<T>()
        .ToList();
        return tmp;
      }
    }

    public static T Cast<T>(object obj)
    {
      T ret;
      try
      {
        ret = (T)obj;
      }
      catch (Exception ex)
      {
        throw new EXmlException($"Failed to cast value '{obj} to type '{typeof(T)}'.", ex);
      }
      return ret;
    }

    public static object CreateInstance(IFactory factory, Type targetType)
    {
      object ret;
      try
      {
        ret = factory.CreateInstance(targetType);
      }
      catch (Exception ex)
      {
        throw new EXmlException($"Failed to create instance of '{targetType}' using '{factory.GetType()}'.", ex);
      }
      return ret;
    }

    public static object Deserialize(XElement elm, Type targetType, IElementDeserializer deserializer, EXmlContext context)
    {
      object ret;
      try
      {
        ret = deserializer.Deserialize(elm, targetType, context);
      }
      catch (Exception ex)
      {
        throw new EXmlException($"Failed to deserialize type '{targetType}' from element '{elm.Name}' " +
          $"using '{deserializer.GetType()}'.", ex);
      }
      return ret;
    }

    public static object Deserialize(XAttribute attr, Type targetType, IAttributeDeserializer deserializer)
    {
      object ret;
      try
      {
        ret = deserializer.Deserialize(attr, targetType);
      }
      catch (Exception ex)
      {
        throw new EXmlException($"Failed to deserialize type '{targetType}' from attribute '{attr.Name}' " +
          $"using '{deserializer.GetType()}'.", ex);
      }
      return ret;
    }

    public static void SetPropertyValue(PropertyInfo prop, object target, object? val)
    {
      try
      {
        prop.SetValue(target, val);
      }
      catch (Exception ex)
      {
        throw new EXmlException($"Failed to set property '{prop.DeclaringType}.{prop.Name} = {val}:", ex);
      }
    }
  }
}
