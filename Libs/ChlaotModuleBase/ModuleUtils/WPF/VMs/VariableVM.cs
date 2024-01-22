using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.VMs
{
  public class VariableVM : WithValueVM
  {

    #region Private Fields

    private static Random rnd = new();

    #endregion Private Fields

    #region Public Properties

    public string AdditionalInfo
    {
      get
      {
        string ret = "";
        if (Variable is UserVariable uv && uv.DefaultValue != null && !double.IsNaN(uv.DefaultValue.Value))
          ret = $"(default = {uv.DefaultValue.Value})";
        else if (Variable is RandomVariable rv)
          ret = $"(random {rv.Minimum} .. {rv.Maximum})";
        return ret;
      }
    }

    public bool IsReadOnly
    {
      get => base.GetProperty<bool>(nameof(IsReadOnly))!;
      private set => base.UpdateProperty(nameof(IsReadOnly), value);
    }

    public Variable Variable
    {
      get => base.GetProperty<Variable>(nameof(Variable))!;
      private set => base.UpdateProperty(nameof(Variable), value);
    }

    #endregion Public Properties

    #region Public Constructors

    public VariableVM(UserVariable variable)
    {
      this.Variable = variable;
      this.IsReadOnly = false;
      this.Value = variable.DefaultValue ?? double.NaN;
    }

    public VariableVM(RandomVariable variable)
    {
      this.Variable = variable;
      this.IsReadOnly = true;
      var tmp = variable.Minimum + rnd.NextDouble() * (variable.Maximum - variable.Minimum);
      if (variable.IsInteger)
        tmp = Math.Round(tmp);
      this.Value = tmp;
    }

    #endregion Public Constructors

    #region Public Methods

    public static VariableVM Create(Variable variable)
    {
      VariableVM ret;
      if (variable is UserVariable uv)
        ret = new VariableVM(uv);
      else if (variable is RandomVariable rv)
        ret = new VariableVM(rv);
      else
        throw new NotImplementedException();
      return ret;
    }

    public override string ToString() => $"{this.Variable.Name}={this.Value} {{VariableVM}}";

    #endregion Public Methods

  }
}
