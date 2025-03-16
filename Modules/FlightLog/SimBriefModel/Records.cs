using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule.SimBriefModel
{
  public record Fuel(int Taxi, int PlanTakeoff);
  public record Weights(int PaxCount, int Payload, int Cargo, int estZfw, int estLw, int estTow);
  public record Aircraft(string IcaoCode, string BaseType, string Name, string Reg);
  /// <summary>
  /// 
  /// </summary>
  /// <param name="EstTimeEnroute"></param>
  /// <param name="SchedOut">Block-off time</param>
  /// <param name="SchedOn">Landed time</param>
  /// <param name="SchedOff">Departed time</param>
  /// <param name="SchedIn">Block-on time</param>
  /// <param name="OriginTimezone"></param>
  /// <param name="DestTimeZone"></param>
  /// <param name="TaxiOut"></param>
  /// <param name="TaxiIn"></param>
  public record Times(long EstTimeEnroute, long SchedOut, long SchedOn, long SchedOff, long SchedIn, int OriginTimezone, int DestTimeZone, long TaxiOut, long TaxiIn);
  public record Fetch(int UserId, string? StaticId, string Status, double Time);
  public record Params(long RequestId, string SequenceId, string? StaticId, int UserId, long TimeGenerated, string XmlFile, string OfpLayout, int Airac, string Units);
  public record General(int Release, string IcaoAirline, string FlightNumber, bool IsEtops, string DxRmk, string? SysRmk,
                        bool IsDetailedProfile, string CruiseProfile, string ClimbProfile, string DescentProfile, string AlternateProfile,
                        string ReserveProfile, int CostIndex, string ContRule, int InitialAltitude, string StepClimbString, int AvgTempDev,
                        int AvgTropopause, int AvgWindComp, int AvgWindDir, int AvgWindSpd, int GcDistance, int RouteDistance, int AirDistance,
                        int TotalBurn, int CruiseTas, string CruiseMach, int Passengers, string Route, string RouteIfps, string RouteNavigraph,
                        string SidIdent, string? SidTrans, string StarIdent, string StarTrans);

  public record Location(string IcaoCode, string IataCode, string? FaaCode, string IcaoRegion, int Elevation, double PosLat, double PosLong, string Name, int Timezone, string PlanRwy, int TransAlt, int TransLevel, string Metar, DateTime MetarTime, string MetarCategory, int MetarVisibility, int MetarCeiling, string Taf, DateTime TafTime, List<Atis> Atis, List<Notam> Notams);
  public record Atis(string Network, DateTime Issued, string Letter, string Phonetic, string Type, string Message);
  public record Notam(string SourceId, string AccountId, string NotamId, string LocationId, string LocationIcao, string LocationName, string LocationType, DateTime DateCreated, DateTime DateEffective, DateTime DateExpire, string? DateExpireIsEstimated, DateTime DateModified, string? NotamSchedule, string NotamHtml, string NotamText, string NotamRaw, string NotamNrc, string NotamQcode, string NotamQcodeCategory, string NotamQcodeSubject, string NotamQcodeStatus, string? NotamIsObstacle);
}
