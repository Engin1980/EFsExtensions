
namespace Eng.EFsExtensions.Modules.RaaSModule.Model
{
  public readonly struct RaasDistance
  {
    public enum RaasDistanceUnit
    {
      m,
      km,
      ft,
      nm
    }

    private readonly double value;
    public readonly double Value => value;
    private readonly RaasDistanceUnit unit;
    public readonly RaasDistanceUnit Unit => unit;

    public override string ToString() => $"{value:0} {unit}";

    public readonly double GetInMeters()
    {
      if (unit == RaasDistanceUnit.m) return value;
      else
      {
        double valueInMeters = unit switch
        {
          RaasDistanceUnit.km => value * 1000,
          RaasDistanceUnit.ft => value / 3.28084,
          RaasDistanceUnit.nm => value * 1852,
          _ => throw new ApplicationException("Unknown RaasDistanceUnit")
        };
        return valueInMeters;
      };
    }

    public readonly double GetInKilometers()
    {
      if (unit == RaasDistanceUnit.km) return value;
      else return GetInMeters() / 1000;
    }

    public readonly double GetInFeet()
    {
      if (unit == RaasDistanceUnit.ft) return value;
      else return GetInMeters() * 3.28084;
    }
    public readonly double GetInNauticalMiles()
    {
      if (unit == RaasDistanceUnit.nm) return value;
      else return GetInMeters() / 1852;
    }

    public RaasDistance() : this(0, RaasDistanceUnit.m) { }
    public RaasDistance(double value, RaasDistanceUnit unit)
    {
      this.value = value;
      this.unit = unit;
    }

    internal readonly void CheckSanity()
    {
      if (value < 0) throw new ApplicationException("RaasDistance is negative.");
    }

    public static RaasDistance Parse(string value)
    {
      const string PATTERN = @"^(\d+) ?(m|ft|km|nm)$";
      System.Text.RegularExpressions.Regex regex = new(PATTERN);
      System.Text.RegularExpressions.Match match = regex.Match(value);
      if (!match.Success) throw new ApplicationException("Unable to decode RaasDistance from string: " + value);

      double dist = double.Parse(match.Groups[1].Value);
      string unit = match.Groups[2].Value;
      RaasDistance ret = unit switch
      {
        "km" => new RaasDistance(dist, RaasDistance.RaasDistanceUnit.km),
        "m" => new RaasDistance(dist, RaasDistance.RaasDistanceUnit.m),
        "ft" => new RaasDistance(dist, RaasDistance.RaasDistanceUnit.ft),
        "nm" => new RaasDistance(dist, RaasDistance.RaasDistanceUnit.nm),
        _ => throw new ApplicationException("Unknown unit: " + unit)
      };
      return ret;
    }
  }
}
