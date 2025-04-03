using Eng.EFsExtensions.Libs.AirportsLib;

namespace Eng.EFsExtensions.Modules.FlightLogModule.LogModel
{
  public class LogTouchdown
  {
    public DateTime TouchDownDateTime { get; set; }
    public GPS TouchDownLocation { get; set; }
    public DateTime? RollOutEndDateTime { get; set; }
    public GPS? RollOutEndLocation { get; set; }
    public double VS { get; set; }
    public int IAS { get; set; }
    public int GS { get; set; }
    public double Bank { get; set; }
    public double Pitch { get; set; }
    public double MaxAccY { get; set; }
    public TimeSpan MainGearTime { get; set; }
    public TimeSpan? AllGearTime { get; set; }
    public double? RollOutLength => this.RollOutEndLocation != null
      ? GpsCalculator.GetDistance(this.TouchDownLocation, this.RollOutEndLocation.Value)
      : null;
    public TimeSpan? RollOutDuration => RollOutEndDateTime == null ? null : RollOutEndDateTime.Value - TouchDownDateTime;
  }

}
