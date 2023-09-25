using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types
{
  public abstract class FailItem
  {
    public double? Weight { get; set; }
  }

  public class FailGroup : FailItem
  {
    public enum ESelection
    {
      None, One, All
    }

    public ESelection Selection { get; set; }
    public List<FailItem> Items { get; set; } = new List<FailItem>();
  }

  public class Failure : FailItem
  {
    public string Id { get; set; }
  }
}
