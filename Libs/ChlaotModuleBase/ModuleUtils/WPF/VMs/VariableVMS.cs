using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.Interfaces;
using ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESystem;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.VMs
{
  public class VariableVMS : BindingList<VariableVM>, IVariableValuesProvider
  {
    public bool IsValid() => this.None(q => double.IsNaN(q.Value));
    public Dictionary<string, double> GetVariableValues()
    {
      return this.ToDictionary(q => q.Variable.Name, q => q.Value);
    }
  }
}
