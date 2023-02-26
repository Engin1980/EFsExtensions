using ESimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ESimConnectWpfTest
{
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
  public struct DataStruct
  {
    [DataDefinition("CAMERA STATE")]
    public int cameraState;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.TITLE)]
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string title;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.AIRSPEED_INDICATED, SimUnits.Speed.KNOT)]
    public int speed;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_ALTITUDE, SimUnits.Length.FOOT)]
    public int altitude;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_BANK_DEGREES, SimUnits.Angle.DEGREE, SimType.INT32)]
    public int bank;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.VELOCITY_BODY_Z, SimUnits.Acceleration.FEET_PER_SECOND_SQUARED)]
    public double velocityBodyZ;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.VELOCITY_BODY_X, SimUnits.Acceleration.FEET_PER_SECOND_SQUARED)]
    public double velocityBodyX;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.ACCELERATION_BODY_X, SimUnits.Acceleration.FEET_PER_SECOND_SQUARED)]
    public double accelerationBodyX;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.ACCELERATION_BODY_Z, SimUnits.Acceleration.FEET_PER_SECOND_SQUARED)]
    public double accelerationBodyZ;
  };

}
