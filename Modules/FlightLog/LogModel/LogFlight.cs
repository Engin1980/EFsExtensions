using Eng.EFsExtensions.Libs.AirportsLib;
using ESystem.Asserting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule.LogModel
{
  public enum DivertReason
  {
    NotDiverted,
    Weather,
    Medical,
    Mechanical,
    Other
  }
  public class LogStartUp
  {
    public DateTime? ScheduledTime { get; set; }
    public DateTime RealTime { get; set; }
    public int RealFuelAmountKg { get; set; }
    public GPS Location { get; set; }

    public LogStartUp(DateTime? scheduledTime, DateTime realTime, int realFuelAmountKg, GPS location)
    {
      ScheduledTime = scheduledTime;
      RealTime = realTime;
      RealFuelAmountKg = realFuelAmountKg;
      Location = location;
    }

    public LogStartUp()
    {
    }
  }

  public class LogTakeOff
  {
    public DateTime? ScheduledTime { get; set; }
    public DateTime RealTime { get; set; }
    public int? ScheduledFuelAmountKg { get; set; }
    public int FuelAmountKg { get; set; }
    public GPS Location { get; set; }
    public int IAS { get; set; }

    public LogTakeOff(DateTime? scheduledTime, DateTime realTime, int? scheduledFuelAmountKg, int fuelAmountKg, GPS location, int iAS)
    {
      ScheduledTime = scheduledTime;
      RealTime = realTime;
      ScheduledFuelAmountKg = scheduledFuelAmountKg;
      FuelAmountKg = fuelAmountKg;
      Location = location;
      IAS = iAS;
    }

    public LogTakeOff()
    {
    }
  }

  public class LogTouchdown
  {
    public DateTime DateTime { get; set; }
    public GPS Location { get; set; }
    public int IAS { get; set; }
    public double Bank { get; set; }
    public double Pitch { get; set; }
    public double MaxAccY { get; set; }
    public double MainGearTime { get; set; }
    public double AllGearTime { get; set; }
  }

  public class LogLanding
  {
    public DateTime? ScheduledTime { get; set; }
    public List<LogTouchdown> Touchdowns { get; set; } = null!;
    public DateTime? RealTime => Touchdowns.LastOrDefault()?.DateTime;
    public int? ScheduledFuelAmountKg { get; set; }
    public int FuelAmountKg { get; set; }
    public GPS Location => Touchdowns.Last().Location;
    public int IAS => Touchdowns.Last().IAS;
    public double Bank => Touchdowns.Last().Bank;
    public double Pitch => Touchdowns.Last().Pitch;

    public LogLanding(DateTime? scheduledTime, int? scheduledFuelAmountKg, int fuelAmountKg, List<LogTouchdown> touchdowns)
    {
      EAssert.Argument.IsTrue(touchdowns.Count > 0, nameof(touchdowns), "Touchdowns must have at least one entry");

      ScheduledTime = scheduledTime;
      this.Touchdowns = touchdowns;
      ScheduledFuelAmountKg = scheduledFuelAmountKg;
      FuelAmountKg = fuelAmountKg;
    }

    public LogLanding()
    {
    }
  }

  public class LogShutDown
  {
    public DateTime? ScheduledTime { get; set; }
    public DateTime RealTime { get; set; }
    public int FuelAmountKg { get; set; }
    public GPS Location { get; set; }

    public LogShutDown(DateTime? scheduledTime, DateTime realTime, int fuelAmountKg, GPS location)
    {
      ScheduledTime = scheduledTime;
      RealTime = realTime;
      FuelAmountKg = fuelAmountKg;
      Location = location;
    }

    public LogShutDown()
    {
    }
  }

  public class LogFlight
  {
    public string? DepartureICAO { get; set; }
    public string? DestinationICAO { get; set; }
    public string? AlternateICAO { get; set; }
    public double ZFW { get; set; }
    public int? PassengerCount { get; set; }
    public int? CargoWeight { get; set; }
    public int? FuelWeight { get; set; }
    public string? AircraftType { get; set; }
    public string? AircraftRegistration { get; set; }
    public string? AircraftModel { get; set; }
    public int CruizeAltitude { get; set; }
    public double AirDistance { get; set; }
    public double? FlightDistance { get; set; }
    public LogStartUp StartUp { get; set; } = null!;
    public LogTakeOff TakeOff { get; set; } = null!;
    public LogLanding Landing { get; set; } = null!;
    public LogShutDown ShutDown { get; set; } = null!;
    public DivertReason? DivertReason { get; set; } = null!;

    public LogFlight()
    {
    }

    public LogFlight(string? departureICAO, string? destinationICAO, string? alternateICAO, double zFW, int? passengerCount, int? cargoWeight, int? fuelWeight, string? aircraftType, string? aircraftRegistration, string? aircraftModel, int cruizeAltitude, double airDistance, double? flightDistance, LogStartUp startUp, LogTakeOff takeOff, LogLanding landing, LogShutDown shutDown, DivertReason? divertReason)
    {
      DepartureICAO = departureICAO;
      DestinationICAO = destinationICAO;
      AlternateICAO = alternateICAO;
      ZFW = zFW;
      PassengerCount = passengerCount;
      CargoWeight = cargoWeight;
      FuelWeight = fuelWeight;
      AircraftType = aircraftType;
      AircraftRegistration = aircraftRegistration;
      AircraftModel = aircraftModel;
      CruizeAltitude = cruizeAltitude;
      AirDistance = airDistance;
      FlightDistance = flightDistance;
      StartUp = startUp;
      TakeOff = takeOff;
      Landing = landing;
      ShutDown = shutDown;
      DivertReason = divertReason;
    }
  }

}
