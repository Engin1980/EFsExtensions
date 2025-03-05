using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking.Exceptions;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking.StateModel;
using EXmlLib;
using EXmlLib.Deserializers;
using System;
using System.Linq;
using System.Windows.Navigation;
using System.Xml.Linq;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking
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
        "for" => DeserializeDelayFromElement(element, context),
        "true" or "false" => DeserializeTrueFalseFromElement(element),
        "wait" => DeserializeWaitFromElement(element, context),
        _ => throw new StateCheckDeserializationException($"Unknown/Unexpected element name '{elementName}'."),
      };
      return ret;
    }

    private StateCheckTrueFalse DeserializeTrueFalseFromElement(XElement element)
    {
      bool value = element.Name.LocalName == "true";
      StateCheckTrueFalse ret = new StateCheckTrueFalse(value);
      return ret;
    }

    private StateCheckWait DeserializeWaitFromElement(XElement element, EXmlContext context)
    {
      //TODO this should be done via EXml
      string s = element.Attribute("seconds")?.Value
                 ?? throw new StateCheckDeserializationException("Argument 'seconds' is missing.");
      IStateCheckItem val = DeserializeElement(element.Elements().First(), typeof(IStateCheckItem), context);
      StateCheckWait ret = new()
      {
        Seconds = s,
        Item = val
      };
      ret.PostDeserialize();
      return ret;
    }

    private StateCheckDelay DeserializeDelayFromElement(XElement element, EXmlContext context)
    {
      //TODO this should be done via EXml
      string s = element.Attribute("seconds")?.Value
                 ?? throw new StateCheckDeserializationException("Argument 'seconds' is missing.");
      IStateCheckItem val = DeserializeElement(element.Elements().First(), typeof(IStateCheckItem), context);
      StateCheckDelay ret = new()
      {
        Seconds = s,
        Item = val
      };
      ret.PostDeserialize();
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
        .WithIgnoredProperty(nameof(StateCheckProperty.DisplayString))
        .WithCustomPropertyDeserialization(nameof(StateCheckProperty.Randomness), (e, t, p, c) =>
        {
          string tmp = e.Attribute("randomness")?.Value ?? "+-0";
          StateCheckPropertyDeviation scpd = StateCheckPropertyDeviation.Parse(tmp);
          EXmlHelper.SetPropertyValue(p, t, scpd);
        })
        .WithCustomPropertyDeserialization(nameof(StateCheckProperty.Sensitivity), (e, t, p, c) =>
        {
          string tmp = e.Attribute("sensitivity")?.Value ?? "+-10%";
          StateCheckPropertyDeviation scpd = StateCheckPropertyDeviation.Parse(tmp);
          EXmlHelper.SetPropertyValue(p, t, scpd);
        });
      StateCheckProperty ret = (StateCheckProperty)EXmlHelper.Deserialize(
        element, typeof(StateCheckProperty), deser, context);
      return ret;
    }
  }
}
