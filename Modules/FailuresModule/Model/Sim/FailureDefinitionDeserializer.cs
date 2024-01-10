using ChlaotModuleBase;
using ESystem;
using ESystem.Asserting;
using ESystem.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Linq;
using static ESystem.Functions;

namespace FailuresModule.Model.Sim
{
  public class FailureDefinitionDeserializer
  {

    public FailureDefinitionGroup Deserialize(string fileName)
    {
      XDocument doc = Try(
        () => XDocument.Load(fileName),
        ex => throw new ApplicationException($"Unable to load xml from '{fileName}'.", ex)
      );

      XElement root = doc.Root ?? throw new UnexpectedNullException();

      FailureDefinitionGroup ret = DeserializeGroup(root);
      return ret;
    }

    private void DeserializeSubElements(XElement element, List<FailureDefinitionBase> targetList)
    {
      FailureDefinitionBase fdb;
      List<FailureDefinition> items;

      foreach (var elm in element.Elements())
      {
        switch (elm.Name.LocalName)
        {
          case "simEvent":
            fdb = DeserializeEvent(elm);
            targetList.Add(fdb);
            break;
          case "simVarLeak":
            fdb = DeserializeLeak(elm);
            targetList.Add(fdb);
            break;
          case "simVarStuck":
            fdb = DeserializeStuck(elm);
            targetList.Add(fdb);
            break;
          case "simVarValue":
            fdb = DeserializeValue(elm);
            targetList.Add(fdb);
            break;
          case "simVarSneak":
            fdb = DeserializeSneak(elm);
            targetList.Add(fdb);
            break;
          case "simVarValueViaEvent":
            fdb = DeserializeValueViaEvent(elm);
            targetList.Add(fdb);
            break; 
          case "sequence":
            items = DeserializeSequence(elm);
            targetList.AddRange(items);
            break;
          case "definitions":
            fdb = DeserializeGroup(elm);
            targetList.Add(fdb);
            break;
          default:
            throw new NotImplementedException();
        }
      }
    }

    private FailureDefinitionBase DeserializeSneak(XElement elm)
    {
      string id, title, scp;
      (id, title, scp) = GetIdTitleScp(elm);
      SneakFailureDefinition ret = new(id, title, scp);
      ret.MinimalInitialSneakValue = GetAttribute(elm, "minimalInitialSneakValue").Pipe(ToDouble);
      ret.MaximalInitialSneakValue = GetAttribute(elm, "maximalInitialSneakValue").Pipe(ToDouble);
      ret.MinimalSneakAdjustPerSecond = GetAttribute(elm, "minimalSneakAdjustPerSecond").Pipe(ToDouble);
      ret.MaximalSneakAdjustPerSecond = GetAttribute(elm, "maximalSneakAdjustPerSecond").Pipe(ToDouble);
      ret.IsPercentageBased = GetAttribute(elm, "isPercentageBased").Pipe(bool.Parse);
      ret.Direction = GetAttribute(elm, "direction").Pipe(q => Enum.Parse<SneakFailureDefinition.EDirection>(q, true));
      ret.FinalValue = GetAttribute(elm, "finalValue").Pipe(ToDouble);
      ret.FinalFailureId = GetAttribute(elm, "finalFailureId");
      SetAttributeIfExists(elm, "tickIntervalInMS", q => int.Parse(q), q => ret.TickIntervalInMS = q);
      ret.EnsureValid();

      return ret;
    }

    private DoubleValueOrPercentage ToValueOrPercentage(string s)
    {
      DoubleValueOrPercentage ret;
      if (s.EndsWith("%") || s.ToLower().EndsWith("p"))
        ret = new(ToDouble(s[..^1]), true);
      else
        ret = new(ToDouble(s), false);
      return ret;
    }

    private FailureDefinitionGroup DeserializeGroup(XElement elm)
    {
      string title = GetAttribute(elm, "title");
      FailureDefinitionGroup ret = new(title);
      DeserializeSubElements(elm, ret.Items);
      return ret;
    }

    private List<FailureDefinition> DeserializeSequence(XElement elm)
    {
      List<FailureDefinition> ret = new();

      int from = GetAttribute(elm, "from").Pipe(int.Parse);
      int to = GetAttribute(elm, "to").Pipe(int.Parse);
      string varRef = GetAttributeOrDefault(elm, "varRef", "{index}");
      EAssert.IsTrue(from <= to);

      List<List<FailureDefinition>> subLists = new List<List<FailureDefinition>>();

      for (int i = from; i <= to; i++)
      {
        List<FailureDefinitionBase> items = new();
        DeserializeSubElements(elm, items);
        EAssert.IsTrue(items.None(q => q is FailureDefinitionGroup));
        var tmp = items
          .Cast<FailureDefinition>()
          .Tap(q => q.ExpandVariableIfExists(varRef, i))
          .ToList();
        subLists.Add(tmp);
      }

      if (subLists.Count > 0)
      {
        for (int i = 0; i < subLists[0].Count; i++)
        {
          foreach (var subList in subLists)
          {
            ret.Add(subList[i]);
          }
        }
      }

      return ret;
    }

    private FailureDefinition DeserializeStuck(XElement elm)
    {
      string id, title, scp;
      (id, title, scp) = GetIdTitleScp(elm);
      StuckFailureDefinition ret = new(id, title, scp);
      SetAttributeIfExists(elm, "refreshIntervalInMs", q => int.Parse(q), q => ret.RefreshIntervalInMs = q);
      SetAttributeIfExists(elm, "onlyOnDetectedChange", q => bool.Parse(q), q => ret.OnlyUpdateOnDetectedChange = q);
      ret.EnsureValid();

      return ret;
    }

    private FailureDefinition DeserializeValueViaEvent(XElement elm)
    {
      string id, title, svcp;
      (id, title, svcp) = GetIdTitleScp(elm);
      string secp = GetAttribute(elm, "simEvt");
      SimVarViaEventFailureDefinition ret = new(id, title, svcp, secp);
      SetAttributeIfExists(elm, "refreshIntervalInMs", q => int.Parse(q), q => ret.RefreshIntervalInMs = q);
      SetAttributeIfExists(elm, "onlyOnDetectedChange", q => bool.Parse(q), q => ret.OnlyUpdateOnDetectedChange = q);
      return ret;
    }

    private FailureDefinition DeserializeLeak(XElement elm)
    {
      string id, title, scp;
      (id, title, scp) = GetIdTitleScp(elm);
      LeakFailureDefinition ret = new(id, title, scp);
      SetAttributeIfExists(elm, "minLeakTicks", q => int.Parse(q), q => ret.MinimumLeakTicks = q);
      SetAttributeIfExists(elm, "maxLeakTicks", q => int.Parse(q), q => ret.MaximumLeakTicks = q);
      SetAttributeIfExists(elm, "tickInterval", q => int.Parse(q), q => ret.TickLengthInMs = q);
      ret.EnsureValid();

      return ret;
    }

    private FailureDefinition DeserializeValue(XElement elm)
    {
      string id, title, scp;
      (id, title, scp) = GetIdTitleScp(elm);
      SimVarFailureDefinition ret = new(id, title, scp);
      SetAttributeIfExists(elm, "okValue", q => ToDouble(q), q => ret.OkValue = q);
      SetAttributeIfExists(elm, "failValue", q => ToDouble(q), q => ret.FailValue = q);

      return ret;
    }

    private static double ToDouble(string txt) => double.Parse(txt, System.Globalization.CultureInfo.GetCultureInfo("en-US"));

    private static void SetAttributeIfExists<T>(XElement elm, string attributeName, Func<string, T> conveter, Action<T> setter)
    {
      var attr = elm.Attribute(attributeName);
      if (attr != null)
      {
        string val = attr.Value;
        T converted = conveter(val);
        setter(converted);
      }
    }

    private FailureDefinition DeserializeEvent(XElement elm)
    {
      string id, title, scp;
      (id, title, scp) = GetIdTitleScp(elm, "simEvt");
      EventFailureDefinition ret = new(id, title, scp);
      return ret;
    }

    private (string, string, string) GetIdTitleScp(XElement elm, string simConPointAttributeName = "simVar")
    {
      string id = GetAttribute(elm, "id");
      string title = GetAttribute(elm, "title");
      string scp = GetAttribute(elm, simConPointAttributeName);
      return (id, title, scp);
    }

    private string GetAttributeOrDefault(XElement elm, string attributeName, string defaultValue)
    {
      if (elm.Attribute(attributeName) == null)
        return defaultValue;
      else
        return GetAttribute(elm, attributeName);
    }

    private string GetAttribute(XElement elm, string attributeName)
    {
      string ret = elm.Attribute(attributeName)?.Value ?? throw new ApplicationException($"Mandatory attribute '{attributeName}' not found.");
      return ret;
    }

  }
}
