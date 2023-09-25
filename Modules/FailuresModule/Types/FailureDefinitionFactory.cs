using ESimConnect;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.Http.Headers;
using System.Printing;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Xps.Serialization;

namespace FailuresModule.Types
{
    internal record NameTriple(string Id, string Name, string Sim);

    public class FailureDefinitionFactory
    {
        private static int ENGINES_COUNT = 4;

        public static List<FailureDefinition> BuildFailures()
        {
            List<FailureDefinition> ret = new();

            var funs = typeof(FailureDefinitionFactory)
              .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
              .Where(q => q.Name.StartsWith("Build") && q.Name != "BuildFailures")
              .ToList();

            foreach (var fun in funs)
            {
                try
                {
                    object tmp = fun.Invoke(null, null) ?? throw new NullReferenceException();
                    if (tmp is FailureDefinition f)
                        ret.Add(f);
                    else if (tmp is List<FailureDefinition> l)
                        ret.AddRange(l);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Failed to invoke building method {fun.Name}.", ex);
                }
            }
            return ret;
        }

        private static List<FailureDefinition> BuildEngineFire()
        {
            List<FailureDefinition> ret = new();

            for (int i = 1; i <= ENGINES_COUNT; i++)
            {
                InstantFailureDefinition f = new($"engFire{i}", $"Engine {i} fire", new VarSimConPoint($"ENGINE ON FIRE:{i}"));
                ret.Add(f);
            }

            return ret;
        }

        private static List<FailureDefinition> BuildEngineTurbochanger()
        {
            List<FailureDefinition> ret = new();

            for (int i = 1; i <= ENGINES_COUNT; i++)
            {
                InstantFailureDefinition f = new($"engTurbo{i}", $"Engine {i} TurboChanger", new VarSimConPoint($"RECIP ENG TURBOCHARGER FAILED:{i}"));
                ret.Add(f);
            }

            return ret;
        }

        private static List<FailureDefinition> BuildEngineCoolantReservoir()
        {
            List<FailureDefinition> ret = new();

            for (int i = 1; i <= ENGINES_COUNT; i++)
            {
                LeakFailureDefinition f = new($"engCoolant{i}", $"Engine {i} Coolant Reservoir", new VarSimConPoint($"RECIP ENG COOLANT RESERVOIR PERCENT:{i}"));
                ret.Add(f);
            }

            return ret;
        }

        private static List<FailureDefinition> BuildEngineFailure()
        {
            List<FailureDefinition> ret = new();

            for (int i = 0; i < ENGINES_COUNT; i++)
            {
                InstantFailureDefinition tmp = new($"eng{i}", $"Engine {i} Failure", new EventSimConPoint($"TOGGLE_ENGINE{i}_FAILURE"));
                ret.Add(tmp);
            }

            return ret;
        }


        private static List<FailureDefinition> BuildSystemFailures()
        {
            NameTriple[] tmp =
            {
        new("vacuum", "Vacuum failure", "TOGGLE_VACUUM_FAILURE"),
        new("electrical", "Electrical failure", "TOGGLE_ELECTRICAL_FAILURE"),
        new("pitot", "Pitot blockage", "TOGGLE_PITOT_BLOCKAGE"),
        new("static_port", "Static port blockage", "TOGGLE_STATIC_PORT_BLOCKAGE"),
        new("hydraulics", "Hydraulics failure", "TOGGLE_HYDRAULIC_FAILURE")
      };

            var ret = tmp
              .Select(q => new InstantFailureDefinition(q.Id, q.Name, new EventSimConPoint(q.Sim)))
              .Cast<FailureDefinition>()
              .ToList();

            return ret;
        }

        private static List<FailureDefinition> BuildInstrumentFailures()
        {
            NameTriple[] vars =
            {
        new("pnlAirspeed", "Airspeed (panel)", "PARTIAL PANEL AIRSPEED"),
        new("pnlAltimeter", "Altimeter (panel)",  "PARTIAL PANEL ALTIMETER"),
        new("pnlAttitude", "Attitude (panel)","PARTIAL PANEL ATTITUDE"),
        new("pnlCompass", "Compass (panel)","PARTIAL PANEL COMPASS"),
        new("pnlElectrical", "Electrical (panel)","PARTIAL PANEL ELECTRICAL"),
        new("pnlAvionics", "Avionics (panel)","PARTIAL PANEL AVIONICS"),
        new("pnlEngine", "Engine (panel)", "PARTIAL PANEL ENGINE"),
        new("pnlFuel", "Fuel indicator (panel)","PARTIAL PANEL FUEL INDICATOR"),
        new("pnlHeading", "Heading (panel)", "PARTIAL PANEL HEADING"),
        new("pnlVS", "Vertical velocity (panel)", "PARTIAL PANEL VERTICAL VELOCITY"),
        new("pnlPitot", "Pitot (panel)", "PARTIAL PANEL PITOT"),
        new("pnlTurn", "Turn coordinator (panel)", "PARTIAL PANEL TURN COORDINATOR"),
        new("pnlVacuum", "Vacuum (panel)", "PARTIAL PANEL VACUUM")
      };

            var ret = vars.ToList()
              .Select(q => new InstantFailureDefinition(q.Id, q.Name, new VarSimConPoint(q.Sim)))
              .Cast<FailureDefinition>()
              .ToList();

            return ret;
        }

        private static List<FailureDefinition> BuildBrakeFailures()
        {
            NameTriple[] tmp =
            {
        new("brakeAll", "All brakes","TOGGLE_TOTAL_BRAKE_FAILURE"),
        new("brakeLeft", "Left brake", "TOGGLE_LEFT_BRAKE_FAILURE"),
        new("brakeRight", "Right brake", "TOGGLE_RIGHT_BRAKE_FAILURE")
      };

            var ret = tmp.ToList()
              .Select(q => new InstantFailureDefinition(q.Id, q.Name, new EventSimConPoint(q.Sim)))
              .Cast<FailureDefinition>()
              .ToList();

            return ret;
        }

        private static List<FailureDefinition> BuildFuelFailures()
        {
            NameTriple[] tmp =
            {
        new("fuelCenter", "Center Fuel Tank", "FUEL TANK CENTER LEVEL"),
        new("fuelLeft", "Left Fuel Tank", "FUEL TANK LEFT MAIN LEVEL"),
        new("fuelRight", "Right Fuel Tank", "FUEL TANK RIGHT MAIN LEVEL")
      };

            var ret = tmp
               .Select(q => new LeakFailureDefinition(q.Id, q.Name, new VarSimConPoint(q.Sim)))
               .Cast<FailureDefinition>()
               .ToList();

            return ret;
        }

        private static List<FailureDefinition> BuildGearFailures()
        {
            NameTriple[] tmp =
            {
        new ("gearCenter", "Gear Center", "GEAR CENTER POSITION"),
        new ("gearLeft", "Left Gear", "GEAR LEFT POSITION"),
        new ("gearRight", "Right Gear", "GEAR RIGHT POSITION")
      };

            var ret = tmp.ToList()
              .Select(q => new StuckFailureDefinition(q.Id, q.Name, new VarSimConPoint(q.Sim)))
              .Cast<FailureDefinition>()
              .ToList();

            return ret;
        }

        private static List<FailureDefinition> BuildFlapsFailures()
        {
            NameTriple[] tmp =
            {
        new ("flapsLeft", "Left Flaps", "TRAILING EDGE FLAPS LEFT PERCENT"),
        new ("flapsRight", "Right Flaps", "TRAILING EDGE FLAPS RIGHT PERCENT")
      };

            var ret = tmp.ToList()
              .Select(q => new StuckFailureDefinition(q.Id, q.Name, new VarSimConPoint(q.Sim)))
              .Cast<FailureDefinition>()
              .ToList();

            return ret;
        }

        private static List<FailureDefinition> BuildSurfacesFailures()
        {
            NameTriple[] tmp =
            {
        new("rudderTrim", "Rudder Trim", "RUDDER TRIM PCT"),
        new("aileronTrim", "Aileron Trim", "AILERON TRIM PCT"),
        new("elevatorTrim", "Elevator Trim", "ELEVATOR TRIM POSITION"),
        new ("elevator", "Elevator", "ELEVATOR POSITION"),
        new("aileron", "Aileron", "AILERON POSITION"),
        new("rudder", "Rudder", "RUDDER POSITION")
      };

            var ret = tmp.ToList()
              .Select(q => new StuckFailureDefinition(q.Id, q.Name, new VarSimConPoint(q.Sim)))
              .Cast<FailureDefinition>()
              .ToList();

            return ret;
        }
    }
}
