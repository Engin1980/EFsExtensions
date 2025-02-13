using ELogging;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimConWrapping.PrdefinedTypes;
using Eng.Chlaot.Libs.AirportsLib;
using Eng.Chlaot.Modules.RaaSModule.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.RaaSModule.ContextHandlers
{
  internal class RemainingDistanceContextHandler : ContextHandler
  {
    private const int MAX_HEIGHT_TO_BE_ON_THE_GROUND_IN_M = 30;
    private const int MAX_SHIFT_DISTANCE_TO_BE_ON_RUNWAY_IN_M = 60;
    private const double MAX_HEADING_DELTA_TO_BE_ALIGNED_ON_RUNWAY = 15;

    private RunwayThreshold? lastDistanceThreshold;
    private List<RaasDistance>? lastDistanceThresholdRemainingDistances;

    public RemainingDistanceContextHandler(Logger logger, Context.RuntimeDataBox data, Raas raas,
      Func<SimDataStruct> simDataProvider) : base(logger, data, raas, simDataProvider)
    {
    }

    public override void Handle()
    {
      Debug.Assert(data.NearestAirport != null);
      var simData = this.simDataProvider();

      var ds = new List<string>();

      if (simData.Height > MAX_HEIGHT_TO_BE_ON_THE_GROUND_IN_M)
      {

        ds.Add($"Plane probably airborne - height {simData.Height} " +
          $"over limit {MAX_HEIGHT_TO_BE_ON_THE_GROUND_IN_M}");
        lastDistanceThreshold = null;
        lastDistanceThresholdRemainingDistances = null;
      }
      else
      {

        var airport = data.NearestAirport.Airport;
        ds.Add("Current airport: " + airport.ICAO);

        var candidateRwy = data.NearestRunways.First();
        ds.Add($"Closest runway: {airport.ICAO}/{candidateRwy.Runway.Designator}");
        if (candidateRwy.ShiftDistance > MAX_SHIFT_DISTANCE_TO_BE_ON_RUNWAY_IN_M)
        {
          ds.Add(
            $"{airport.ICAO}/{candidateRwy.Runway.Designator} shift-distance {candidateRwy.ShiftDistance} " +
            $"over threshold {MAX_SHIFT_DISTANCE_TO_BE_ON_RUNWAY_IN_M}m");
        }
        else
        {
          var tmps = candidateRwy.Runway.Thresholds
            .Select(q => new
            {
              Threshold = q,
              //TODO rewrite DeltaBearing to be valid w.r.t headings
              DeltaBearing = Math.Abs((double)(simData.Heading - ((double)((Heading)q.Heading! - airport.Declination + 180))))
            })
            .ToList();
          tmps = tmps
            .Where(q => q.DeltaBearing < MAX_HEADING_DELTA_TO_BE_ALIGNED_ON_RUNWAY)
                .OrderBy(q => q.DeltaBearing)
                .ToList();

          if (tmps.Count == 0)
          {
            ds.Add(
              $"{airport.ICAO}/{candidateRwy.Runway.Designator} no threshold within " +
              $"{MAX_HEADING_DELTA_TO_BE_ALIGNED_ON_RUNWAY} degrees bearing-delta");
          }
          else
          {

            var candidate = tmps.First();
            ds.Add(
              $"{airport.ICAO}/{candidateRwy.Runway.Designator} threshold {candidate.Threshold.Designator} " +
              $"bearing-delta {candidate.DeltaBearing} degrees");
            var dist = GpsCalculator.GetDistance(candidate.Threshold.Coordinate.Latitude, candidate.Threshold.Coordinate.Longitude, simData.latitude, simData.longitude);
            ds.Add($"Distance to threshold: {dist}m");

            if (candidate.Threshold != lastDistanceThreshold)
            {
              lastDistanceThreshold = candidate.Threshold;
              lastDistanceThresholdRemainingDistances = raas.Speeches.DistanceRemaining.Distances
                .OrderBy(q => q.GetInMeters())
                .ToList();
            }

            var candidateDistances = lastDistanceThresholdRemainingDistances!
              .Where(q => q.GetInMeters() > dist)
              .OrderBy(q => q.GetInMeters());
            if (!candidateDistances.Any())
            {
              ds.Add("No distances to announce of remaining " + string.Join(", ", lastDistanceThresholdRemainingDistances!));
            }
            else
            {
              var candidateDistance = candidateDistances.First();
              lastDistanceThresholdRemainingDistances!.RemoveAll(q => q.GetInMeters() >= candidateDistance.GetInMeters());

              ds.Add("Announcing distance: " + candidateDistance);
              Say(raas.Speeches.DistanceRemaining, candidateDistance);
            }
          }
        }
      }
      data.DistanceStates = ds;
    }
  }
}
