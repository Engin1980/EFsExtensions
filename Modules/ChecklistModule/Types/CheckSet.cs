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
#pragma warning disable CS8618
    public MetaInfo Meta { get; set; }
    public List<CheckList> Checklists { get; set; }
#pragma warning restore CS8618
  }
}
