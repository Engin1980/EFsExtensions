using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using EXmlLib.Deserializers;
using EXmlLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Eng.Chlaot.Modules.FailuresModule.Model.Failures;

namespace Eng.Chlaot.Modules.FailuresModule.Model.Incidents.Xml
{
  internal static class Deserialization
  {
    #region Private Classes

    private class PercentageDeserializer : IAttributeDeserializer
    {
      #region Public Methods

      public bool AcceptsType(Type type)
      {
        return type == typeof(Percentage);
      }

      public object Deserialize(XAttribute attribute, Type targetType)
      {
        string tmp = attribute.Value;
        if (double.TryParse(tmp, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("en-US"), out double res) == false)
        {
          throw new ApplicationException($"Percentage-deserialzer failed to deserialize percentage value from attribute {attribute.Name} with value {attribute.Value}.");
        }
        Percentage ret = (Percentage)res;
        return ret;
      }

      #endregion Public Methods
    }

    #endregion Private Classes

    #region Private Fields

    private const string FAILURES_KEY = "__failures";

    #endregion Private Fields

    #region Public Methods

    public static IncidentGroup Deserialize(XElement element, List<FailureDefinition> failures)
    {
      var der = CreateDeserializer(failures);
      IncidentGroup ret = der.Deserialize(element);
      return ret;
    }

    public static EXml<IncidentGroup> CreateDeserializer(List<FailureDefinition> failures)
    {
      EXml<IncidentGroup> ret = new();

      Dictionary<string, FailureDefinition> failDict = failures.ToDictionary(q => q.Id, q => q);
      ret.Context.CustomData[FAILURES_KEY] = failDict;
      int index = 0;
      ret.Context.ElementDeserializers.Insert(index++, CreateIncidentGroupDeserializer());
      ret.Context.ElementDeserializers.Insert(index++, CreateIncidentDefinitionDeserializer());
      ret.Context.ElementDeserializers.Insert(index++, CreateCheckStateTriggerDeserializer()); // this one should work as default
      ret.Context.ElementDeserializers.Insert(index++, CreateTimeTriggerDeserializer());
      ret.Context.ElementDeserializers.Insert(index++, new StateCheckDeserializer());
      ret.Context.ElementDeserializers.Insert(index++, CreateFailureDeserializer());
      ret.Context.ElementDeserializers.Insert(index++, CreateFailGroupDeserializer());

      index = 0;
      ret.Context.AttributeDeserializers.Insert(index++, new PercentageDeserializer());

      return ret;
    }

    #endregion Public Methods

    #region Private Methods

    private static IElementDeserializer CreateFailGroupDeserializer()
    {
      ObjectElementDeserializer ret = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(FailGroup))
        .WithCustomPropertyDeserialization(
          nameof(FailGroup.Items),
          EXmlHelper.List.CreateForFlat<Fail>(
            new EXmlHelper.List.DT[]
            {
              new EXmlHelper.List.DT("failure", typeof(FailId)),
              new EXmlHelper.List.DT("failures", typeof(FailGroup))
             }));
      return ret;
    }

    private static IElementDeserializer CreateFailureDeserializer()
    {
      ObjectElementDeserializer ret = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(FailId));
      return ret;
    }

    private static IElementDeserializer CreateCheckStateTriggerDeserializer()
    {
      ObjectElementDeserializer ret = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(CheckStateTrigger))
        .WithCustomPropertyDeserialization(
          nameof(CheckStateTrigger.Condition), (e, t, f, c) =>
          {
            var deser = new StateCheckDeserializer();
            var val = deser.Deserialize(e, typeof(IStateCheckItem), c);
            EXmlHelper.SetPropertyValue(f, t, val);
          });
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
          nameof(IncidentDefinition.Trigger),
          (e, t, f, c) =>
          {
            var te = e.LElementOrNull("trigger");
            var tte = e.LElementOrNull("timeTrigger");
            if (te != null)
            {
              var des = c.ResolveElementDeserializer(typeof(CheckStateTrigger));
              var val = des.Deserialize(te, typeof(CheckStateTrigger), c);
              EXmlHelper.SetPropertyValue(f, t, val);
            }
            else if (tte != null)
            {
              var des = c.ResolveElementDeserializer(typeof(TimeTrigger));
              var val = des.Deserialize(tte, typeof(TimeTrigger), c);
              EXmlHelper.SetPropertyValue(f, t, val);
            }
            else
              throw new ApplicationException($"Unable to find trigger definition for element {e}.");
          })
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
          nameof(IncidentGroup.Incidents),
          EXmlHelper.List.CreateForFlat<Incident>(new EXmlHelper.List.DT[]
          {
            new EXmlHelper.List.DT("incident", typeof(IncidentDefinition)),
            new EXmlHelper.List.DT("group", typeof(IncidentGroup))
          }));

      return ret;
    }

    private static IElementDeserializer CreateTimeTriggerDeserializer()
    {
      ObjectElementDeserializer ret = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(TimeTrigger))
        .WithIgnoredProperty(nameof(TimeTrigger.Probability));
      return ret;
    }

    #endregion Private Methods
  }
}
