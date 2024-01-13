using AffinityModule;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Storable;
using Eng.Chlaot.Modules.AffinityModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.AffinityModule
{
  public class RuleBase : StorableObject
  {
    public int ResetIntervalInS { get; set; } = 0;
    public List<AffinityRule> AffinityRules { get; set; } = new();
    public List<PriorityRule> PriorityRules { get; set; } = new();
  }
}
