using Eng.EFsExtensions.Libs.AirportsLib;
using ESystem.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel
{
  public class LoggedFlightTakeOff
  {
    public DateTime RunStartDateTime { get; set; }
    public GPS RunStartLocation { get; set; }
    public DateTime AirborneDateTime { get; set; }
    public GPS AirborneLocation { get; set; }
    public double MaxVS { get; set; }
    public Speed IAS { get; set; }
    public Speed GS { get; set; }
    public double MaxBank { get; set; }
    public double MaxPitch { get; set; }
    public double MaxAccY { get; set; }
    public TimeSpan FrontGearTime { get; set; }
    public TimeSpan AllGearTime { get; set; }
    public Distance Length => Distance.Of(GpsCalculator.GetDistance(RunStartLocation, AirborneLocation), DistanceUnit.Meters);
  }
}
