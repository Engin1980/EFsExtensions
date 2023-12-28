using Eng.Chlaot.ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.SimVarTestModule
{
  public class SimVarCase : NotifyPropertyChangedBase
  {
    public string SimVar
    {
      get => base.GetProperty<string>(nameof(SimVar))!;
      set => base.UpdateProperty(nameof(SimVar), value);
    }

    public double Value
    {
      get => base.GetProperty<double>(nameof(Value))!;
      set => base.UpdateProperty(nameof(Value), value);
    }

  }
}
