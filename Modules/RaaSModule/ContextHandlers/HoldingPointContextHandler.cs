﻿using ESystem.Logging;
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
  internal class HoldingPointContextHandler : ContextHandler
  {
    private Runway? lastHoldingPointRunway;

    public HoldingPointContextHandler(ContextHandlerArgs args) : base(args) { }

    public override void Handle()
    {
      Debug.Assert(data.NearestAirport != null);
      var simDataSnapshot = simDataSnapshotProvider();
      var sett = this.settings.HoldingPointThresholds;

      if (simDataSnapshot.Height > sett.MaxHeight)
      {
        data.HoldingPointStatus = $"Plane probably airborne - height {simDataSnapshot.Height} over limit " +
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
              .MinBy(q => GpsCalculator.GetDistance(q.Coordinate.Latitude, q.Coordinate.Longitude, simDataSnapshot.Latitude, simDataSnapshot.Longitude))
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
