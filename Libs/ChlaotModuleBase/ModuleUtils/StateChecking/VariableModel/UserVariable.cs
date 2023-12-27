using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel
{
  public class UserVariable : Variable
  {

    public double? DefaultValue
    {
      get => base.GetProperty<double>(nameof(DefaultValue))!;
      set => base.UpdateProperty(nameof(DefaultValue), value);
    }

    public double? UserValue
    {
      get => base.GetProperty<double>(nameof(UserValue))!;
      set => base.UpdateProperty(nameof(UserValue), value);
    }

    public override double Value { get => UserValue ?? DefaultValue ?? double.NaN; }
  }
}
