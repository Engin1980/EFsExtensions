using ESystem.Asserting;
using EXmlLib;
using EXmlLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimObjects
{
  public class SimProperty : SimPropertyBase, IXmlObjectPostDeserialize
  {
    public string Name { get; set; } = null!;
    public string SimVar { get; set; } = null!;
    public string? Unit { get; set; } = null;

    public void PostDeserialize()
    {
      EAssert.IsNonEmptyString(Name, $"{nameof(Name)} cannot be empty or null.");
      EAssert.IsNonEmptyString(SimVar, $"{nameof(SimVar)} cannot be empty or null.");
    }

    public override string ToString() => $"{Name}::{SimVar} {{SimProperty}}";
  }
}
