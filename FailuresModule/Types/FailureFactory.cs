using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Printing;
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
          Failure tmp = (Failure)(fun.Invoke(null, null) ?? throw new NullReferenceException());
          ret.Add(tmp);
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
  }
}
