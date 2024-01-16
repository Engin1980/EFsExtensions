using ChlaotModuleBase.ModuleUtils.SimConWrapping;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimConExtenders;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimConWrapping;
using ESystem.Asserting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimObjects
{
  public class SimObject
  {
    public record struct SimVarReg(string SimVar, string Unit);

    private readonly ESimConnect.ESimConnect simCon;
    private readonly OpenAsyncExtender openAsyncSimConExtender;
    private readonly SimSecondElapsedExtender secondElapsedSimConExtender;
    private readonly Dictionary<SimProperty, double> simPropertyValues = new();
    private readonly Dictionary<int, SimVarReg> typeIdMapping = new();
    private readonly Dictionary<int, SimVarReg> requestIdMapping = new();
    private readonly Dictionary<SimVarReg, List<SimProperty>> simVarReqMapping = new();
    public delegate void SimPropertyChangedDelegate(SimProperty property, double value);
    public event SimPropertyChangedDelegate? SimPropertyChanged;
    public event Action? SimSecondElapsed;
    public event Action? Started;

    public bool IsSimPaused => this.secondElapsedSimConExtender.IsSimPaused;

    public SimObject(ESimConnect.ESimConnect simCon)
    {
      EAssert.Argument.IsNotNull(simCon, nameof(simCon));
      this.simCon = simCon;
      this.simCon.ThrowsException += SimCon_ThrowsException;
      this.simCon.DataReceived += SimCon_DataReceived;
      this.openAsyncSimConExtender = new(simCon);
      this.openAsyncSimConExtender.Opened += OpenAsyncSimConExtender_Opened;
      this.secondElapsedSimConExtender = new(simCon, false);
      this.secondElapsedSimConExtender.SimSecondElapsed += SecondElapsedSimConExtender_SimSecondElapsed;
    }

    private void SimCon_ThrowsException(ESimConnect.ESimConnect sender, Microsoft.FlightSimulator.SimConnect.SIMCONNECT_EXCEPTION ex)
    {
      throw new ApplicationException("SimCon thows exception: " + ex.ToString());
    }

    private void SecondElapsedSimConExtender_SimSecondElapsed()
    {
      this.SimSecondElapsed?.Invoke();
    }

    private void OpenAsyncSimConExtender_Opened()
    {
      this.Started?.Invoke();
    }

    public void StartAsync()
    {
      this.openAsyncSimConExtender.OpenAsync();
    }

    public double this[string propertyName]
    {
      get
      {
        SimProperty? sp = simPropertyValues.Keys.FirstOrDefault(q => q.Name == propertyName);
        if (sp == null)
          throw new KeyNotFoundException($"Property {propertyName} not found among registered properties.");
        else
          return this[sp];
      }
    }

    public double this[SimProperty simProperty]
    {
      get
      {
        EAssert.IsNotNull(simProperty, nameof(simProperty));

        if (simPropertyValues.ContainsKey(simProperty))
          return simPropertyValues[simProperty];
        else
          throw new KeyNotFoundException($"Property {simProperty.Name} not found among registered properties.");
      }
    }

    public Dictionary<SimProperty, double> GetAllPropertiesWithValues() => new(this.simPropertyValues);

    private void SimCon_DataReceived(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
    {
      double value = (double)e.Data;
      int? requestId = e.RequestId;
      if (requestId == null) return; // not my registered type
      if (requestIdMapping.ContainsKey(requestId.Value) == false) return; // not my registered type
      SimVarReg svr = requestIdMapping[requestId.Value];
      foreach (var simProperty in simVarReqMapping[svr])
      {
        simPropertyValues[simProperty] = value;
        SimPropertyChanged?.Invoke(simProperty, value);
      }
    }

    public void RegisterProperty(SimProperty property)
    {
      EAssert.Argument.IsNotNull(property, nameof(property));
      EAssert.IsTrue(this.openAsyncSimConExtender.IsOpened, "SimObject must be started first.");

      lock (this)
      {
        if (simPropertyValues.Keys.Any(q => q.Name == property.Name))
          throw new ApplicationException(
            $"SimProperty {property.Name} is already registered.");

        RegisterPropertyToSimCon(property);
        this.simPropertyValues[property] = double.NaN;
      }
    }

    private void RegisterPropertyToSimCon(SimProperty property)
    {
      const string DEFAULT_PROPERTY_UNIT = "Number";

      SimVarReg svr = new(property.SimVar, property.Unit ?? DEFAULT_PROPERTY_UNIT);
      if (simVarReqMapping.ContainsKey(svr) == false)
      {
        int typeId = simCon.RegisterPrimitive<double>(property.SimVar, property.Unit ?? DEFAULT_PROPERTY_UNIT);
        typeIdMapping[typeId] = svr;
        simVarReqMapping[svr] = new();

        simCon.RequestPrimitiveRepeatedly(typeId, out int requestId, Microsoft.FlightSimulator.SimConnect.SIMCONNECT_PERIOD.SECOND);
        requestIdMapping[requestId] = svr;
      }
      simVarReqMapping[svr].Add(property);
    }

    private static SimObject? _instance = null;
    public static SimObject GetInstance()
    {
      var tmp = _instance;
      if (tmp == null)
      {
        lock (typeof(SimObject))
        {
          if (_instance == null)
            _instance = new SimObject(new ESimConnect.ESimConnect());
          tmp = _instance;
        }
      }
      EAssert.IsNotNull(tmp);
      return tmp;
    }

    public void RegisterProperties(IEnumerable<SimProperty> simProperties)
    {
      EAssert.Argument.IsNotNull(simProperties, nameof(simProperties));
      foreach (var simProperty in simProperties)
        this.RegisterProperty(simProperty);
    }
  }
}
