using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types
{
  public class RandomVariable : Variable
  {
    private static readonly Random rnd = new();
    public double Minimum { get; set; }
    public double Maximum { get; set; }

    private double? _Value = null;
    public override double Value
    {
      get
      {
        if (_Value == null)
          this._Value = this.Minimum + RandomVariable.rnd.NextDouble() * (this.Maximum - this.Minimum);
        return this._Value.Value;
      }
    }
  }
}
