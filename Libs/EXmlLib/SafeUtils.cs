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
  public class SafeUtils
  {
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
