using Eng.EFsExtensions.EFsExtensionsModuleBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESystem;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking.VariableModel;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.VMs;
using System.Windows.Forms.VisualStyles;
using System.Runtime.CompilerServices;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.VMs
{
  public class VariableVMS : GenericVMS<VariableVM, Variable>
  {
    private VariableVMS(IEnumerable<VariableVM> items) 
      : base(items, q => q.Variable.Name, q=>q.Variable) { }

    public static VariableVMS Create(List<Variable> variables)
    {
      VariableVMS ret = new(variables.Select(q => VariableVM.Create(q)));
      return ret;
    }

    public bool IsValid() => this.None(q => double.IsNaN(q.Value));
  }
}
