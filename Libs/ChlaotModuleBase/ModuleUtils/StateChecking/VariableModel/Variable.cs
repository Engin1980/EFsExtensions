using ESystem.Asserting;
using EXmlLib.Attributes;
using EXmlLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking.VariableModel
{
  public abstract class Variable : IXmlObjectPostDeserialize
  {
    [EXmlNonemptyString]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual void PostDeserialize()
    {
      EAssert.IsNonEmptyString(Name, nameof(Name));
      EAssert.IsTrue(Regex.IsMatch(this.Name, @"^[a-zA-Z][a-zA-Z0-9-_~^]*$"), $"Invalid variable name '{this.Name}'");
    }
  }
}
