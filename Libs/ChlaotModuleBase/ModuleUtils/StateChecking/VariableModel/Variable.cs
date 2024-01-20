using ESystem.Asserting;
using EXmlLib.Attributes;
using EXmlLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel
{
  public abstract class Variable : NotifyPropertyChangedBase, IXmlObjectPostDeserialize
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

    public virtual void PostDeserialize()
    {
      EAssert.IsNonEmptyString(Name, nameof(Name));
      EAssert.IsTrue(Regex.IsMatch(this.Name, @"^[^\{]\S+[^\}]$"), $"Invalid variable name '{this.Name}'");
    }
  }
}
