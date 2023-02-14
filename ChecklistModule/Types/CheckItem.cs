using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChecklistModule.Types
{
  public class CheckItem
  {
    public CheckDefinition Call { get; set; }
    public CheckDefinition Confirmation { get; set; }
  }
}
