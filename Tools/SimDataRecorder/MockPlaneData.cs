using ESimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SimDataRecorder
{
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
  public struct MockPlaneData
  {
    #region Fields
    [DataDefinition(SimVars.Aircraft.Miscelaneous.ACCELERATION_BODY_Z, SimUnits.Acceleration.FEET_PER_SECOND_SQUARED)]
    public double accelerationBodyZ;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_ALTITUDE, SimUnits.Length.FOOT)]
    public int altitude;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_BANK_DEGREES, SimUnits.Angle.DEGREE)]
    public double bankAngle;

    [DataDefinition(SimVars.Aircraft.RadioAndNavigation.ATC_ID)]
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string callsign;

    [DataDefinition(SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "4")]
    public int engineFourCombustion;

    [DataDefinition(SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "1")]
    public int engineOneCombustion;
    [DataDefinition(SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "3")]
    public int engineThreeCombustion;

    [DataDefinition(SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "2")]
    public int engineTwoCombustion;
    [DataDefinition(SimVars.Aircraft.Miscelaneous.GROUND_VELOCITY, SimUnits.Speed.KNOT)]
    public int groundSpeed;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_ALT_ABOVE_GROUND, SimUnits.Length.FOOT)]
    public int height;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.AIRSPEED_INDICATED, SimUnits.Speed.KNOT)]
    public int indicatedSpeed;

    [DataDefinition(SimVars.Aircraft.BrakesAndLandingGear.BRAKE_PARKING_POSITION)]
    public int parkingBrake;
    [DataDefinition(SimVars.Services.PUSHBACK_ATTACHED)]
    public int pushbackTugConnected;
    [DataDefinition(SimVars.Aircraft.Miscelaneous.VERTICAL_SPEED, SimUnits.Length.FOOT)]
    public int verticalSpeed;
    #endregion Fields
  }
}
