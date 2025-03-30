using Eng.EFsExtensions.Libs.AirportsLib;

namespace Eng.EFsExtensions.Modules.FlightLogModule.LogModel
{
  public class LogTakeOff
  {
    public DateTime? ScheduledTime { get; set; }
    public DateTime Time { get; set; }
    public int? ScheduledFuelAmountKg { get; set; }
    public int FuelAmountKg { get; set; }
    public GPS Location { get; set; }
    public int IAS { get; set; }

    public LogTakeOff(DateTime? scheduledTime, DateTime realTime, int? scheduledFuelAmountKg, int fuelAmountKg, GPS location, int iAS)
    {
      ScheduledTime = scheduledTime;
      Time = realTime;
      ScheduledFuelAmountKg = scheduledFuelAmountKg;
      FuelAmountKg = fuelAmountKg;
      Location = location;
      IAS = iAS;
    }

    public LogTakeOff()
    {
    }
  }

}
