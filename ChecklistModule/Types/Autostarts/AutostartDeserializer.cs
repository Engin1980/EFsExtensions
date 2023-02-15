using EXmlLib;
using EXmlLib.Deserializers;
using System;
using System.Linq;
using System.Windows.Navigation;
using System.Xml.Linq;

namespace ChecklistModule.Types.Autostarts
{
  public class AutostartDeserializer : IElementDeserializer
  {
    public bool AcceptsType(Type type)
    {
      return type.IsAssignableTo(typeof(IAutostart));
    }

    public object Deserialize(XElement element, Type targetType, EXmlContext context)
    {
      IAutostart ret;

      ret = DeserializeElement(element.Elements().First(), targetType, context);

      return ret;
    }
    private IAutostart DeserializeElement(XElement element, Type targetType, EXmlContext context)
    {
      string elementName = element.Name.LocalName;
      IAutostart ret = elementName switch
      {
        "property" => DeserializePropertyFromElement(element, targetType, context),
        "and" or "or" => DeserializeConditionFromElement(element, targetType, context),
        "for" => DeserializeDelayfromElement(element, targetType, context),
        _ => throw new NotSupportedException($"Unknown autostart element name '{elementName}'."),
      };
      return ret;
    }

    private IAutostart DeserializeDelayfromElement(XElement element, Type targetType, EXmlContext context)
    {
      var seconds = int.Parse(element.Attribute("seconds")!.Value);

      var deser = context.ResolveElementDeserializer(typeof(IAutostart));
      IAutostart val = DeserializeElement(element.Elements().First(), typeof(IAutostart), context); //(IAutostart)SafeUtils.Deserialize(element.LElement("Item"), typeof(IAutostart), deser, context);

      AutostartDelay ret = new()
      {
        Seconds = seconds,
        Item = val
      };
      return ret;
    }

    private IAutostart DeserializeConditionFromElement(XElement element, Type targetType, EXmlContext context)
    {
      var op = element.Name.LocalName switch
      {
        "and" => AutostartConditionOperator.And,
        "or" => AutostartConditionOperator.Or,
        _ => throw new NotImplementedException()
      };
      var items = element.Elements().Select(q => DeserializeElement(q, targetType, context)).ToList();

      AutostartCondition ret = new()
      {
        Operator = op,
        Items = items
      };
      return ret;
    }

    private IAutostart DeserializePropertyFromElement(XElement element, Type targetType, EXmlContext context)
    {
      var deser = context.ResolveElementDeserializer(typeof(object));
      AutostartProperty ret = (AutostartProperty)SafeUtils.Deserialize(
        element, typeof(AutostartProperty), deser, context);
      return ret;
    }
  }
}
