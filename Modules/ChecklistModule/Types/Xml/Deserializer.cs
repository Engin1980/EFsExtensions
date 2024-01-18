using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel;
using EXmlLib;
using EXmlLib.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Eng.Chlaot.Modules.ChecklistModule.Types.Xml
{
  internal static class Deserializer
  {
    internal static CheckSet Deserialize(XDocument doc)
    {
      EXml<CheckSet> exml = CreateDeserializer();
      CheckSet ret = exml.Deserialize(doc);
      return ret;
    }

    private static EXml<CheckSet> CreateDeserializer()
    {
      EXml<CheckSet> ret = new();
      int index = 0;

      var oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(CheckDefinition))
        .WithIgnoredProperty(nameof(CheckDefinition.Bytes));
      ret.Context.ElementDeserializers.Insert(index++, oed);

      oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(CheckSet))
        .WithCustomPropertyDeserialization(
          nameof(CheckSet.Checklists),
          EXmlHelper.List.CreateForFlat<CheckList>("checklist"));
      ret.Context.ElementDeserializers.Insert(index++, oed);

      oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(CheckList))
        .WithIgnoredProperty(nameof(CheckList.EntrySpeechBytes))
        .WithIgnoredProperty(nameof(CheckList.ExitSpeechBytes))
        .WithCustomPropertyDeserialization(
          nameof(CheckList.Items),
          EXmlHelper.List.CreateForNested<CheckItem>("items", "item"))
        .WithCustomPropertyDeserialization(
          nameof(CheckList.Variables),
          EXmlHelper.List.CreateForNested("variables", new EXmlHelper.List.DT[]
          {
            new EXmlHelper.List.DT("userVariable", typeof(UserVariable)),
            new EXmlHelper.List.DT("randomVariable", typeof(RandomVariable))
          },
          () => new List<Variable>()));
        //.WithCustomPropertyDeserialization(
        //  nameof(CheckList.NextChecklistIds),
        //  (e, t, p, c) =>
        //  {
        //    string? val = e.LElementOrNull("nextChecklistId")?.Attribute("id")?.Value;
        //    EXmlHelper.SetPropertyValue(p, t, val);
        //  });
      ret.Context.ElementDeserializers.Insert(index++, oed);

      ret.Context.ElementDeserializers.Insert(index++, new StateCheckDeserializer());

      return ret;
    }
  }
}
