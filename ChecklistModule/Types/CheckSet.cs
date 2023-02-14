using EXmlLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChecklistModule.Types
{
  public class CheckSet
  {
    public MetaInfo Meta { get; set; }
    [EXmlFlat("checklist")]
    public List<CheckList> Checklists { get; set; }
  }
}
