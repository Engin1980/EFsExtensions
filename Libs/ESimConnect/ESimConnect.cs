using ESimConnect.Types;
using Microsoft.FlightSimulator.SimConnect;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;

namespace ESimConnect
{
  public class ESimConnect : IDisposable
  {
    public class ESimConnectDataReceivedEventArgs
    {
      public object Data { get; set; }
      public int? RequestId { get; set; }
      public Type Type { get; set; }

      public ESimConnectDataReceivedEventArgs(int? requestId, Type type, object data)
      {
        this.RequestId = requestId;
        this.Type = type ?? throw new ArgumentNullException(nameof(type));
        this.Data = data ?? throw new ArgumentNullException(nameof(data));
      }
    }

    public class ESimConnectEventInvokedEventArgs
    {
      public string Event { get; set; }

      public uint Value { get; set; }

      public ESimConnectEventInvokedEventArgs(string @event, uint value)
      {
        Event = @event;
        Value = value;
      }
    }

    public delegate void ESimConnectDataReceived(ESimConnect sender, ESimConnectDataReceivedEventArgs e);

    public delegate void ESimConnectDelegate(ESimConnect sender);

    public delegate void ESimConnectEventInvoked(ESimConnect sender, ESimConnectEventInvokedEventArgs e);

    public delegate void ESimConnectExceptionDelegate(ESimConnect sender, SIMCONNECT_EXCEPTION ex);

    public event ESimConnectDelegate? Connected;

    public event ESimConnectDataReceived? DataReceived;

    public event ESimConnectDelegate? Disconnected;

    public event ESimConnectEventInvoked? EventInvoked;

    public event ESimConnectExceptionDelegate? ThrowsException;

    private readonly Types.EventManager eventManager = new();
    private readonly PrimitiveManager primitiveManager = new();
    private readonly RequestDataManager requestDataManager = new();
    private readonly RequestExceptionManager requestExceptionManager = new();
    private readonly TypeManager typeManager = new();
    private readonly WinHandleManager winHandleManager = new();
    private EEnum GROUP_ID_PRIORITY_STANDARD = (EEnum)1900000000;
    private SimConnect? simConnect;

    public bool IsOpened { get => this.simConnect != null; }

    public ESimConnect()
    {
      Logger.LogMethodStart();
      winHandleManager.FsExitDetected += (() => ResolveExitedFS2020());
      Logger.LogMethodEnd();
    }

    public void Close()
    {
      Logger.LogMethodStart();
      if (this.simConnect != null)
      {
        this.simConnect.Dispose();
        this.simConnect = null;
      }
      this.winHandleManager.Dispose();
      Logger.LogMethodEnd();
    }

    public void Dispose()
    {
      Logger.LogMethodStart();
      Close();
      Logger.LogMethodEnd();
    }

    public void Open()
    {
      Logger.LogMethodStart();
      EnsureNotConnected();

      Try(
        () => winHandleManager.Acquire(),
        ex => new InternalException("Failed to register windows queue handler.", ex));

      Try(() =>
        {
          this.simConnect = new SimConnect("ESimConnect", winHandleManager.Handle, WinHandleManager.WM_USER_SIMCONNECT, null, 0);
          this.simConnect.OnRecvOpen += SimConnect_OnRecvOpen;
          this.simConnect.OnRecvQuit += SimConnect_OnRecvQuit;
          this.simConnect.OnRecvException += SimConnect_OnRecvException;
          this.simConnect.OnRecvSimobjectDataBytype += SimConnect_OnRecvSimobjectDataBytype;
          this.simConnect.OnRecvSimobjectData += SimConnect_OnRecvSimobjectData;
          this.simConnect.OnRecvEvent += SimConnect_OnRecvEvent;
          this.winHandleManager.SimConnect = this.simConnect;
        },
        ex => new InternalException("Unable to open connection to FS2020.", ex));

      Logger.LogMethodEnd();
    }

    public int RegisterPrimitive<T>(string simVarName, string unit, string simTypeName, int epsilon = 0, bool validate = false)
    {
      Logger.LogMethodStart();
      EnsureConnected();

      SIMCONNECT_DATATYPE simType = Try(
        () => Enum.Parse<SIMCONNECT_DATATYPE>(simTypeName),
        ex => new ApplicationException($"Failed to parse '{simTypeName}' as SIMCONNECT_DATATYPE enum value.", ex));

      EEnum eTypeId = IdProvider.GetNextAsEnum();

      if (validate) ValidateSimVarName(simVarName);

      Try(
        () => simConnect!.AddToDataDefinition(eTypeId, simVarName, unit, simType, epsilon, SimConnect.SIMCONNECT_UNUSED),
        ex => new InternalException("Failed to invoke 'simConnect.AddToDataDefinition(...)'.", ex));

      Try(
        () => this.simConnect!.RegisterDataDefineStruct<T>(eTypeId),
        ex => new InternalException("Failed to invoke 'simConnect.RegisterDataDefineStruct<T>(...)'.", ex));

      this.primitiveManager.Register((int)eTypeId, typeof(T));

      Logger.LogMethodEnd();
      return (int)eTypeId;
    }

    public void RegisterSystemEvent(string eventName, bool validate = false)
    {
      Logger.LogMethodStart();
      EnsureConnected();

      if (validate) ValidateSystemEventName(eventName);

      EEnum eRequestId = IdProvider.GetNextAsEnum();
      Try(() =>
        {
          this.simConnect!.SubscribeToSystemEvent(eRequestId, eventName);
          this.eventManager.Register(eRequestId, eventName);
        },
        ex => new InternalException($"Failed to register sim-event listener for '{eventName}'.", ex));
      Logger.LogMethodEnd();
    }

    public void RegisterType<T>(bool validate = false) where T : struct
    {
      Logger.LogMethodStart();
      EnsureConnected();

      EEnum eTypeId = IdProvider.GetNextAsEnum();
      int epsilon = 0;

      Type t = typeof(T);
      List<SanityHelpers.FieldMapInfo> fieldInfos = SanityHelpers.CheckAndDecodeFieldMappings(t);

      foreach (var fieldInfo in fieldInfos)
      {
        if (validate) ValidateSimVarName(fieldInfo.Name);
        Try(() =>
          this.simConnect!.AddToDataDefinition(eTypeId,
            fieldInfo.Name, fieldInfo.Unit, fieldInfo.Type,
            epsilon, SimConnect.SIMCONNECT_UNUSED),
          ex => new InternalException("Failed to invoke 'simConnect.AddToDataDefinition(...)'.", ex));
      }

      Try(() => this.simConnect!.RegisterDataDefineStruct<T>(eTypeId),
        ex => new InternalException("Failed to invoke 'simConnect.RegisterDataDefineStruct<T>(...)'.", ex));
      this.typeManager.Register((int)eTypeId, t);
      Logger.LogMethodEnd();
    }

    public void RequestData<T>(out int requestId)
    {
      uint radius = 0;
      requestId = IdProvider.GetNext();
      RequestData<T>(requestId, radius, SIMCONNECT_SIMOBJECT_TYPE.USER);
    }

    public void RequestData<T>(out int requestId, uint radius, SIMCONNECT_SIMOBJECT_TYPE simObjectType)
    {
      requestId = IdProvider.GetNext();
      RequestData<T>(requestId, radius, simObjectType);
    }

    public void RequestData<T>(int? customRequestId = null)
      => RequestData<T>(customRequestId, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);

    public void RequestData<T>(int? customRequestId, uint radius, SIMCONNECT_SIMOBJECT_TYPE simObjectType)
    {
      Logger.LogMethodStart(new object?[] { customRequestId, radius, simObjectType });
      EnsureConnected();

      EEnum eTypeId = typeManager.GetIdAsEnum(typeof(T));
      EEnum eRequestId = IdProvider.GetNextAsEnum();
      Try(() => this.simConnect!.RequestDataOnSimObjectType(eRequestId, eTypeId, radius, simObjectType),
        ex => throw new InternalException("Failed to invoke 'RequestDataOnSimObjectType(...)'.", ex));
      requestDataManager.Register(customRequestId, typeof(T), eRequestId);
      Logger.LogMethodEnd();
    }

    public void RequestDataRepeatedly<T>(out int requestId, SIMCONNECT_PERIOD period, bool sendOnlyOnChange = true,
      int initialDelayFrames = 0, int skipBetweenFrames = 0, int numberOfReturnedFrames = 0)
    {
      requestId = IdProvider.GetNext();
      RequestDataRepeatedly<T>(requestId, period, sendOnlyOnChange,
        initialDelayFrames, skipBetweenFrames, numberOfReturnedFrames);
    }

    public void RequestDataRepeatedly<T>(
      int? customRequestId, SIMCONNECT_PERIOD period, bool sendOnlyOnChange = true,
      int initialDelayFrames = 0, int skipBetweenFrames = 0, int numberOfReturnedFrames = 0)
    {
      Logger.LogMethodStart(new object?[] {
        customRequestId, period, sendOnlyOnChange, initialDelayFrames,
        skipBetweenFrames, numberOfReturnedFrames });
      if (this.simConnect == null) throw new NotConnectedException();
      if (initialDelayFrames < 0) initialDelayFrames = 0;
      if (skipBetweenFrames < 0) skipBetweenFrames = 0;
      if (numberOfReturnedFrames < 0) numberOfReturnedFrames = 0;

      SIMCONNECT_DATA_REQUEST_FLAG flag = sendOnlyOnChange
        ? SIMCONNECT_DATA_REQUEST_FLAG.CHANGED
        : SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT;

      EEnum eTypeId = typeManager.GetIdAsEnum(typeof(T));
      EEnum eRequestId = IdProvider.GetNextAsEnum();

      Try(() =>
        this.simConnect.RequestDataOnSimObject(
          eRequestId, eTypeId, SimConnect.SIMCONNECT_OBJECT_ID_USER, period,
          flag, (uint)initialDelayFrames, (uint)skipBetweenFrames, (uint)numberOfReturnedFrames),
        ex => new InternalException($"Failed to invoke 'RequestDataOnSimObject(...)'.", ex));
      this.requestDataManager.Register(customRequestId, typeof(T), eRequestId);
      Logger.LogMethodEnd();
    }

    public void RequestPrimitiveRepeatedly(int typeId, out int requestId, SIMCONNECT_PERIOD period, bool sendOnlyOnChange = true,
      int initialDelayFrames = 0, int skipBetweenFrames = 0, int numberOfReturnedFrames = 0)
    {
      requestId = IdProvider.GetNext();
      RequestPrimitiveRepeatedly(typeId, requestId, period, sendOnlyOnChange, initialDelayFrames, skipBetweenFrames, numberOfReturnedFrames);
    }

    public void RequestPrimitiveRepeatedly(int typeId, int customRequestId, SIMCONNECT_PERIOD period, bool sendOnlyOnChange = true,
      int initialDelayFrames = 0, int skipBetweenFrames = 0, int numberOfReturnedFrames = 0)
    {
      Logger.LogMethodStart(new object?[] {
        customRequestId, period, sendOnlyOnChange, initialDelayFrames,
        skipBetweenFrames, numberOfReturnedFrames });
      if (this.simConnect == null) throw new NotConnectedException();
      if (initialDelayFrames < 0) initialDelayFrames = 0;
      if (skipBetweenFrames < 0) skipBetweenFrames = 0;
      if (numberOfReturnedFrames < 0) numberOfReturnedFrames = 0;

      SIMCONNECT_DATA_REQUEST_FLAG flag = sendOnlyOnChange
        ? SIMCONNECT_DATA_REQUEST_FLAG.CHANGED
        : SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT;

      EnsurePrimitiveTypeIdExists(typeId);
      EEnum eTypeId = (EEnum)typeId;
      Type type = this.primitiveManager.GetType(typeId);
      EEnum eRequestId = IdProvider.GetNextAsEnum();

      Try(() =>
        this.simConnect.RequestDataOnSimObject(
          eRequestId, eTypeId, SimConnect.SIMCONNECT_OBJECT_ID_USER, period,
          flag, (uint)initialDelayFrames, (uint)skipBetweenFrames, (uint)numberOfReturnedFrames),
        ex => new InternalException($"Failed to invoke 'RequestDataOnSimObject(...)'.", ex));
      this.requestDataManager.Register(customRequestId, type, eRequestId);
      Logger.LogMethodEnd();
    }

    private void EnsurePrimitiveTypeIdExists(int typeId)
    {
      if (!this.primitiveManager.IsRegistered(typeId))
        throw new Exception($"Primitive typeId={typeId} is not registered.");
    }

    public void RequestPrimitive(int typeId, out int requestId)
      => RequestPrimitive(typeId, 0, SIMCONNECT_SIMOBJECT_TYPE.USER, out requestId);

    public void RequestPrimitive(int typeId, uint radius, SIMCONNECT_SIMOBJECT_TYPE simObjectType, out int requestId)
    {
      requestId = IdProvider.GetNext();
      RequestPrimitive(typeId, requestId, radius, simObjectType);
    }

    public int RequestPrimitive(int typeId, int customRequestId)
      => RequestPrimitive(typeId, customRequestId, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);

    public int RequestPrimitive(int typeId, int customRequestId, uint radius, SIMCONNECT_SIMOBJECT_TYPE simObjectType)
    {
      Logger.LogMethodStart(new object?[] { typeId, customRequestId, radius });
      EnsureConnected();

      EnsurePrimitiveTypeIdExists(typeId);
      EEnum eTypeId = (EEnum)typeId;
      Type t = primitiveManager.GetType(typeId);
      EEnum eRequestId = IdProvider.GetNextAsEnum();
      this.simConnect!.RequestDataOnSimObjectType(eRequestId, eTypeId, radius, simObjectType);
      requestDataManager.Register(customRequestId, t, eRequestId);
      Logger.LogMethodEnd();
      return (int)customRequestId;
    }

    public void SendClientEvent(string eventName, uint[]? parameters = null, bool validate = false)
    {
      Logger.LogMethodStart();
      EnsureConnected();

      parameters ??= Array.Empty<uint>();

      // up to 5 parameters available, but probably with a different .ddl version
      if (parameters.Length > 1) throw new NotImplementedException($"Maximum expected number of parameters is {1} (provided {parameters.Length}).");

      if (validate) ValidateClientEvent(eventName, parameters);

      EEnum eEvent = IdProvider.GetNextAsEnum();
      this.simConnect!.MapClientEventToSimEvent(eEvent, eventName);

      uint val = parameters.Length == 0 ? 0 : parameters[0];
      Try(() =>
        this.simConnect.TransmitClientEvent(
        SimConnect.SIMCONNECT_OBJECT_ID_USER, eEvent, val, GROUP_ID_PRIORITY_STANDARD, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY),
        ex => new InternalException($"Failed to invoke 'TransmitClientEvent(...)'", ex));
    }

    public void SendPrimitive<T>(int typeId, T value)
    {
      Logger.LogMethodStart();
      EnsureConnected();
      if (value == null) throw new ArgumentNullException(nameof(value));

      if (!this.primitiveManager.IsRegistered(typeId))
        throw new ApplicationException($"Primitive type with id {typeId} not registered.");
      Type expectedType = this.primitiveManager.GetType(typeId);
      if (value.GetType().Equals(expectedType) == false)
        throw new ApplicationException($"Primitive type should be {expectedType.Name}, but provided value {value} is {value.GetType().Name}.");

      EEnum eTypeId = (EEnum)typeId;
      this.simConnect!.SetDataOnSimObject(eTypeId, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, value);

      Logger.LogMethodEnd();
    }

    public void UnregisterPrimitive(int typeId)
    {
      Logger.LogMethodStart();
      EnsureConnected();

      EEnum eTypeId = (EEnum)typeId;

      Try(() => this.simConnect!.ClearDataDefinition(eTypeId),
        ex => new InternalException($"Failed to unregister typeId {typeId}.", ex));
      this.primitiveManager.Unregister(typeId);
      Logger.LogMethodEnd();
    }

    public void UnregisterType<T>()
    {
      Logger.LogMethodStart();
      EnsureConnected();

      EEnum eTypeId = typeManager.GetIdAsEnum(typeof(T));

      Try(
        () => this.simConnect!.ClearDataDefinition(eTypeId),
        ex => new InternalException($"Failed to unregister type {typeof(T).Name}.", ex));
      this.typeManager.Unregister(typeof(T));
      Logger.LogMethodEnd();
    }

    private static void ValidateClientEvent(string eventName, uint[] parameters)
    {
      FieldInfo? extractEventField(string eventName, Type? cls = null)
      {
        FieldInfo? ret;
        if (cls == null) cls = typeof(SimClientEvents);

        ret = cls.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
          .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
          .FirstOrDefault(q => q.Name == eventName);

        if (ret == null)
        {
          var classes = cls.GetNestedTypes();
          foreach (var c in classes)
          {
            ret = extractEventField(eventName, c);
            if (ret != null) break;
          }
        }
        return ret;
      };

      FieldInfo? eventField = extractEventField(eventName) ?? throw new Exception($"ClientEvent '{eventName}' not found in declarations.");

      var paramAttrs = eventField.GetCustomAttributes().Where(q => q is SimClientEvents.Parameter).Cast<SimClientEvents.Parameter>();
      if (paramAttrs.Count() != parameters.Length)
      {
        throw new Exception($"ClientEvent '{eventName}' parameter check failed. Expected {paramAttrs.Count()} params, provided {parameters.Length}.");
      }
    }

    private void ValidateSimVarName(string simVarName)
    {
      string unindexSimVarName(string simVarName)
      {
        string ret;
        int index = simVarName.IndexOf(':');
        ret = index < 0 ? simVarName : simVarName.Substring(0, index + 1);
        return ret;
      }
      bool findSimVar(string simVarName, Type? cls = null)
      {
        bool ret;
        if (cls == null) cls = typeof(SimVars);

        ret = cls.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
          .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
          .Select(q => q.GetValue(null))
          .Any(q => Equals(q, simVarName));
        if (!ret)
        {
          var classes = cls.GetNestedTypes();
          foreach (var c in classes)
          {
            ret = findSimVar(simVarName, c);
            if (ret) break;
          }
        }

        return ret;
      };

      string tmp = unindexSimVarName(simVarName);
      bool exists = findSimVar(tmp);
      if (!exists)
      {
        throw new Exception($"SimVar '{simVarName}' check failed. SimVar not found in known values.");
      }
    }

    private void ValidateSystemEventName(string eventName)
    {
      bool findEvent(string simVarName, Type? cls = null)
      {
        bool ret;
        if (cls == null) cls = typeof(SimEvents);

        ret = cls.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
          .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
          .Select(q => q.GetValue(null))
          .Any(q => Equals(q, simVarName));
        if (!ret)
        {
          var classes = cls.GetNestedTypes();
          foreach (var c in classes)
          {
            ret = findEvent(simVarName, c);
            if (ret) break;
          }
        }

        return ret;
      };

      bool exists = findEvent(eventName);
      if (!exists)
      {
        throw new Exception($"SystemEvent '{eventName}' check failed. SystemEvent name not found in known values.");
      }
    }

    private void EnsureConnected()
    {
      if (this.simConnect == null) throw new NotConnectedException();
    }

    private void EnsureNotConnected()
    {
      if (this.simConnect != null) throw new InvalidRequestException("SimConnect already opened.");
    }

    private void ResolveExitedFS2020()
    {
      if (this.simConnect != null)
      {
        this.simConnect.Dispose();
        this.simConnect = null;
      }
      this.Disconnected?.Invoke(this);
    }

    private void SimConnect_OnRecvEvent(SimConnect sender, SIMCONNECT_RECV_EVENT data)
    {
      Logger.LogInvokedEvent(this, nameof(SimConnect_OnRecvEvent), data);
      EEnum iRequest = (EEnum)data.uEventID;
      string @event = eventManager.GetEvent(iRequest);
      uint value = data.dwData;

      ESimConnectEventInvokedEventArgs e = new(@event, value);
      this.EventInvoked?.Invoke(this, e);
    }

    private void SimConnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
    {
      Logger.LogInvokedEvent(this, nameof(SimConnect_OnRecvException), data);
      SIMCONNECT_EXCEPTION ex = (SIMCONNECT_EXCEPTION)data.dwException;
      ThrowsException?.Invoke(this, ex);
    }

    private void SimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
    {
      Logger.LogInvokedEvent(this, nameof(SimConnect_OnRecvOpen), data);
      this.Connected?.Invoke(this);
    }

    private void SimConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
    {
      Logger.LogInvokedEvent(this, nameof(SimConnect_OnRecvQuit), data);
      this.Disconnected?.Invoke(this);
    }

    private void SimConnect_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
    {
      Logger.LogInvokedEvent(this, nameof(SimConnect_OnRecvSimobjectData), data);
      EEnum iRequest = (EEnum)data.dwRequestID;
      object ret = data.dwData[0];
      requestDataManager.Recall(iRequest, out Type type, out int? userRequestId);
      ESimConnectDataReceivedEventArgs e = new(userRequestId, type, ret);
      this.DataReceived?.Invoke(this, e);
    }

    private void SimConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
    {
      Logger.LogInvokedEvent(this, nameof(SimConnect_OnRecvSimobjectDataBytype), data);
      EEnum iRequest = (EEnum)data.dwRequestID;
      requestDataManager.Recall(iRequest, out Type type, out int? userRequestId);
      object ret = data.dwData[0];

      ESimConnectDataReceivedEventArgs e = new(userRequestId, type, ret);
      this.DataReceived?.Invoke(this, e);
    }

    private void Try(Action tryAction, Func<Exception, Exception> exceptionProducer)
    {
      try
      {
        tryAction.Invoke();
      }
      catch (Exception ex)
      {
        Exception newException = exceptionProducer.Invoke(ex);
        Logger.LogException(newException);
        throw newException;
      }
    }

    private T Try<T>(Func<T> tryFunc, Func<Exception, Exception> exceptionProducer)
    {
      T ret;
      try
      {
        ret = tryFunc.Invoke();
      }
      catch (Exception ex)
      {
        Exception newException = exceptionProducer.Invoke(ex);
        Logger.LogException(newException);
        throw newException;
      }
      return ret;
    }
  }
}
