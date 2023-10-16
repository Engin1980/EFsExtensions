using EXmlLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types
{
  public abstract class FailItem
  {
    public double Weight { get; set; } = 1;
  }

  public class FailGroup : FailItem
  {
    public enum ESelection
    {
      None, One, All
    }

    public ESelection Selection { get; set; } = ESelection.One;
    public List<FailItem> Items { get; set; } = new List<FailItem>();
  }

  public class Failure : FailItem
  {
    [EXmlNonemptyString]
    public string Id { get; set; } = "";
  }
}
