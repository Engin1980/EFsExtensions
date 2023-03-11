using EXmlLib;
using EXmlLib.Deserializers;
using System;
using System.Linq;
using System.Windows.Navigation;
using System.Xml.Linq;

namespace ChlaotModuleBase.ModuleUtils.StateChecking
{
  public class StateCheckDeserializer : IElementDeserializer
  {
    public bool AcceptsType(Type type)
    {
      return type.IsAssignableTo(typeof(IStateCheckItem));
    }

    public object Deserialize(XElement element, Type targetType, EXmlContext context)
    {
      IStateCheckItem ret;

      ret = DeserializeElement(element.Elements().First(), targetType, context);

      return ret;
    }
    private IStateCheckItem DeserializeElement(XElement element, Type targetType, EXmlContext context)
    {
      string elementName = element.Name.LocalName;
      IStateCheckItem ret = elementName switch
      {
        "property" => DeserializePropertyFromElement(element, targetType, context),
        "and" or "or" => DeserializeConditionFromElement(element, targetType, context),
        "for" => DeserializeDelayfromElement(element, context),
        "true" or "false" => DeserializeTrueFalseFromElement(element),
        _ => throw new NotSupportedException($"Unknown element name '{elementName}'."),
      };
      return ret;
    }

    private StateCheckTrueFalse DeserializeTrueFalseFromElement(XElement element)
    {
      bool value = element.Name.LocalName == "true";
      StateCheckTrueFalse ret = new StateCheckTrueFalse(value);
      return ret;
    }

    private StateCheckDelay DeserializeDelayfromElement(XElement element, EXmlContext context)
    {
      string s = element.Attribute("seconds")?.Value
        ?? throw new ArgumentNullException("Argument 'seconds' is missing.");
      int seconds = int.Parse(s);

      IStateCheckItem val = DeserializeElement(element.Elements().First(), typeof(IStateCheckItem), context);

      StateCheckDelay ret = new()
      {
        Seconds = seconds,
        Item = val
      };
      return ret;
    }

    private IStateCheckItem DeserializeConditionFromElement(XElement element, Type targetType, EXmlContext context)
    {
      var op = element.Name.LocalName switch
      {
        "and" => StateCheckConditionOperator.And,
        "or" => StateCheckConditionOperator.Or,
        _ => throw new NotImplementedException()
      };
      var items = element.Elements().Select(q => DeserializeElement(q, targetType, context)).ToList();

      StateCheckCondition ret = new()
      {
        Operator = op,
        Items = items
      };
      return ret;
    }

    private IStateCheckItem DeserializePropertyFromElement(XElement element, Type targetType, EXmlContext context)
    {
      var deser = new ObjectElementDeserializer()
        .WithIgnoredProperty("DisplayName")
        .WithIgnoredProperty("DisplayString")
        .WithCustomPropertyDeserialization("randomize", (e, t, p, c) =>
        {
          string tmp = e.Attribute("randomize")?.Value ?? "0";
          StateCheckPropertyValueDeviation scpvd = new(tmp);
          SafeUtils.SetPropertyValue(p, t, scpvd);
        })
        .WithCustomPropertyDeserialization("sensitivity", (e, t, p, c) =>
        {
          string tmp = e.Attribute("sensitivity")?.Value ?? "0";
          StateCheckPropertyValueDeviation scpvd = new(tmp);
          SafeUtils.SetPropertyValue(p, t, scpvd);
        });
      StateCheckProperty ret = (StateCheckProperty)SafeUtils.Deserialize(
        element, typeof(StateCheckProperty), deser, context);
      return ret;
    }
  }
}
