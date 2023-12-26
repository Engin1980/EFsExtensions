using EXmlLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking
{
  public abstract class Variable : NotifyPropertyChangedBase
  {
    [EXmlNonemptyString]
    public string Name
    {
      get => base.GetProperty<string>(nameof(Name))!;
      set => base.UpdateProperty(nameof(Name), value);
    }


    public string? Description
    {
      get => base.GetProperty<string?>(nameof(Description))!;
      set => base.UpdateProperty(nameof(Description), value);
    }

    public abstract double Value { get; }
  }
}
