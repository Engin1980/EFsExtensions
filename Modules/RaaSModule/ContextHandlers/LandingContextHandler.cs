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
  internal class LandingContextHandler : ContextHandler
  {
    private RunwayThreshold? lastLandingThreshold = null;

    public LandingContextHandler(ContextHandlerArgs args) : base(args) { }

    public override void Handle()
    {
      Debug.Assert(data.NearestAirport != null);
      var airport = data.NearestAirport.Airport;
      var sett = this.settings.LandingThresholds;

      if (simData.Height > sett.MaxHeight)
      {
        data.LandingStatus = $"Plane height {simData.Height} over limit {sett.MaxHeight}";
        lastLandingThreshold = null;
        return;
      }
      else if (simData.Height < sett.MinHeight)
      {
        data.LandingStatus = $"Plane height {simData.Height} under limit {sett.MinHeight}";
        lastLandingThreshold = null;
        return;
      }
      else if (simData.VerticalSpeed > sett.MaxVerticalSpeed)
      {
        data.LandingStatus = $"Plane vertical speed {simData.VerticalSpeed} over limit {sett.MaxVerticalSpeed}).";
        return;
      }

        var tmpR = data.NearestRunways; //TODO calculate the next only for the closest runway?
      var tmpT = tmpR.SelectMany(q => q.Runway.Thresholds, (r, t) => new
      {
        Runway = r.Runway,
        Threshold = t,
        OrthoDistance = r.OrthoDistance,
        ThresholdDistance = GpsCalculator.GetDistance(
          t.Coordinate.Latitude, t.Coordinate.Longitude,
          simData.Latitude, simData.Longitude),
        Bearing = GpsCalculator.InitialBearing(
          simData.Latitude, simData.Longitude,
          t.Coordinate.Latitude, t.Coordinate.Longitude)
      });
      data.Landing = tmpT
        .Select(q => new LandingRaasData(airport, q.Runway, q.Threshold, q.OrthoDistance, q.ThresholdDistance, (Heading)q.Bearing))
        .OrderBy(q => q.OrthoDistance).ThenBy(q => q.ThresholdDistance)
        .ToList();

      var thrsCandidate = data.Landing.First() ?? throw new UnexpectedNullException();

      if (thrsCandidate.ThresholdDistance > sett.MaxDistance)
      {
        data.LandingStatus = $"{thrsCandidate.Airport.ICAO}/{thrsCandidate.Threshold.Designator} " +
          $"threshold-distance {thrsCandidate.ThresholdDistance} over limit {sett.MaxDistance}.";
      }
      else if (thrsCandidate.OrthoDistance > sett.MaxOrthoDistance)
      {
        data.LandingStatus = $"{thrsCandidate.Airport.ICAO}/{thrsCandidate.Threshold.Designator} " +
          $"ortho-distance {thrsCandidate.OrthoDistance} over limit {sett.MaxOrthoDistance}.";
      }
      else
      {
        if (lastLandingThreshold != thrsCandidate.Threshold)
        {
          lastLandingThreshold = thrsCandidate.Threshold;
          Say(raas.Speeches.LandingRunway, thrsCandidate.Threshold);
          data.LandingStatus = 
            $"{thrsCandidate.Airport.ICAO}/{thrsCandidate.Threshold.Designator} announced";
        }
        else
        {
          data.LandingStatus = 
            $"{thrsCandidate.Airport.ICAO}/{thrsCandidate.Threshold.Designator} already announced";
        }
      }
    }
  }
}
