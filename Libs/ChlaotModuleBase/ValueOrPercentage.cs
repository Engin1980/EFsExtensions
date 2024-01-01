using ESystem.Asserting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChlaotModuleBase
{
  public struct DoubleValueOrPercentage
  {
    public double Value { get; set; }
    public bool IsPercentage { get; set; }

    public bool IsEmpty => double.IsNaN(Value);

    public static DoubleValueOrPercentage Empty { get => new(double.NaN, false); }

    public DoubleValueOrPercentage WithValue(double newValue) => new DoubleValueOrPercentage(newValue, this.IsPercentage);

    public DoubleValueOrPercentage(double value, bool isPercentage)
    {
      if (isPercentage)
        EAssert.Argument.IsTrue(value >= 0, nameof(value));
      this.Value = value;
      this.IsPercentage = isPercentage;
    }
  }
}
