using ChlaotModuleBase;
using ChlaotModuleBase.ModuleUtils.Storable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.AffinityModule
{
  public class Settings : StorableObject
  {
    protected override string FileName => "affinityRules.affru.xml";

    public BindingList<Rule> Rules
    {
      get => base.GetProperty<BindingList<Rule>>(nameof(Rules))!;
      set => base.UpdateProperty(nameof(Rules), value);
    }

    public Settings()
    {
      this.Rules = new();
    }

    public static Settings CreateDefault()
    {
      Settings ret = new();
      ret.Rules.Add(new Rule()
      {
        Regex = "Flight Simulator",
        CoresPattern = "1-6"
      });
      ret.Rules.Add(new Rule()
      {
        Regex = ".+",
        CoresPattern = "7-64"
      });
      return ret;
    }
  }
}
