using ESystem.Logging;
using Eng.EFsExtensions.EFsExtensionsModuleBase;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects;
using Eng.EFsExtensions.Modules.SimVarTestModule.Model;
using ESimConnect;
using ESimConnect.Definitions;
using ESystem;
using ESystem.Exceptions;
using ESystem.Miscelaneous;
using Microsoft.Windows.Themes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using static ESystem.Functions;

namespace Eng.EFsExtensions.Modules.SimVarTestModule
{
  public class Context : NotifyPropertyChanged
  {
    public class Watch : NotifyPropertyChanged
    {
      public string SimVarName
      {
        get { return base.GetProperty<string>(nameof(SimVarName))!; }
        set { base.UpdateProperty(nameof(SimVarName), value); }
      }
      public double Value
      {
        get { return base.GetProperty<double>(nameof(Value))!; }
        set { base.UpdateProperty(nameof(Value), value); }
      }
    }

    private readonly Action onReadySet;
    private NewSimObject simObject = null!;
    private record SimVarId(TypeId TypeId, RequestId RequestId, SimVarCase Case);
    private readonly List<SimVarId> SimVarIds = new();

    public BindingList<SimVarCase> Cases { get; } = new();
    public List<IStringGroupItem> PredefinedSimVars { get; private set; }
    public List<IStringGroupItem> PredefinedSimEvents { get; private set; }
    public BindingList<string> AppliedSimEvents { get; } = new();
    public BindingList<Watch> Watches { get; } = new();
    private int watchesUpdaterFlag = 0;
    private readonly System.Timers.Timer watchesUpdater = new()
    {
      Interval = 500,
      AutoReset = true,
      Enabled = false
    };


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
      this.PredefinedSimEvents = DecodePredefinedSimVarSet(typeof(SimEvents.Client));
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
      }
      ;

      analyseClass(baseType, ret);

      return ret;
    }

    public void Connect()
    {
      simObject = NewSimObject.GetInstance();
      simObject.ExtOpen.OpenInBackground();

      watchesUpdater.Elapsed += (s, e) => ReadOutWatches();
      watchesUpdater.Start();

      simObject.ESimCon.DataReceived += SimCon_DataReceived;
      simObject.ESimCon.ThrowsException += SimCon_ThrowsException;
    }

    private void ReadOutWatches()
    {
      if (Interlocked.Exchange(ref watchesUpdaterFlag, 1) == 1)
        return;
      var w = simObject.ExtValue;
      var snapShot = w.GetAllValues();

      foreach (var item in snapShot)
      {
        Action a;
        int x = Watches.Count;
        var watch = Watches.FirstOrDefault(q => q.SimVarName == item.SimVarDefinition.Name);
        if (watch != null)
          a = () => { watch.Value = item.Value; };
        else
          a = () => { Watches.Add(new() { SimVarName = item.SimVarDefinition.Name, Value = item.Value }); };

        Application.Current.Dispatcher.Invoke(a);
      }
      Interlocked.Exchange(ref watchesUpdaterFlag, 0);
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
        typeId = simObject.ESimCon.Values.Register<double>(name, validate: validateName);
      }
      catch (Exception ex)
      {
        Logger.Log(this, LogLevel.ERROR, $"Unable register '{name}'. {ex.Message}");
        return;
      }

      RequestId requestId = simObject.ESimCon.Values.RequestRepeatedly(typeId, SimConnectPeriod.SECOND, true, 0, 0, 0);

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
      simObject.ESimCon.Values.Send(sid.TypeId, newValue);
    }

    internal void DeleteSimVar(SimVarCase svc)
    {
      SimVarId sid = SimVarIds.First(q => q.Case == svc);
      SimVarIds.Remove(sid);
      this.Cases.Remove(svc);
    }

    internal void SendEvent(string eventName)
    {
      this.simObject.ESimCon.ClientEvents.Invoke(eventName);
    }
  }
}
