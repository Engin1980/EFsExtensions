using Eng.EFsExtensions.Libs.AirportsLib;

namespace Eng.EFsExtensions.Modules.FlightLogModule.LogModel
{
  public class LogTouchdown
  {
    public DateTime DateTime { get; set; }
    public GPS Location { get; set; }
    public double VS { get; set; }
    public int IAS { get; set; }
    public double Bank { get; set; }
    public double Pitch { get; set; }
    public double MaxAccY { get; set; }
    public double MainGearTime { get; set; }
    public double AllGearTime { get; set; }
  }

}
