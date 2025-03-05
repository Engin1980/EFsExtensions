using ESystem.Asserting;
using EXmlLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking.VariableModel
{
  public class UserVariable : Variable
  {
    public double? DefaultValue { get; set; } = double.NaN;

  }
}
