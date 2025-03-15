using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.RaaSModule.Model
{
  public struct Heading
  {
    private double value;

    public double Value => value;

    public Heading(double degrees)
    {
      value = Normalize(degrees);
    }

    private static double Normalize(double degrees)
    {
      degrees %= 360;
      return degrees < 0 ? degrees + 360 : degrees;
    }

    public static Heading operator +(Heading h1, double degrees) =>
      new(h1.value + degrees);

    public static Heading operator +(Heading h1, Heading h2) => new(h1.value + h2.value);

    public static Heading operator -(Heading h1, double degrees) => new(h1.value - degrees);
    public static Heading operator -(Heading h1, Heading h2) => new(h1.value - h2.value);

    public static double Difference(Heading h1, Heading h2)
    {
      double diff = Math.Abs(h1.value - h2.value);
      return diff > 180 ? 360 - diff : diff;
    }

    public readonly override string ToString()
    {
      return $"{value:F1}°";
    }

    public static explicit operator double(Heading h)
    {
      return h.value;
    }

    public static explicit operator Heading(double degrees)
    {
      return new Heading(degrees);
    }
  }
}
