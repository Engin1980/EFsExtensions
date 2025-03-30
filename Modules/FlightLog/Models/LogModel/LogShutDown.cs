using Eng.EFsExtensions.Libs.AirportsLib;

namespace Eng.EFsExtensions.Modules.FlightLogModule.LogModel
{
  public class LogShutDown
  {
    public DateTime? ScheduledTime { get; set; }
    public DateTime Time { get; set; }
    public int FuelAmountKg { get; set; }
    public GPS Location { get; set; }

    public LogShutDown(DateTime? scheduledTime, DateTime realTime, int fuelAmountKg, GPS location)
    {
      ScheduledTime = scheduledTime;
      Time = realTime;
      FuelAmountKg = fuelAmountKg;
      Location = location;
    }

    public LogShutDown()
    {
    }
  }

}
