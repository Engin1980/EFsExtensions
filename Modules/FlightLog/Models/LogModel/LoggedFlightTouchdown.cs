using Eng.EFsExtensions.Libs.AirportsLib;
using ESystem.Structs;

namespace Eng.EFsExtensions.Modules.FlightLogModule.LogModel
{
  public class LoggedFlightTouchdown
  {
    public DateTime TouchDownDateTime { get; set; }
    public GPS TouchDownLocation { get; set; }
    public DateTime? RollOutEndDateTime { get; set; }
    public GPS? RollOutEndLocation { get; set; }
    public double VS { get; set; }
    public double SmartVS { get; set; } = double.NaN;
    public Speed IAS { get; set; }
    public Speed GS { get; set; }
    public double Bank { get; set; }
    public double Pitch { get; set; }
    public double MaxAccY { get; set; }
    public TimeSpan MainGearTime { get; set; }
    public TimeSpan? AllGearTime { get; set; }
    public Distance? RollOutLength => this.RollOutEndLocation != null
      ? Distance.Of(GpsCalculator.GetDistance(this.TouchDownLocation, this.RollOutEndLocation.Value), DistanceUnit.Meters)
      : null;
    public TimeSpan? RollOutDuration => RollOutEndDateTime == null ? null : RollOutEndDateTime.Value - TouchDownDateTime;
  }

}
