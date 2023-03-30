using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.Http.Headers;
using System.Printing;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types
{
  public class FailureFactory
  {
    private static int ENGINES_COUNT = 4;

    public static List<Failure> BuildFailures()
    {
      List<Failure> ret = new();

      var funs = typeof(FailureFactory)
        .GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
        .Where(q => q.Name.StartsWith("Build") && q.Name != "BuildFailures")
        .ToList();

      foreach (var fun in funs)
      {
        try
        {
          object tmp = (fun.Invoke(null, null) ?? throw new NullReferenceException());
          if (tmp is Failure f)
            ret.Add(f);
          else if (tmp is List<Failure> l)
            ret.AddRange(l);
        }
        catch (Exception ex)
        {
          throw new ApplicationException($"Failed to invoke building method {fun.Name}.", ex);
        }
      }
      return ret;
    }

    private static MultiFailure BuildEngineFire()
    {
      MultiFailure ret = new("Engine Fire");

      for (int i = 0; i < ENGINES_COUNT; i++)
      {
        InstantFailure instantFailure = new($"Engine {i} fire", new VarSimConPoint($"ENGINE ON FIRE:{i}"));

        ret.Failures.Add(instantFailure);
      }

      return ret;
    }

    private static MultiFailure BuildEngineFailure()
    {
      MultiFailure ret = new("Engine Failure");

      for (int i = 0; i < ENGINES_COUNT; i++)
      {
        InstantFailure tmp = new($"Engine {i} Failure", new EventSimConPoint($"TOGGLE_ENGINE{i}_FAILURE"));
        ret.Failures.Add(tmp);
      }

      return ret;
    }

    private static string ExtractName(string varName)
    {
      throw new NotImplementedException();
    }

    private static List<Failure> BuildSystemFailures()
    {
      string[] tmp =
      {
        "TOGGLE_VACUUM_FAILURE",
        "TOGGLE_ELECTRICAL_FAILURE",
        "TOGGLE_PITOT_BLOCKAGE",
        "TOGGLE_STATIC_PORT_BLOCKAGE",
        "TOGGLE_HYDRAULIC_FAILURE",
        "TOGGLE_PITOT_BLOCKAGE"
      };

      var ret = tmp
        .Select(q => new InstantFailure(ExtractName(q), new EventSimConPoint(q))
        {
          GroupId = "Systems"
        })
        .Cast<Failure>()
        .ToList();

      return ret;
    }

    private static List<Failure> BuildInstrumentFailures()
    {
      string[] vars =
      {
      "PARTIAL PANEL AIRSPEED",
        "PARTIAL PANEL ALTIMETER",
        "PARTIAL PANEL ATTITUDE",
        "PARTIAL PANEL COMPASS",
        "PARTIAL PANEL ELECTRICAL",
        "PARTIAL PANEL AVIONICS",
        "PARTIAL PANEL ENGINE",
        "PARTIAL PANEL FUEL INDICATOR",
        "PARTIAL PANEL HEADING",
        "PARTIAL PANEL VERTICAL VELOCITY",
        "PARTIAL PANEL PITOT",
        "PARTIAL PANEL TURN COORDINATOR",
        "PARTIAL PANEL VACUUM"
      };

      var ret = vars.ToList()
        .Select(q => new InstantFailure($"Instrument {q[8..]} Failure", new VarSimConPoint(q))
        {
          GroupId = "Instruments"
        })
        .Cast<Failure>()
        .ToList();

      return ret;
    }

    public static OneOfFailure BuildBrakeFailures()
    {
      string[] tmp =
      {
        "TOGGLE_TOTAL_BRAKE_FAILURE",
        "TOGGLE_LEFT_BRAKE_FAILURE",
        "TOGGLE_RIGHT_BRAKE_FAILURE"
      };

      OneOfFailure ret = new OneOfFailure("Brake Failure");
      ret.Failures.AddRange(tmp
         .Select(q => new InstantFailure(ExtractName(q), new EventSimConPoint(q)))
         .Cast<Failure>()
         .ToList());

      return ret;
    }

    public static AnyOfFailure BuildFuelFailures()
    {
      string[] tmp =
      {
        "FUEL TANK CENTER LEVEL",
        "FUEL TANK LEFT MAIN LEVEL",
        "FUEL TANK RIGHT MAIN LEVEL"
      };

      AnyOfFailure ret = new("Fuel Tank Failure");
      ret.Failures.AddRange(tmp
         .Select(q => new LeakFailure(ExtractName(q), new VarSimConPoint(q)))
         .Cast<Failure>()
         .ToList());

      return ret;
    }

    public static List<Failure> BuildGearFailures()
    {
      //createRegister(new SimVar(lSimVars.Count(), "GEAR CENTER POSITION", "fCenterGear", POSSIBLE_FAIL_TYPE.STUCK));
      //createRegister(new SimVar(lSimVars.Count(), "GEAR LEFT POSITION", "fLeftGear", POSSIBLE_FAIL_TYPE.STUCK));
      //createRegister(new SimVar(lSimVars.Count(), "GEAR RIGHT POSITION", "fRightGear", POSSIBLE_FAIL_TYPE.STUCK));
    }

    public static List<Failure> BuildFlapsFailures()
    {
      //createRegister(new SimVar(lSimVars.Count(), "TRAILING EDGE FLAPS LEFT PERCENT", "fLeftFlap", POSSIBLE_FAIL_TYPE.STUCK));
      //createRegister(new SimVar(lSimVars.Count(), "TRAILING EDGE FLAPS RIGHT PERCENT", "fRightFlap", POSSIBLE_FAIL_TYPE.STUCK));
    }

    public static List<Failure> BuildSurfacesFailures()
    {
      //createRegister(new SimVar(lSimVars.Count(), "ELEVATOR TRIM POSITION", "fTrimElevator", POSSIBLE_FAIL_TYPE.STUCK));
      //createRegister(new SimVar(lSimVars.Count(), "RUDDER TRIM PCT", "fTrimRudder", POSSIBLE_FAIL_TYPE.STUCK));
      //createRegister(new SimVar(lSimVars.Count(), "AILERON TRIM PCT", "fTrimAileron", POSSIBLE_FAIL_TYPE.STUCK));

      //createRegister(new SimVar(lSimVars.Count(), "ELEVATOR POSITION", "fControlElevator", POSSIBLE_FAIL_TYPE.STUCK));
      //createRegister(new SimVar(lSimVars.Count(), "RUDDER POSITION", "fControlRudder", POSSIBLE_FAIL_TYPE.STUCK));
      //createRegister(new SimVar(lSimVars.Count(), "AILERON POSITION", "fControlAileron", POSSIBLE_FAIL_TYPE.STUCK));
    }

    public static List<Failure> Build
  }
}
