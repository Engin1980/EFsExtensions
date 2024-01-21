using ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESystem;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel;
using ChlaotModuleBase.ModuleUtils.WPF.VMs;
using System.Windows.Forms.VisualStyles;
using System.Runtime.CompilerServices;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.VMs
{
  public class VariableVMS : GenericVMS<VariableVM>
  {
    private static readonly Random rnd = new();

    private VariableVMS(IEnumerable<VariableVM> items) : base(items, q => q.Variable.Name) { }

    public static VariableVMS Create(List<Variable> variables)
    {
      VariableVMS ret = new(variables.Select(q => VariableVM.Create(q)));
      foreach (var item in ret)
      {
        if (item.Key.Variable is RandomVariable rv)
        {
          var tmp = rv.Minimum + rnd.NextDouble() * (rv.Maximum - rv.Minimum);
          if (rv.IsInteger)
            tmp = Math.Round(tmp);
          item.Value = tmp;
        }
        else if (item.Key.Variable is UserVariable uv)
        {
          item.Value = uv.DefaultValue ?? double.NaN;
        }
      }
      return ret;
    }

    public bool IsValid() => this.None(q => double.IsNaN(q.Value));
  }
}
