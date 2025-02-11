using ELogging;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimConWrapping.PrdefinedTypes;
using Eng.Chlaot.Libs.AirportsLib;
using Eng.Chlaot.Modules.RaaSModule.Model;
using ESystem.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Eng.Chlaot.Modules.RaaSModule.Context;

namespace Eng.Chlaot.Modules.RaaSModule.ContextHandlers
{
  internal class LineUpContextHandler : ContextHandler
  {
    private const int MAX_HEIGHT_TO_BE_EXPECTED_ON_THE_GROUND_IN_FT = 50;
    private const int MAX_SHIFT_DISTANCE_TO_BE_ON_RUNWAY_IN_M = 60;
    private const double MAX_DELTA_HEADING_TO_BE_LINED_UP_ON_RUNWAY_IN_DEG = 15;
    private RunwayThreshold? lastThreshold;

    public LineUpContextHandler(Logger logger, Context.RuntimeDataBox data, Raas raas, Func<SimDataStruct> simDataProvider) : base(logger, data, raas, simDataProvider)
    {
    }

    public override void Handle()
    {
      Debug.Assert(data.NearestAirport != null);
      var simData = simDataProvider();

      if (simData.Height > MAX_HEIGHT_TO_BE_EXPECTED_ON_THE_GROUND_IN_FT)
      {
        data.GroundLineUpStatus = $"Plane probably airborne - height {simData.Height} " +
          $"over limit {MAX_HEIGHT_TO_BE_EXPECTED_ON_THE_GROUND_IN_FT}";
        lastThreshold = null;
        return;
      }

      var rwyWithMinDistance = this.data.NearestRunways.First();
      if (rwyWithMinDistance.ShiftDistance > MAX_SHIFT_DISTANCE_TO_BE_ON_RUNWAY_IN_M)
      {
        data.GroundLineUpStatus = $"Threshold {data.NearestAirport.Airport.ICAO}/{rwyWithMinDistance.Runway.Designator}" +
          $" shift-distance {rwyWithMinDistance.ShiftDistance} over threshold" +
          $" {MAX_SHIFT_DISTANCE_TO_BE_ON_RUNWAY_IN_M}";
        lastThreshold = null;
      }
      else
      {
        //TODO here probably the calculation should me done only for the lowest shift distance
        this.data.GroundLineUp = rwyWithMinDistance.Runway.Thresholds
          .Select(q => new
          {
            Threshold = q,
            Bearing = GpsCalculator.InitialBearing(q.Coordinate.Latitude, q.Coordinate.Longitude, 
            simData.latitude, simData.longitude),
            Distance = GpsCalculator.GetDistance(q.Coordinate.Latitude, q.Coordinate.Longitude, 
            simData.latitude, simData.longitude)
          })
          .Select(q => new GroundRaasLineUpData(
            data.NearestAirport.Airport,
            rwyWithMinDistance.Runway,
            q.Threshold,
            (Heading)q.Bearing,
            q.Distance,
            Math.Abs((double)q.Threshold.Heading! - (double)simData.Heading)))
          .OrderBy(q => q.DeltaHeading)
          .ToList();

        var thresholdCandidate = this.data.GroundLineUp.MinBy(q => q.DeltaHeading) 
          ?? throw new UnexpectedNullException();
        if (thresholdCandidate.DeltaHeading < MAX_DELTA_HEADING_TO_BE_LINED_UP_ON_RUNWAY_IN_DEG)
        {
          if (lastThreshold != thresholdCandidate.Threshold)
          {
            lastThreshold = thresholdCandidate.Threshold;
            Say(raas.Speeches.OnRunway, thresholdCandidate.Threshold);
            data.GroundLineUpStatus =
              $"Threshold {thresholdCandidate.Airport.ICAO}/{thresholdCandidate.Threshold.Designator} " +
              $"announced";
          }
          else
          {
            data.GroundLineUpStatus =
              $"Threshold {thresholdCandidate.Airport.ICAO}/{thresholdCandidate.Threshold.Designator} " +
              $"already announced";
          }
        } else
        {
          data.GroundLineUpStatus =
              $"Threshold {thresholdCandidate.Airport.ICAO}/{thresholdCandidate.Threshold.Designator} " +
              $"is close, but heading diff {thresholdCandidate.DeltaHeading} is too big (over {MAX_DELTA_HEADING_TO_BE_LINED_UP_ON_RUNWAY_IN_DEG}).";
        }
      }
    }
  }
}
