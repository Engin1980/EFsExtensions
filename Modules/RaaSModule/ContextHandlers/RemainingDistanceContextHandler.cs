﻿using ESystem.Logging;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimConWrapping.PrdefinedTypes;
using Eng.EFsExtensions.Libs.AirportsLib;
using Eng.EFsExtensions.Modules.RaaSModule.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.RaaSModule.ContextHandlers
{
  internal class RemainingDistanceContextHandler : ContextHandler
  {
    private RunwayThreshold? lastDistanceThreshold;
    private List<RaasDistance>? lastDistanceThresholdRemainingDistances;
    private int previousIas;
    private int previousHeight;

    public RemainingDistanceContextHandler(ContextHandlerArgs args) : base(args) { }

    public override void Handle()
    {
      Debug.Assert(data.NearestAirport != null);
      var simDataSnapshot = simDataSnapshotProvider();
      var sett = this.settings.RemainingDistanceThresholds;

      int iasDelta = simDataSnapshot.IndicatedSpeed - previousIas;
      previousIas = simDataSnapshot.IndicatedSpeed;
      int heightDelta = simDataSnapshot.Height - previousHeight;
      previousHeight = simDataSnapshot.Height;

      var ds = new List<string>();

      if (simDataSnapshot.Height > sett.MaxHeight)
      {

        ds.Add($"Plane probably airborne - height {simDataSnapshot.Height} " +
          $"over limit {sett.MaxHeight}");
        lastDistanceThreshold = null;
        lastDistanceThresholdRemainingDistances = null;
      }
      else if (iasDelta > 10) //TODO if working, move to thresholds; detects, if plane is deccelerating or moreless stable speed
      {
        ds.Add($"Plane is not deccelerating (ias-diff={iasDelta}");
      }
      else if (heightDelta > 10) //TODO if working, move to thresholds;
      {
        ds.Add($"Plane is not descending (height-díff={heightDelta}");
      }
      else
      {
        var airport = data.NearestAirport.Airport;
        ds.Add($"Current airport: {airport.ICAO} (declination = {airport.Declination})");

        var candidateRwy = data.NearestRunways.First();
        ds.Add($"Closest runway: {airport.ICAO}/{candidateRwy.Runway.Designator}");
        if (candidateRwy.OrthoDistance > sett.MaxOrthoDistance)
        {
          ds.Add(
            $"{airport.ICAO}/{candidateRwy.Runway.Designator} ortho-distance {candidateRwy.OrthoDistance} " +
            $"over threshold {sett.MaxOrthoDistance}m");
        }
        else
        {
          var tmps = candidateRwy.Runway.Thresholds
            .Select(q => new
            {
              Threshold = q,
              //TODO rewrite DeltaBearing to be valid w.r.t headings
              DeltaBearing = Math.Abs((double)(simDataSnapshot.Heading - ((double)((Heading)q.Heading! - airport.Declination + 180))))
            })
            .ToList();
          var passedTmps = tmps
            .Where(q => q.DeltaBearing < sett.MaxHeadingDiff)
                .OrderBy(q => q.DeltaBearing)
                .ToList();

          if (passedTmps.Count == 0)
          {
            ds.Add(
              $"{airport.ICAO}/{candidateRwy.Runway.Designator} no threshold within " +
              $"{sett.MaxHeadingDiff} degrees bearing-delta: " +
              $"{string.Join(",", tmps.Select(q => $"{q.Threshold.Designator}={q.DeltaBearing}"))}.");
          }
          else
          {

            var candidate = passedTmps.First();
            ds.Add(
              $"{airport.ICAO}/{candidateRwy.Runway.Designator} threshold {candidate.Threshold.Designator} " +
              $"bearing-delta {candidate.DeltaBearing} degrees");
            var dist = GpsCalculator.GetDistance(candidate.Threshold.Coordinate.Latitude, candidate.Threshold.Coordinate.Longitude, simDataSnapshot.Latitude, simDataSnapshot.Longitude);
            ds.Add($"Distance to threshold: {dist}m");

            if (candidate.Threshold != lastDistanceThreshold)
            {
              lastDistanceThreshold = candidate.Threshold;
              lastDistanceThresholdRemainingDistances = raas.Variables.AnnouncedRemainingDistances.Value
                .OrderBy(q => q.GetInMeters())
                .ToList();
            }

            var candidateDistances = lastDistanceThresholdRemainingDistances!
              .Where(q => q.GetInMeters() > dist)
              .OrderBy(q => q.GetInMeters());
            if (!candidateDistances.Any())
            {
              ds.Add(
                "No distances to announce of remaining " +
                string.Join(", ", lastDistanceThresholdRemainingDistances!));
            }
            else
            {
              var candidateDistance = candidateDistances.First();
              lastDistanceThresholdRemainingDistances!
                .RemoveAll(q => q.GetInMeters() >= candidateDistance.GetInMeters());

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
