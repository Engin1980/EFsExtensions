using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Model.Sim
{
  public class FailureDefinitionGroup : FailureDefinitionBase
  {
    public string Name { get; set; }
    public List<FailureDefinitionBase> Items { get; } = new List<FailureDefinitionBase>();

    public FailureDefinitionGroup(string name)
    {
      Name = name;
    }
  }
}
