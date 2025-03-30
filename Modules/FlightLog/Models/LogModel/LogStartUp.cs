using Eng.EFsExtensions.Libs.AirportsLib;

namespace Eng.EFsExtensions.Modules.FlightLogModule.LogModel
{
  public class LogStartUp
  {
    public DateTime? ScheduledTime { get; set; }
    public DateTime Time { get; set; }
    public int FuelAmountKg { get; set; }
    public GPS Location { get; set; }

    public LogStartUp(DateTime? scheduledTime, DateTime realTime, int realFuelAmountKg, GPS location)
    {
      ScheduledTime = scheduledTime;
      Time = realTime;
      FuelAmountKg = realFuelAmountKg;
      Location = location;
    }

    public LogStartUp()
    {
    }
  }

}
