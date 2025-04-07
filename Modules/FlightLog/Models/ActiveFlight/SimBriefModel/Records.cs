using System.Xml.Serialization;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Models.ActiveFlight.SimBriefModel
{
  public class Fuel
  {
    [XmlElement("taxi")]
    public int Taxi { get; set; }

    [XmlElement("plan_takeoff")]
    public int PlanTakeoff { get; set; }
  }

  public class Atc
  {
    [XmlElement("flight_rules")]
    public string FlightRules { get; set; } = null!;
    [XmlElement("route")]
    public string Route { get; set; } = null!;
    [XmlElement("callsign")]
    public string Callsign { get; set; } = null!;
  }

  public class Weights
  {
    [XmlElement("pax_count")]
    public int PaxCount { get; set; }

    [XmlElement("payload")]
    public int Payload { get; set; }

    [XmlElement("cargo")]
    public int Cargo { get; set; }

    [XmlElement("est_zfw")]
    public int EstZfw { get; set; }

    [XmlElement("est_ldw")]
    public int EstLdw { get; set; }

    [XmlElement("est_tow")]
    public int EstTow { get; set; }
  }

  public class Aircraft
  {
    [XmlElement("icao_code")]
    public string IcaoCode { get; set; } = null!;

    [XmlElement("base_type")]
    public string BaseType { get; set; } = null!;

    [XmlElement("name")]
    public string Name { get; set; } = null!;

    [XmlElement("reg")]
    public string Reg { get; set; } = null!;
  }

  public class Times
  {
    [XmlElement("est_time_enroute")]
    public long EstTimeEnroute { get; set; }

    [XmlElement("sched_out")]
    public long SchedOut { get; set; }

    [XmlElement("sched_on")]
    public long SchedOn { get; set; }

    [XmlElement("sched_off")]
    public long SchedOff { get; set; }

    [XmlElement("sched_in")]
    public long SchedIn { get; set; }

    [XmlElement("origin_timezone")]
    public int OriginTimezone { get; set; }

    [XmlElement("dest_time_zone")]
    public int DestTimeZone { get; set; }

    [XmlElement("taxi_out")]
    public long TaxiOut { get; set; }

    [XmlElement("taxi_in")]
    public long TaxiIn { get; set; }
  }

  public class Fetch
  {
    [XmlElement("user_id")]
    public int UserId { get; set; }

    [XmlElement("static_id")]
    public string? StaticId { get; set; }

    [XmlElement("status")]
    public string Status { get; set; } = null!;

    [XmlElement("time")]
    public double Time { get; set; }
  }

  public class Params
  {
    [XmlElement("request_id")]
    public long RequestId { get; set; }

    [XmlElement("sequence_id")]
    public string SequenceId { get; set; } = null!;

    [XmlElement("static_id")]
    public string? StaticId { get; set; }

    [XmlElement("user_id")]
    public int UserId { get; set; }

    [XmlElement("time_generated")]
    public long TimeGenerated { get; set; }

    [XmlElement("xml_file")]
    public string XmlFile { get; set; } = null!;

    [XmlElement("ofp_layout")]
    public string OfpLayout { get; set; } = null!;

    [XmlElement("airac")]
    public int Airac { get; set; }

    [XmlElement("units")]
    public string Units { get; set; } = null!;
  }

  public class General
  {
    [XmlElement("release")]
    public int Release { get; set; }

    [XmlElement("icao_airline")]
    public string IcaoAirline { get; set; } = null!;

    [XmlElement("flight_number")]
    public string FlightNumber { get; set; } = null!;

    [XmlElement("is_etops")]
    public bool IsEtops { get; set; }

    [XmlElement("dx_rmk")]
    public string DxRmk { get; set; } = null!;

    [XmlElement("sys_rmk")]
    public string? SysRmk { get; set; }

    [XmlElement("is_detailed_profile")]
    public bool IsDetailedProfile { get; set; }

    [XmlElement("cruise_profile")]
    public string CruiseProfile { get; set; } = null!;

    [XmlElement("climb_profile")]
    public string ClimbProfile { get; set; } = null!;

    [XmlElement("descent_profile")]
    public string DescentProfile { get; set; } = null!;

    [XmlElement("alternate_profile")]
    public string AlternateProfile { get; set; } = null!;

    [XmlElement("reserve_profile")]
    public string ReserveProfile { get; set; } = null!;

    [XmlElement("cost_index")]
    public int CostIndex { get; set; }

    [XmlElement("cont_rule")]
    public string ContRule { get; set; } = null!;

    [XmlElement("initial_altitude")]
    public int InitialAltitude { get; set; }

    [XmlElement("step_climb_string")]
    public string StepClimbString { get; set; } = null!;

    [XmlElement("avg_temp_dev")]
    public int AvgTempDev { get; set; }

    [XmlElement("avg_tropopause")]
    public int AvgTropopause { get; set; }

    [XmlElement("avg_wind_comp")]
    public int AvgWindComp { get; set; }

    [XmlElement("avg_wind_dir")]
    public int AvgWindDir { get; set; }

    [XmlElement("avg_wind_spd")]
    public int AvgWindSpd { get; set; }

    [XmlElement("gc_distance")]
    public int GcDistance { get; set; }

    [XmlElement("route_distance")]
    public int RouteDistance { get; set; }

    [XmlElement("air_distance")]
    public int AirDistance { get; set; }

    [XmlElement("total_burn")]
    public int TotalBurn { get; set; }

    [XmlElement("cruise_tas")]
    public int CruiseTas { get; set; }

    [XmlElement("cruise_mach")]
    public string CruiseMach { get; set; } = null!;

    [XmlElement("passengers")]
    public int Passengers { get; set; }

    [XmlElement("route")]
    public string Route { get; set; } = null!;

    [XmlElement("route_ifps")]
    public string RouteIfps { get; set; } = null!;

    [XmlElement("route_navigraph")]
    public string RouteNavigraph { get; set; } = null!;

    [XmlElement("sid_ident")]
    public string SidIdent { get; set; } = null!;

    [XmlElement("sid_trans")]
    public string? SidTrans { get; set; }

    [XmlElement("star_ident")]
    public string StarIdent { get; set; } = null!;

    [XmlElement("star_trans")]
    public string StarTrans { get; set; } = null!;
  }

  public class Location
  {
    [XmlElement("icao_code")]
    public string IcaoCode { get; set; } = null!;

    [XmlElement("iata_code")]
    public string IataCode { get; set; } = null!;

    [XmlElement("faa_code")]
    public string? FaaCode { get; set; }

    [XmlElement("icao_region")]
    public string IcaoRegion { get; set; } = null!;

    [XmlElement("elevation")]
    public int Elevation { get; set; }

    [XmlElement("pos_lat")]
    public double PosLat { get; set; }

    [XmlElement("pos_long")]
    public double PosLong { get; set; }

    [XmlElement("name")]
    public string Name { get; set; } = null!;

    [XmlElement("timezone")]
    public int Timezone { get; set; }

    [XmlElement("plan_rwy")]
    public string PlanRwy { get; set; } = null!;

    [XmlElement("trans_alt")]
    public int TransAlt { get; set; }

    [XmlElement("trans_level")]
    public int TransLevel { get; set; }

    [XmlElement("metar")]
    public string Metar { get; set; } = null!;

    [XmlElement("metar_time")]
    public DateTime MetarTime { get; set; }

    [XmlElement("metar_category")]
    public string MetarCategory { get; set; } = null!;

    [XmlElement("metar_visibility")]
    public int MetarVisibility { get; set; }

    [XmlElement("metar_ceiling")]
    public int MetarCeiling { get; set; }

    [XmlElement("taf")]
    public string Taf { get; set; } = null!;

    [XmlElement("taf_time")]
    public DateTime TafTime { get; set; }
  }
}
