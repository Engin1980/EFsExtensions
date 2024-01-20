using ESystem.Asserting;
using EXmlLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel
{
  public class UserVariable : Variable //probably no longer needed to be notifi-based and uservalue can be omitted too
  {
    public double? DefaultValue
    {
      get => base.GetProperty<double?>(nameof(DefaultValue))!;
      set => base.UpdateProperty(nameof(DefaultValue), value);
    }

    public double? UserValue
    {
      get => base.GetProperty<double?>(nameof(UserValue))!;
      set => base.UpdateProperty(nameof(UserValue), value);
    }

    public double? UserOrDefaultValue
    {
      get => UserValue ?? DefaultValue ?? double.NaN;
      set => UserValue = value;
    }

    public UserVariable()
    {
      DefaultValue = null;
      UserValue = null;
    }

    public override double Value { get => UserValue ?? DefaultValue ?? double.NaN; }

  }
}
