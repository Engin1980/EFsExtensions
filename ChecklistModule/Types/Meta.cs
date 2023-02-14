using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ChecklistModule.Types
{
  public class MetaInfo
  {
    public string Label { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public string Web { get; set; }
    public string Email { get; set; }
    public string License { get; set; }
  }
}
