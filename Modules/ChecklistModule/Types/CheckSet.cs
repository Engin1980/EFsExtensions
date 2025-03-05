using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils;
using EXmlLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.ChecklistModule.Types
{
  public class CheckSet
  {
    public List<CheckList> Checklists { get; set; } = null!;
  }
}
