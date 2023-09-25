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
        nameof(RuleBase.Rules),
        EXmlHelper.List.CreateForFlat<Rule>("rule"));
      ret.Context.ElementDeserializers.Insert(0, oed);

      oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(Rule))
        .WithIgnoredProperty(nameof(Rule.CoreFlags));
      ret.Context.ElementDeserializers.Insert(0, oed);

      return ret;
    }
  }
}
