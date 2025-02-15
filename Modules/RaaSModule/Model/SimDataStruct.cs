using ESimConnect.Definitions;
using ESimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.RaaSModule.Model
{
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
  public struct SimDataStruct
  {
    [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_ALT_ABOVE_GROUND, SimUnits.Length.FOOT)]
    public int height;
    public readonly int Height => height;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.AIRSPEED_INDICATED, SimUnits.Speed.KNOT)]
    public int indicatedSpeed;
    public readonly int IndicatedSpeed => indicatedSpeed;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_HEADING_DEGREES_MAGNETIC, SimUnits.Angle.DEGREE)]
    public int heading;
    public readonly int Heading => heading;


    [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_LATITUDE, SimUnits.Angle.DEGREE)]
    public double latitude;
    public readonly double Latitude => latitude;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_LONGITUDE, SimUnits.Angle.DEGREE)]
    public double longitude;
    public readonly double Longitude => longitude;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.VERTICAL_SPEED, SimUnits.Length.FOOT)]
    public int verticalSpeed;
    public readonly int VerticalSpeed => verticalSpeed;
  }
}
