using ESystem.Asserting;
using EXmlLib;
using EXmlLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects
{
  public class SimProperty : SimPropertyBase, IXmlObjectPostDeserialize
  {
    public string Name { get; set; } = null!;
    public string SimVar { get; set; } = null!;
    public string? Unit { get; set; } = null;

    public override int GetHashCode() => string.GetHashCode(this.Name) + string.GetHashCode(SimVar);

    public override bool Equals(object? obj)
    {
      bool ret;
      if (obj is SimProperty sp)
        ret = string.Equals(this.Name, sp.Name)
          && string.Equals(this.SimVar, sp.SimVar)
          && string.Equals(this.Unit, sp.Unit);
      else
        ret = false;
      return ret;
    }

    public void PostDeserialize()
    {
      EAssert.IsNonEmptyString(Name, $"{nameof(Name)} cannot be empty or null.");
      EAssert.IsNonEmptyString(SimVar, $"{nameof(SimVar)} cannot be empty or null.");
    }

    public override string ToString() => $"{Name}::{SimVar} {{SimProperty}}";
  }
}
