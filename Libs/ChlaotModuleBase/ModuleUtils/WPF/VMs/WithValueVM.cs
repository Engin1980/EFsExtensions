using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.VMs
{
  public abstract class WithValueVM : NotifyPropertyChanged
  {
    public double Value
    {
      get => base.GetProperty<double>(nameof(Value))!;
      set
      {
        base.UpdateProperty(nameof(Value), value);
        this.IsValid = !double.IsNaN(value);
      }
    }

    public bool IsValid
    {
      get => base.GetProperty<bool>(nameof(IsValid))!;
      private set => base.UpdateProperty(nameof(IsValid), value);
    }
  }
}
