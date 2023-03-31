using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types
{
  public class FailGroup
  {
    public enum EAggregation
    {
    }

    public List<FailGroup> Groups { get; set; }
    public string Title { get; set; }
    public double Probability { get; set; }
    public List<Failure> Failures { get; set; }
    public EAggregation Aggregation { get; set; }
  }
}
