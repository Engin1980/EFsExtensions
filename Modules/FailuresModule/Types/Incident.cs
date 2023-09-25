using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types
{
  public abstract class Incident
  {
    public string Title { get; set; }
    public override string ToString()
    {
      return $"{Title} ({this.GetType().Name})";
    }
  }
}
