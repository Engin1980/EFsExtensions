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
    private RunwayThreshold? lastThreshold;

    public LineUpContextHandler(ContextHandlerArgs args) : base(args) { }

    public override void Handle()
    {
      Debug.Assert(data.NearestAirport != null);
      var airport = data.NearestAirport.Airport;
      var simData = simDataProvider();
      var sett = this.settings.LineUpThresholds;

      if (simData.Height > sett.MaxHeight)
      {
        data.GroundLineUpStatus = $"Plane probably airborne - height {simData.Height} " +
          $"over limit {sett.MaxHeight}";
        lastThreshold = null;
        return;
      }

      var rwyWithMinDistance = this.data.NearestRunways.First();
      if (rwyWithMinDistance.OrthoDistance > sett.MaxOrthoDistance)
      {
        data.GroundLineUpStatus = $"Threshold {airport.ICAO}/{rwyWithMinDistance.Runway.Designator}" +
          $" ortho-distance {rwyWithMinDistance.OrthoDistance} over threshold" +
          $" {sett.MaxOrthoDistance}";
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
            Math.Abs((double)q.Threshold.Heading! - ((double)simData.Heading + airport.Declination))))
          .OrderBy(q => q.DeltaHeading)
          .ToList();

        var thresholdCandidate = this.data.GroundLineUp.MinBy(q => q.DeltaHeading)
          ?? throw new UnexpectedNullException();
        if (thresholdCandidate.DeltaHeading < sett.MaxHeadingDiff)
        {
          if (lastThreshold != thresholdCandidate.Threshold)
          {
            lastThreshold = thresholdCandidate.Threshold;
            if (simData.IndicatedSpeed > sett.MaxSpeed)
            {
              data.GroundLineUpStatus =
                $"Threshold {thresholdCandidate.Airport.ICAO}/{thresholdCandidate.Threshold.Designator} " +
                $"announcement skipped due to high speed {simData.IndicatedSpeed} (max {sett.MaxSpeed}).";
            }
            else
            {
              Say(raas.Speeches.OnRunway, thresholdCandidate.Threshold);
              data.GroundLineUpStatus =
                $"Threshold {thresholdCandidate.Airport.ICAO}/{thresholdCandidate.Threshold.Designator} " +
                $"announced";
            }
          }
          else
          {
            data.GroundLineUpStatus =
              $"Threshold {thresholdCandidate.Airport.ICAO}/{thresholdCandidate.Threshold.Designator} " +
              $"already announced";
          }
        }
        else
        {
          data.GroundLineUpStatus =
              $"Threshold {thresholdCandidate.Airport.ICAO}/{thresholdCandidate.Threshold.Designator} " +
              $"is close, but heading diff {thresholdCandidate.DeltaHeading} is too big (over {sett.MaxHeadingDiff}).";
        }
      }
    }
  }
}
