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
  internal class HoldingPointContextHandler : ContextHandler
  {
    private Runway? lastHoldingPointRunway;

    public HoldingPointContextHandler(ContextHandlerArgs args) : base(args) { }

    public override void Handle()
    {
      Debug.Assert(data.NearestAirport != null);
      var simData = simDataProvider();
      var sett = this.settings.HoldingPointThresholds;

      if (simData.Height > sett.MaxHeight)
      {
        data.HoldingPointStatus = $"Plane probably airborne - height {simData.Height} over limit " +
          $"{sett.MaxHeight}";
        lastHoldingPointRunway = null;
        return;
      }

      data.HoldingPoint = data.NearestRunways
        .Select(q => new HoldingPointData(
        data.NearestAirport.Airport,
        q.Runway,
          q.OrthoDistance))
        .OrderBy(q => q.OrthoDistance)
        .ToList();

      var grtd = data.HoldingPoint.First();
      if (grtd.OrthoDistance > sett.TooFarOrthoDistance)
      {
        lastHoldingPointRunway = null;
        data.HoldingPointStatus = $"Best ortho-distance threshold {grtd.Airport.ICAO}/{grtd.Runway.Designator} " +
          $"too far (over {sett.TooFarOrthoDistance}).";
      }
      else if (grtd.OrthoDistance < sett.TooCloseOrthoDistance)
      {
        // entered runway, calls are ignored
        lastHoldingPointRunway = grtd.Runway;
        data.HoldingPointStatus = $"Best ortho-distance threshold {grtd.Airport.ICAO}/{grtd.Runway.Designator} " +
          $"too close (probably on the runway?) (under {sett.TooCloseOrthoDistance}).";
      }
      else
      {
        bool isShortRwy = grtd.Runway.LengthInM <= sett.ShortLongRunwayLengthThreshold;
        var orthoDistance = isShortRwy ? sett.AnnounceOrthoDistanceShortRwy : sett.AnnounceOrthoDistanceLongRwy;
        if (grtd.OrthoDistance < orthoDistance)
        {
          if (lastHoldingPointRunway != grtd.Runway)
          {
            lastHoldingPointRunway = grtd.Runway;
            var closestThreshold = grtd.Runway.Thresholds
              .MinBy(q => GpsCalculator.GetDistance(q.Coordinate.Latitude, q.Coordinate.Longitude, simData.latitude, simData.longitude))
              ?? throw new UnexpectedNullException();
            Say(raas.Speeches.TaxiToRunway, closestThreshold);
            data.HoldingPointStatus = 
              $"Threshold {grtd.Airport.ICAO}/{grtd.Runway.Designator} announced";
          }
          else
          {
            data.HoldingPointStatus = 
              $"Threshold {grtd.Airport.ICAO}/{grtd.Runway.Designator} already announced";
          }
        }
        else
        {
          data.HoldingPointStatus = $"Threshold {grtd.Airport.ICAO}/{grtd.Runway.Designator} " +
            $"ortho-distance {grtd.OrthoDistance} not close enought for announcement ({orthoDistance}).";
        }
      }
    }
  }
}
