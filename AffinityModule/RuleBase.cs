using ChlaotModuleBase.ModuleUtils;
using ChlaotModuleBase.ModuleUtils.Storable;
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
    public MetaInfo MetaInfo { get; set; } = new();
    public List<Rule> Rules { get; set; } = new List<Rule>();
  }
}
