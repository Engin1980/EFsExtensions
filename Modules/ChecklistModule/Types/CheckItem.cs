using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.ChecklistModule.Types
{
  public class CheckItem
  {
#pragma warning disable CS8618
    public CheckDefinition Call { get; set; }
    public CheckDefinition Confirmation { get; set; }
#pragma warning restore CS8618
  }
}
