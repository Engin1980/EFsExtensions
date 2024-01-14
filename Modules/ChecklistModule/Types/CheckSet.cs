using Eng.Chlaot.ChlaotModuleBase.ModuleUtils;
using EXmlLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.ChecklistModule.Types
{
  public class CheckSet
  {
    public List<CheckList> Checklists { get; set; } = null!;
  }
}
