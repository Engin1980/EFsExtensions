using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using ESimConnect;
using ESystem;
using ESystem.Exceptions;
using ESystem.Miscelaneous;
using Microsoft.Windows.Themes;
using SimVarTestModule.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static ESystem.Functions;

namespace Eng.Chlaot.Modules.SimVarTestModule
{
  public class Context : NotifyPropertyChanged
  {

    private readonly Action onReadySet;
    private ESimConnect.ESimConnect simCon = null!;
    private Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimConWrapping.SimConWrapperWithSimSecond simConWrapper = null!;
    private record SimVarId(TypeId TypeId, RequestId RequestId, SimVarCase Case);
    private readonly List<SimVarId> SimVarIds = new();

    public BindingList<SimVarCase> Cases { get; } = new();
    public List<IStringGroupItem> PredefinedSimVars { get; private set; }
    public List<IStringGroupItem> PredefinedSimEvents { get; private set; }

    public BindingList<string> AppliedSimEvents { get; } = new();


    public bool? IsEnabled
    {
      get => base.GetProperty<bool?>(nameof(IsEnabled))!;
      set
      {
        base.UpdateProperty(nameof(IsEnabled), value);
        onReadySet();
      }
    }

    public Context(Action onReadySet)
    {
      this.onReadySet = onReadySet;
      this.PredefinedSimVars = DecodePredefinedSimVarSet(typeof(SimVars));
      this.PredefinedSimEvents = DecodePredefinedSimVarSet(typeof(SimClientEvents));
    }

    private static List<IStringGroupItem> DecodePredefinedSimVarSet(Type baseType)
    {
      List<IStringGroupItem> ret = new();
      void analyseClass(Type type, List<IStringGroupItem> lst)
      {
        var nestedTypes = type.GetNestedTypes().Where(q => q.IsClass);
        foreach (var nestedType in nestedTypes)
        {
          StringGroupList sgl = new StringGroupList() { Title = nestedType.Name };
          analyseClass(nestedType, sgl.Items);
          lst.Add(sgl);
        }

        var constFields = type.GetFields().Where(q => q.IsLiteral && q.FieldType == typeof(string));
        foreach (var constField in constFields)
        {
          var val = constField.GetValue(null) ?? throw new UnexpectedNullException();
          string s = (string)val;
          lst.Add(new StringGroupValue(s));
        }
      };

      analyseClass(baseType, ret);

      return ret;
    }

    public void Connect()
    {
      simCon = new ESimConnect.ESimConnect();
      simConWrapper = new(simCon);
      simConWrapper.OpenAsync(() => { }, ex => { });

      simCon.DataReceived += SimCon_DataReceived;
      simCon.ThrowsException += SimCon_ThrowsException;
    }

    private void SimCon_ThrowsException(ESimConnect.ESimConnect sender, SimConnectException ex)
    {
      Logger.Log(this, LogLevel.ERROR, $"SimCon throws error - {ex}");
    }

    private void SimCon_DataReceived(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
    {
      SimVarId? sid = SimVarIds.FirstOrDefault(q => q.RequestId == e.RequestId);
      if (sid == null) // probably deleted one
        return;

      SimVarCase svc = sid.Case;
      svc.Value = (double)e.Data;
    }

    internal void RegisterNewSimVar(string name, bool validateName)
    {
      TypeId typeId;
      try
      {
        typeId = simCon.Values.Register<double>(name, validate: validateName);
      }
      catch (Exception ex)
      {
        Logger.Log(this, LogLevel.ERROR, $"Unable register '{name}'. {ex.Message}");
        return;
      }

      RequestId requestId = simCon.Values.RequestRepeatedly(typeId, SimConnectPeriod.SECOND, true, 0, 0, 0);

      SimVarCase svc = new()
      {
        SimVar = name,
        Value = Double.NaN
      };
      SimVarId sid = new(typeId, requestId, svc);
      SimVarIds.Add(sid);
      this.Cases.Add(svc);

    }

    internal void SetValue(SimVarCase simVarCase, double newValue)
    {
      SimVarId sid = SimVarIds.First(q => q.Case == simVarCase);
      simCon.Values.Send(sid.TypeId, newValue);
    }

    internal void DeleteSimVar(SimVarCase svc)
    {
      SimVarId sid = SimVarIds.First(q => q.Case == svc);
      SimVarIds.Remove(sid);
      this.Cases.Remove(svc);
    }

    internal void SendEvent(string eventName)
    {
      this.simCon.ClientEvents.Invoke(eventName);
    }
  }
}
