using ESimConnect.Definitions;
using ESimConnect.Extenders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.RaaSModule.Model
{
  class SimDataSnapshot
  {
    [SimProperty(SimVars.Aircraft.Miscelaneous.PLANE_ALT_ABOVE_GROUND, SimUnits.Length.FOOT)]
    public int Height { get; set; }

    [SimProperty(SimVars.Aircraft.Miscelaneous.AIRSPEED_INDICATED, SimUnits.Speed.KNOT)]
    public int IndicatedSpeed { get; set; }

    [SimProperty(SimVars.Aircraft.Miscelaneous.PLANE_HEADING_DEGREES_MAGNETIC, SimUnits.Angle.DEGREE)]
    public double Heading { get; set; }

    [SimProperty(SimVars.Aircraft.Miscelaneous.PLANE_LATITUDE, SimUnits.Angle.DEGREE)]
    public double Latitude { get; set; }

    [SimProperty(SimVars.Aircraft.Miscelaneous.PLANE_LONGITUDE, SimUnits.Angle.DEGREE)]
    public double Longitude { get; set; }

    [SimProperty(SimVars.Aircraft.Miscelaneous.VERTICAL_SPEED, SimUnits.Length.FOOT)]
    public double VerticalSpeed { get; set; }
  }
}
