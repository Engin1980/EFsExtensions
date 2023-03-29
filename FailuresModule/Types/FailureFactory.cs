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
      MultiFailure ret = new MultiFailure();

      for (int i = 0; i < ENGINES_COUNT; i++)
      {
        InstantFailure instantFailure = new InstantFailure()
        {
          SimConPoint = new VarSimConPoint($"ENGINE ON FIRE:{i}")
        };

        ret.Failures.Add(instantFailure);
      }

      return ret;
    }

    private static MultiFailure BuildEngineFailure()
    {
      MultiFailure ret = new MultiFailure();

      for (int i = 0; i < ENGINES_COUNT; i++)
      {
        InstantFailure tmp = new()
        {
          SimConPoint = new EventSimConPoint($"TOGGLE_ENGINE{i}_FAILURE")
        };
        ret.Failures.Add(tmp);
      }

      return ret;
    }

    private static InstantFailure BuildPitotFailure()
    {
      InstantFailure ret = new()
      {
        SimConPoint = new EventSimConPoint("TOGGLE_PITOT_BLOCKAGE")
      };

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
        .Select(q => new InstantFailure()
        {
          Title = q[8..],
          SimConPoint = new VarSimConPoint(q),
          GroupId = "Instruments"
        })
        .Cast<Failure>()
        .ToList();

      return ret;
    }
  }
}
