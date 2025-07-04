﻿using Eng.EFsExtensions.EFsExtensionsModuleBase;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.Globals;
using Eng.EFsExtensions.Libs.AirportsLib;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.Shared;
using ESystem.Exceptions;
using ESystem.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Eng.EFsExtensions.Modules.FlightLogModule.LogModel
{
  public class LoggedFlight
  {
    public int Version { get; set; } = 5;
    [XmlIgnore]
    public string? FileName { get; set; } = null;
    public string Callsign { get; set; } = string.Empty;
    public FlightRules FlightRules { get; set; } = FlightRules.Unknown;
    public string? DepartureICAO { get; set; } = string.Empty;
    public string? DestinationICAO { get; set; } = string.Empty;
    public string? AlternateICAO { get; set; } = string.Empty;
    public string LandedICAO { get; set; } = string.Empty;
    public int CruizeAltitude { get; set; }
    public Distance Distance { get; set; }
    public DivertReason? DivertReason { get; set; } = null!;
    public GPS StartupLocation { get; set; }
    public Weight ZFW { get; set; }
    public int? PassengerCount { get; set; }
    public Weight? CargoWeight { get; set; }
    public Weight StartUpFuelWeight { get; set; }
    public string? AircraftType { get; set; }
    public string? AircraftRegistration { get; set; }
    public string? AircraftModel { get; set; }

    public int NumberOfGoArounds { get; set; } = 0;
    public DateTime? StartUpScheduledDateTime { get; set; }
    public DateTime StartUpDateTime { get; set; }
    public DateTime? TakeOffScheduledDateTime { get; set; }
    public DateTime TakeOffDateTime => TakeOff.RunStartDateTime;
    public Weight? TakeOffScheduledFuelWeight { get; set; }
    public Weight TakeOffFuelWeight { get; set; }
    public DateTime? LandingScheduledDateTime { get; set; }
    public DateTime? ScheduledTime { get; set; }
    public List<LoggedFlightTouchdown> Touchdowns { get; set; } = null!;
    public LoggedFlightTouchdown LastTouchdown => Touchdowns.Last();

    public LoggedFlightTakeOff TakeOff { get; set; } = null!;
    public Weight? LandingScheduledFuelWeight { get; set; }
    public Weight LandingFuelWeight { get; set; }
    public DateTime? ShutDownScheduledDateTime { get; set; }
    public DateTime ShutDownDateTime { get; set; }
    public Weight ShutDownFuelWeight { get; set; }
    public GPS ShutDownLocation { get; set; }
    public GPS TakeOffLocation => TakeOff.RunStartLocation;
    public TimeSpan DepartureTaxiTime => this.TakeOffDateTime - this.StartUpDateTime;
    public Speed LandingIAS => LastTouchdown.IAS;
    public double LandingBank => LastTouchdown.Bank;
    public double LandingPitch => LastTouchdown.Pitch;
    public TimeSpan ArrivalTaxiTime => this.ShutDownDateTime - this.LandingDateTime;
    public TimeSpan TaxiTime => this.DepartureTaxiTime + this.ArrivalTaxiTime;
    public TimeSpan AirTime => this.LandingDateTime - this.TakeOffDateTime;
    public TimeSpan? ScheduledAirTime => this.LandingScheduledDateTime != null && this.TakeOffScheduledDateTime != null ? this.LandingScheduledDateTime.Value - this.TakeOffScheduledDateTime.Value : null;
    public TimeSpan BlockTime => this.ShutDownDateTime - this.StartUpDateTime;
    public TimeSpan? ScheduledBlockTime => this.ShutDownScheduledDateTime - this.StartUpScheduledDateTime;
    public Speed TakeOffIAS => TakeOff.IAS;
    public GPS LandingLocation => LastTouchdown.TouchDownLocation;
    public DateTime LandingDateTime => LastTouchdown.TouchDownDateTime;
    public Weight? AirScheduledFuelUsedWeight => LandingScheduledFuelWeight != null && TakeOffScheduledFuelWeight != null ? TakeOffScheduledFuelWeight.Value - LandingScheduledFuelWeight.Value : null;
    public Weight AirFuelUsedWeight => TakeOffFuelWeight - LandingFuelWeight;

    public TimeSpan? StartUpDateTimeDelta => this.StartUpDateTime - this.StartUpScheduledDateTime;
    public TimeSpan? TakeOffDateTimeDelta => this.TakeOffDateTime - this.TakeOffScheduledDateTime;
    public TimeSpan? LandingDateTimeDelta => this.LandingDateTime - this.LandingScheduledDateTime;
    public TimeSpan? ShutDownDateTimeDelta => this.ShutDownDateTime - this.ShutDownScheduledDateTime;
    public TimeSpan? AirTimeDelta => this.AirTime - this.ScheduledAirTime;
    public TimeSpan? BlockTimeDelta => this.BlockTime - this.ScheduledBlockTime;

    public Weight? TakeOffFuelWeightDelta => this.TakeOffFuelWeight - this.TakeOffScheduledFuelWeight;
    public Weight? LandingFuelWeightDelta => this.LandingFuelWeight - this.LandingScheduledFuelWeight;
    public Weight? AirFuelUsedWeightDelta => this.AirFuelUsedWeight - this.AirScheduledFuelUsedWeight;

    public void CheckValidity(out bool resaveNeeded)
    {
      AdjustVersion(out resaveNeeded);
      try
      {
        ValidateAllPropertiesByRead();
      }
      catch (Exception e)
      {
        throw new ApplicationException($"Flight '{Callsign}' is not valid: {e.Message}", e);
      }
    }

    private void ValidateAllPropertiesByRead()
    {
      var props = typeof(LoggedFlight).GetProperties(BindingFlags.Public | BindingFlags.Instance);
      foreach (PropertyInfo prop in props)
      {
        try
        {
          prop.GetValue(this);
        }
        catch (Exception e)
        {
          throw new ApplicationException($"Property '{prop.Name}' cannot be read: {e.Message}", e);
        }
      }
    }

    private void AdjustVersion(out bool resaveNeeded)
    {
      resaveNeeded = false;

      if (this.LandedICAO == null || this.LandedICAO == string.Empty)
      {
        this.LandedICAO = GetLandedAirportICAO();
        resaveNeeded = this.LandedICAO != null;
      }

      if (Version == 1)
      {
        this.LandedICAO = GetLandedAirportICAO();
        Version = 2;
        resaveNeeded = true;
      }
      if (Version == 2)
      {
        this.Distance = Distance.Of(this.Distance.Value, DistanceUnit.Kilometers);
        Version = 3;
        resaveNeeded = true;
      }
      if (Version == 3)
      {
        this.Touchdowns.ForEach(q => q.Bank = Math.Abs(q.Bank));
        Version = 4;
        resaveNeeded = true;
      }
      if (Version == 4)
      {
        this.Touchdowns.ForEach(q => q.SmartVS = Math.Abs(q.VS));
        this.Version = 5;
        resaveNeeded = true;
      }
    }

    private string GetLandedAirportICAO()
    {
      GPS gps = this.Touchdowns.Last().TouchDownLocation;
      var ret = GlobalProvider.Instance.NavData.Airports.TryGetNearestAirport(gps);
      return ret?.ICAO ?? string.Empty;
    }

    public override string ToString() => $"{this.Callsign} ({this.DepartureICAO}-{this.LandedICAO ?? this.DestinationICAO}, {this.StartUpDateTime})";
  }
}
