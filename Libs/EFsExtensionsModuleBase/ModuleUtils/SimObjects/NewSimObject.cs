using ESimConnect;
using ESystem;
using ESystem.Asserting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects
{
  public class NewSimObject
  {
    private record TypeIdSimProperty(TypeId TypeId, SimProperty SimProperty);

    private static readonly NewSimObject instance;
    static NewSimObject()
    {
      instance = new NewSimObject();
    }

    public static NewSimObject GetInstance() => instance;

    private readonly ESimConnect.ESimConnect eSimCon;
    private readonly ESimConnect.Extenders.OpenInBackgroundExtender extOpen;
    private readonly ESimConnect.Extenders.SimTimeExtender extTime;
    private readonly ESimConnect.Extenders.ValueCacheExtender extValue;
    private readonly ESimConnect.Extenders.TypeCacheExtender extType;
    private readonly ConcurrentBag<TypeIdSimProperty> registerdSimProperties = new();

    public event Action? Started;
    public event Action? SimSecondElapsed;
    public event Action<SimProperty, double>? SimPropertyChanged;
    public ESimConnect.ESimConnect ESimCon => eSimCon;
    public ESimConnect.Extenders.ValueCacheExtender ExtValue => extValue;
    public ESimConnect.Extenders.TypeCacheExtender ExtType => extType;
    public ESimConnect.Extenders.SimTimeExtender ExtTime => extTime;
    public ESimConnect.Extenders.OpenInBackgroundExtender ExtOpen => extOpen;

    public NewSimObject()
    {
      eSimCon = new ESimConnect.ESimConnect();
      extOpen = new ESimConnect.Extenders.OpenInBackgroundExtender(eSimCon);
      extTime = new ESimConnect.Extenders.SimTimeExtender(eSimCon, false);
      extValue = new ESimConnect.Extenders.ValueCacheExtender(eSimCon);
      extType = new ESimConnect.Extenders.TypeCacheExtender(extValue);

      extOpen.Opened += () => this.Started?.Invoke();
      extTime.SimSecondElapsed += () => this.SimSecondElapsed?.Invoke();
      extValue.ValueChanged += ExtValue_ValueChanged;
    }

    private void ExtValue_ValueChanged(ESimConnect.Extenders.ValueCacheExtender.ValueChangeEventArgs e)
    {
      if (this.SimPropertyChanged == null) return;

      registerdSimProperties
        .Where(q => q.TypeId == e.TypeId)
        .ForEach(q => this.SimPropertyChanged?.Invoke(q.SimProperty, e.Value));
    }

    public bool IsSimPaused => this.extTime.IsSimPaused;
    public bool IsOpened => this.extOpen.IsOpened;

    [Obsolete("Used ..ExtOpen.StartInBackground() instead.")]
    public void StartInBackground(Action? onStarted = null)
    {
      this.extOpen.OpenInBackground(onStarted);
    }

    public void RegisterProperties(IEnumerable<SimProperty> simProperties)
    {
      EAssert.Argument.IsNotNull(simProperties, nameof(simProperties));
      foreach (var simProperty in simProperties)
        this.RegisterProperty(simProperty);
    }

    public void RegisterProperty(SimProperty property)
    {
      EAssert.Argument.IsNotNull(property, nameof(property));
      EAssert.IsTrue(this.extOpen.IsOpened, "SimObject must be started first.");
      var typeId = this.extValue.Register(property.SimVar, property.Unit ?? "Number");
      lock (this.registerdSimProperties)
      {
        if (this.registerdSimProperties.None(q => q.SimProperty == property))
          this.registerdSimProperties.Add(new(typeId, property));
      }
    }
  }
}
