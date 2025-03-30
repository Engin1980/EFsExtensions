using Eng.EFsExtensions.Libs.AirportsLib;
using ESystem.Asserting;

namespace Eng.EFsExtensions.Modules.FlightLogModule.LogModel
{
  public class LogLanding
  {
    public DateTime? ScheduledTime { get; set; }
    public List<LogTouchdown> Touchdowns { get; set; } = null!;
    public DateTime Time => Touchdowns.Last().DateTime;
    public int? ScheduledFuelAmountKg { get; set; }
    public int FuelAmountKg { get; set; }
    public GPS Location => Touchdowns.Last().Location;
    public int IAS => Touchdowns.Last().IAS;
    public double Bank => Touchdowns.Last().Bank;
    public double Pitch => Touchdowns.Last().Pitch;

    public LogLanding(DateTime? scheduledTime, int? scheduledFuelAmountKg, int fuelAmountKg, List<LogTouchdown> touchdowns)
    {
      EAssert.Argument.IsTrue(touchdowns.Count > 0, nameof(touchdowns), "Touchdowns must have at least one entry");

      ScheduledTime = scheduledTime;
      this.Touchdowns = touchdowns;
      ScheduledFuelAmountKg = scheduledFuelAmountKg;
      FuelAmountKg = fuelAmountKg;
    }

    public LogLanding()
    {
    }
  }

}
