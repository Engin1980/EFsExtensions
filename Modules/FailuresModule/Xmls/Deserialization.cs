using ELogging;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using EXmlLib;
using EXmlLib.Deserializers;
using FailuresModule.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Linq;

namespace FailuresModule.Xmls
{
  public class Deserialization
  {
    private const string FAILURES_KEY = "__failures";
    private const string LOG_HANDLER_KEY = "__log_handler";

    private class PercentageDeserializer : IAttributeDeserializer
    {
      public bool AcceptsType(Type type)
      {
        return type == typeof(Percentage);
      }

      public object Deserialize(XAttribute attribute, Type targetType)
      {
        string tmp = attribute.Value;
        if (double.TryParse(tmp, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("en-US"), out double res) == false)
        {
          throw new EXmlException($"Percentage-deserialzer failed to deserialize percentage value from attribute {attribute.Name} with value {attribute.Value}.");
        }
        Percentage ret = (Percentage)res;
        return ret;
      }
    }

    public static EXml<FailureSet> CreateDeserializer(List<FailureDefinition> failures, NewLogHandler logHandler)
    {
      EXml<FailureSet> ret = new();

      Dictionary<string, FailureDefinition> failDict = failures.ToDictionary(q => q.Id, q => q);
      ret.Context.CustomData[FAILURES_KEY] = failDict;
      ret.Context.CustomData[LOG_HANDLER_KEY] = logHandler;
      int index = 0;
      ret.Context.ElementDeserializers.Insert(index++, CreateIncidentSetDeserializer());
      ret.Context.ElementDeserializers.Insert(index++, CreateIncidentGroupDeserializer());
      ret.Context.ElementDeserializers.Insert(index++, CreateIncidentDefinitionDeserializer());
      //ret.Context.ElementDeserializers.Insert(index++, CreateTriggerDeserializer()); // this one should work as default
      ret.Context.ElementDeserializers.Insert(index++, new StateCheckDeserializer());
      ret.Context.ElementDeserializers.Insert(index++, CreateFailureDeserializer());
      ret.Context.ElementDeserializers.Insert(index++, CreateFailGroupDeserializer());

      index = 0;
      ret.Context.AttributeDeserializers.Insert(index++, new PercentageDeserializer());

      return ret;
    }

    private static IElementDeserializer CreateFailGroupDeserializer()
    {
      ObjectElementDeserializer ret = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(FailGroup))
        .WithCustomPropertyDeserialization(
          nameof(FailGroup.Items),
          EXmlHelper.List.CreateForFlat<FailItem>(
            new EXmlHelper.List.DT[]
            {
              new EXmlHelper.List.DT("failure", typeof(Failure)),
              new EXmlHelper.List.DT("failGroup", typeof(FailGroup))
             }));
      return ret;
    }

    private static IElementDeserializer CreateFailureDeserializer()
    {
      ObjectElementDeserializer ret = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(Failure));
      return ret;
    }

    private static IElementDeserializer CreateIncidentSetDeserializer()
    {
      ObjectElementDeserializer ret = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(FailureSet))
        .WithCustomPropertyDeserialization(
        nameof(FailureSet.Incidents),
        EXmlHelper.List.CreateForFlat<Incident>(new EXmlHelper.List.DT[]
        {
          new EXmlHelper.List.DT("incident", typeof(IncidentDefinition)),
          new EXmlHelper.List.DT("group", typeof(IncidentGroup))
        }));
      return ret;
    }

    private static IElementDeserializer CreateIncidentDefinitionDeserializer()
    {
      IElementDeserializer ret = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(IncidentDefinition))
        .WithCustomPropertyDeserialization(
          nameof(IncidentDefinition.Variables),
          EXmlHelper.List.CreateForNested<Variable>(
            "variables",
            new EXmlHelper.List.DT[] {
              new EXmlHelper.List.DT("randomVariable", typeof(RandomVariable)),
              new EXmlHelper.List.DT("userVariable", typeof(UserVariable))},
            () => new List<Variable>()))
        .WithCustomPropertyDeserialization(
          nameof(IncidentDefinition.Triggers),
          EXmlHelper.List.CreateForNested<Trigger>(
            "triggers",
            new EXmlHelper.List.DT("trigger", typeof(Trigger))))
        .WithCustomPropertyDeserialization(
          nameof(IncidentDefinition.FailGroup),
          EXmlHelper.Property.Create("failures", typeof(FailGroup)));

      return ret;
    }

    private static IElementDeserializer CreateIncidentGroupDeserializer()
    {
      IElementDeserializer ret = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(IncidentGroup))
        .WithCustomPropertyDeserialization(
          nameof(FailureSet.Incidents),
          EXmlHelper.List.CreateForFlat<Incident>(new EXmlHelper.List.DT[]
          {
            new EXmlHelper.List.DT("incident", typeof(IncidentDefinition)),
            new EXmlHelper.List.DT("group", typeof(IncidentGroup))
          }));

      return ret;
    }
  }
}
