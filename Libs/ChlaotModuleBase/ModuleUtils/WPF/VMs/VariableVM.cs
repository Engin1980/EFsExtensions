using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.VMs
{
  public class VariableVM : NotifyPropertyChangedBase
  {

    public Variable Variable
    {
      get => base.GetProperty<Variable>(nameof(Variable))!;
      set => base.UpdateProperty(nameof(Variable), value);
    }

    public bool IsReadOnly
    {
      get => base.GetProperty<bool>(nameof(IsReadOnly))!;
      set => base.UpdateProperty(nameof(IsReadOnly), value);
    }

    public double Value
    {
      get => base.GetProperty<double>(nameof(Value))!;
      set => base.UpdateProperty(nameof(Value), value);
    }

    public VariableVM(UserVariable variable)
    {
      this.Variable = variable;
      this.IsReadOnly = false;
      this.Value = variable.DefaultValue ?? double.NaN;
    }

    private static readonly Random rnd = new();
    public VariableVM(RandomVariable variable)
    {
      this.Variable = variable;
      this.IsReadOnly = true;

      var tmp = variable.Minimum + rnd.NextDouble() * (variable.Maximum - variable.Minimum);
      if (variable.IsInteger)
        tmp = Math.Round(tmp);
      this.Value = tmp;
    }
  }
}
