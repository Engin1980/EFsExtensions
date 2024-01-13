using Eng.Chlaot.Modules.AffinityModule;
using EXmlLib.Deserializers;
using EXmlLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.AffinityModule
{
  public static class Deserialization
  {
    public static EXml<RuleBase> CreateDeserializer()
    {
      EXml<RuleBase> ret = new();

      ObjectElementDeserializer oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(RuleBase))
        .WithCustomPropertyDeserialization(
          nameof(RuleBase.AffinityRules),
          EXmlHelper.List.CreateForNested(
            "affinity", new EXmlHelper.List.DT("rule", typeof(AffinityRule)), () => new List<AffinityRule>()))
      .WithCustomPropertyDeserialization(
        nameof(RuleBase.PriorityRules),
        EXmlHelper.List.CreateForNested(
          "priority", new EXmlHelper.List.DT("rule", typeof(PriorityRule)), () => new List<PriorityRule>()));
      ret.Context.ElementDeserializers.Insert(0, oed);

      oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(AffinityRule))
        .WithIgnoredProperty(nameof(AffinityRule.CoreFlags));
      ret.Context.ElementDeserializers.Insert(0, oed);

      return ret;
    }
  }
}
