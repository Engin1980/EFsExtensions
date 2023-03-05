using ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.AffinityModule
{
  public class Rule : NotifyPropertyChangedBase
  {
    private static int NumberOfCores = Environment.ProcessorCount;

    public string Regex
    {
      get => base.GetProperty<string>(nameof(Regex))!;
      set => base.UpdateProperty(nameof(Regex), value);
    }

    private string _CoresPattern;
    public string CoresPattern
    {
      get => base.GetProperty<string>(nameof(CoresPattern))!;
      set
      {
        base.UpdateProperty(nameof(CoresPattern), value);
        Cores = ExpandToCores();
      }
    }

    private BindingList<bool> ExpandToCores()
    {

      string [] pts = this.CoresPattern.Split(';');
      foreach(string pt in pts)
      {
        if (pt.Contains('-'))
        {

        }
        else
        {
          int index = int.Parse(pt);
          SetCore(index, true);
        }
      }
    }

    public BindingList<bool> Cores
    {
      get => base.GetProperty<BindingList<bool>>(nameof(Cores))!;
      set => base.UpdateProperty(nameof(Cores), value);
    }

    public Rule()
    {

    }
  }
}
