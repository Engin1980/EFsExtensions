using ELogging;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimConWrapping.PrdefinedTypes;
using Eng.EFsExtensions.Libs.AirportsLib;
using Eng.EFsExtensions.Modules.RaaSModule.Model;
using ESystem.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Eng.EFsExtensions.Modules.RaaSModule.Context;

namespace Eng.EFsExtensions.Modules.RaaSModule.ContextHandlers
{
  internal class LineUpContextHandler : ContextHandler
  {
    private RunwayThreshold? lastThreshold;

    public LineUpContextHandler(ContextHandlerArgs args) : base(args) { }

    public override void Handle()
    {
      Debug.Assert(data.NearestAirport != null);
      var simDataSnapshot = simDataSnapshotProvider.GetSnapshot();
      var airport = data.NearestAirport.Airport;
      var sett = this.settings.LineUpThresholds;

      if (simDataSnapshot.Height > sett.MaxHeight)
      {
        data.LineUpStatus = $"Plane probably airborne - height {simDataSnapshot.Height} " +
          $"over limit {sett.MaxHeight}";
        lastThreshold = null;
        return;
      }

      var rwyWithMinDistance = this.data.NearestRunways.First();
      if (rwyWithMinDistance.OrthoDistance > sett.MaxOrthoDistance)
      {
        data.LineUpStatus = $"Threshold {airport.ICAO}/{rwyWithMinDistance.Runway.Designator}" +
          $" ortho-distance {rwyWithMinDistance.OrthoDistance} over threshold" +
          $" {sett.MaxOrthoDistance}";
        lastThreshold = null;
      }
      else
      {
        //TODO here probably the calculation should me done only for the lowest shift distance
        this.data.LineUp = rwyWithMinDistance.Runway.Thresholds
          .Select(q => new
          {
            Threshold = q,
            Bearing = GpsCalculator.InitialBearing(q.Coordinate.Latitude, q.Coordinate.Longitude,
            simDataSnapshot.Latitude, simDataSnapshot.Longitude),
            Distance = GpsCalculator.GetDistance(q.Coordinate.Latitude, q.Coordinate.Longitude,
            simDataSnapshot.Latitude, simDataSnapshot.Longitude)
          })
          .Select(q => new LineUpData(
            data.NearestAirport.Airport,
            rwyWithMinDistance.Runway,
            q.Threshold,
            (Heading)q.Bearing,
            q.Distance,
            Math.Abs((double)q.Threshold.Heading! - ((double)simDataSnapshot.Heading + airport.Declination))))
          .OrderBy(q => q.DeltaHeading)
          .ToList();

        var thresholdCandidate = this.data.LineUp.MinBy(q => q.DeltaHeading)
          ?? throw new UnexpectedNullException();
        if (thresholdCandidate.DeltaHeading < sett.MaxHeadingDiff)
        {
          if (lastThreshold != thresholdCandidate.Threshold)
          {
            lastThreshold = thresholdCandidate.Threshold;
            if (simDataSnapshot.IndicatedSpeed > sett.MaxSpeed)
            {
              data.LineUpStatus =
                $"Threshold {thresholdCandidate.Airport.ICAO}/{thresholdCandidate.Threshold.Designator} " +
                $"announcement skipped due to high speed {simDataSnapshot.IndicatedSpeed} (max {sett.MaxSpeed}).";
            }
            else
            {
              Say(raas.Speeches.OnRunway, thresholdCandidate.Threshold);
              data.LineUpStatus =
                $"Threshold {thresholdCandidate.Airport.ICAO}/{thresholdCandidate.Threshold.Designator} " +
                $"announced";
            }
          }
          else
          {
            data.LineUpStatus =
              $"Threshold {thresholdCandidate.Airport.ICAO}/{thresholdCandidate.Threshold.Designator} " +
              $"already announced";
          }
        }
        else
        {
          data.LineUpStatus =
              $"Threshold {thresholdCandidate.Airport.ICAO}/{thresholdCandidate.Threshold.Designator} " +
              $"is close, but heading diff {thresholdCandidate.DeltaHeading} is too big (over {sett.MaxHeadingDiff}).";
        }
      }
    }
  }
}
