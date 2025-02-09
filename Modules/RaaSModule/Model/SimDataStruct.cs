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

    [DataDefinition(SimVars.Aircraft.Miscelaneous.AIRSPEED_INDICATED, SimUnits.Speed.KNOT)]
    public int indicatedSpeed;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_LATITUDE)]
    public double latitude;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_LONGITUDE)]
    public double longitude;

    //[DataDefinition(SimVars.Aircraft.Miscelaneous.VERTICAL_SPEED, SimUnits.Length.FOOT)]
    //public int verticalSpeed;
  }
}
