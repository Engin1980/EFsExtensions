using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types
{
  public class Incident
  {
    public List<Variable> Variables { get; set; }
    public List<Trigger> Triggers { get; set; }
    public FailGroup FailGroup { get; set; }
  }
}
