using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Model.Incidents
{
  public abstract class Trigger
  {
    public Percentage Probability { get; set; }
    public bool Repetitive { get; set; }
  }
}
