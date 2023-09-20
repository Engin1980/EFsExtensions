using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types
{
  public abstract class Variable
  {
    public string Name { get; set; }
    public string Description { get; set; }

    public abstract double GetValue();
  }
}
