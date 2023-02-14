using EXmlLib.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EXmlLib
{
  public static class ObjectUtils
  {
    //public static void DeserializeProperties(XElement element, object target, PropertyInfo[] properties, EXmlContext context)
    //{
    //  foreach (var property in properties)
    //  {
    //    DeserializeProperty(element, target, property, context);
    //  }
    //}

    //public static void DeserializeProperty(XElement element, object ret, string propertyName, EXmlContext context)
    //{
    //  PropertyInfo propertyInfo = ret.GetType().GetProperty(propertyName) 
    //    ?? throw new EXmlException($"Unable to find '{ret.GetType()}'.{propertyName}' property.");

    //  DeserializeProperty(element, ret, propertyInfo, context);
    //}

    //public static void DeserializeProperty(XElement element, object ret, PropertyInfo prop, EXmlContext context)
    //{
      
    //}
  }
}
