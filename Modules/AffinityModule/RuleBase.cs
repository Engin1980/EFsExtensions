using AffinityModule;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.Storable;
using Eng.EFsExtensions.Modules.AffinityModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.AffinityModule
{
  public class RuleBase : StorableObject
  {
    public int ResetIntervalInS { get; set; } = 0;
    public List<AffinityRule> AffinityRules { get; set; } = new();
    public List<PriorityRule> PriorityRules { get; set; } = new();
  }
}
