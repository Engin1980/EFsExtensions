using Eng.EFsExtensions.Libs.AirportsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel
{
  public class LogTakeOff
  {
    public GPS RunStartLocation { get; set; }
    public GPS AirborneLocation { get; set; }
    public double MaxVS { get; set; }
    public int IAS { get; set; }
    public double MaxBank { get; set; }
    public double MaxPitch { get; set; }
    public double MaxAccY { get; set; }
    public TimeSpan MainGearTime { get; set; }
    public TimeSpan AllGearTime { get; set; }
  }
}
