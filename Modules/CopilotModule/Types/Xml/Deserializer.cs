using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.Modules.CopilotModule.Types;
using EXmlLib;
using EXmlLib.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Eng.Chlaot.CopilotModule.Types.Xml
{
  public static class Deserializer
  {
    internal static CopilotSet Deserialize(string xmlFile)
    {
      XDocument doc = XDocument.Load(xmlFile, LoadOptions.SetLineInfo);
      EXml<CopilotSet> exml = CreateDeserializer();
      CopilotSet ret = exml.Deserialize(doc);
      return ret;
    }

    private static EXml<CopilotSet> CreateDeserializer()
    {
      EXml<CopilotSet> ret = new();

      var oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(CopilotSet))
        .WithCustomPropertyDeserialization(
        nameof(CopilotSet.SpeechDefinitions),
        EXmlHelper.List.CreateForFlat<SpeechDefinition>("speechDefinition"));
      ret.Context.ElementDeserializers.Insert(0, oed);

      oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(SpeechDefinition))
        .WithCustomPropertyDeserialization(
          nameof(SpeechDefinition.Variables),
          EXmlHelper.List.CreateForNested(
            "variables",
            new EXmlHelper.List.DT[] {
              new EXmlHelper.List.DT("userVariable", typeof(UserVariable)),
            new EXmlHelper.List.DT("randomVariable", typeof(RandomVariable))},
            () => new List<Variable>()));
      ret.Context.ElementDeserializers.Insert(0, oed);



      oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(Speech))
        .WithIgnoredProperty(nameof(Speech.Bytes));
      ret.Context.ElementDeserializers.Insert(0, oed);

      ret.Context.ElementDeserializers.Insert(0, new StateCheckDeserializer());

      return ret;
    }
  }
}
