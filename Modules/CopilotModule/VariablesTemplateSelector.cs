using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel;
using ESystem.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.VMs;

namespace Eng.Chlaot.Modules.CopilotModule
{
  internal class VariablesTemplateSelector : DataTemplateSelector
  {
    public DataTemplate UserVariableTemplate { get; set; }
    public DataTemplate RandomVariableTemplate { get; set; }
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      BindingKeyValue<VariableVM, double> kv = (BindingKeyValue<VariableVM, double>)item;
      Variable v = kv.Key.Variable;
      if (v is UserVariable)
        return UserVariableTemplate;
      else if (v is RandomVariable)
        return RandomVariableTemplate;
      else
        throw new NotImplementedException();
    }
  }
}
