using ESimConnect.Types;
using Microsoft.FlightSimulator.SimConnect;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
      public ESimConnectEventInvokedEventArgs(string @event, uint value)
      {
        Event = @event;
        Value = value;
      }

      public string Event { get; set; }
      public uint Value { get; set; }

    }

    public delegate void ESimConnectDataReceived(ESimConnect sender, ESimConnectDataReceivedEventArgs e);
    public delegate void ESimConnectEventInvoked(ESimConnect sender, ESimConnectEventInvokedEventArgs e);
    public delegate void ESimConnectDelegate(ESimConnect sender);
    public delegate void ESimConnectExceptionDelegate(ESimConnect sender, SIMCONNECT_EXCEPTION ex);

    public event ESimConnectDelegate? Connected;
    public event ESimConnectDataReceived? DataReceived;
    public event ESimConnectEventInvoked? EventInvoked;
    public event ESimConnectDelegate? Disconnected;
    public event ESimConnectExceptionDelegate? ThrowsException;

    private readonly TypeManager typeManager = new();
    private readonly Types.EventManager eventManager = new();
    private readonly WinHandleManager winHandleManager = new();
    private readonly RequestManager requestIdManager = new();
    private SimConnect? simConnect;

    public bool IsOpened { get => this.simConnect != null; }

    public ESimConnect()
    {
      Logger.LogMethodStart();
      winHandleManager.FsExitDetected += (() => ResolveExitedFS2020());
      Logger.LogMethodEnd();
    }

    public static void SetLogHandler(Action<string> logHandler)
    {
      Logger.LogHandler = logHandler;
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
      if (this.simConnect != null)
        throw new InvalidRequestException("SimConnect already opened.");

      try
      {
        winHandleManager.Acquire();
      }
      catch (Exception ex)
      {
        var tmp = new InternalException("Failed to register windows queue handler.", ex);
        Logger.LogException(tmp);
        throw tmp;
      }

      try
      {
        this.simConnect = new SimConnect("ESimConnect", winHandleManager.Handle, WinHandleManager.WM_USER_SIMCONNECT, null, 0);
        this.simConnect.OnRecvOpen += SimConnect_OnRecvOpen;
        this.simConnect.OnRecvQuit += SimConnect_OnRecvQuit;
        this.simConnect.OnRecvException += SimConnect_OnRecvException;
        this.simConnect.OnRecvSimobjectDataBytype += SimConnect_OnRecvSimobjectDataBytype;
        this.simConnect.OnRecvSimobjectData += SimConnect_OnRecvSimobjectData;
        this.simConnect.OnRecvEvent += SimConnect_OnRecvEvent;
        this.winHandleManager.Acquire();
        this.winHandleManager.SimConnect = this.simConnect;
      }
      catch (Exception ex)
      {
        var tmp = new InternalException("Unable to open connection to FS2020.", ex);
        Logger.LogException(tmp);
        throw tmp;
      }
      Logger.LogMethodEnd();
    }

    public void RegisterSystemEvent(string eventName)
    {
      Logger.LogMethodStart();
      if (this.simConnect == null) throw new NotConnectedException();

      EEnum eRequestId = IdProvider.GetNextAsEnum();
      try
      {
        this.simConnect.SubscribeToSystemEvent(eRequestId, eventName);
        this.eventManager.Register(eRequestId, eventName);
      }
      catch (Exception ex)
      {
        var tmp = new InternalException($"Failed to register sim-event listener for '{eventName}'.", ex);
        Logger.LogException(tmp);
        throw tmp;
      }
      Logger.LogMethodEnd();
    }

    public void RegisterType<T>() where T : struct
    {
      Logger.LogMethodStart();
      if (this.simConnect == null) throw new NotConnectedException();

      EEnum eTypeId = IdProvider.GetNextAsEnum();
      int epsilon = 0;

      Type t = typeof(T);
      List<SanityHelpers.FieldMapInfo> fieldInfos = SanityHelpers.CheckAndDecodeFieldMappings(t);

      foreach (var fieldInfo in fieldInfos)
      {
        try
        {
          simConnect.AddToDataDefinition(eTypeId,
            fieldInfo.Name, fieldInfo.Unit, fieldInfo.Type,
            epsilon, SimConnect.SIMCONNECT_UNUSED);
        }
        catch (Exception ex)
        {
          var tmp = new InternalException("Failed to invoke 'simConnect.AddToDataDefinition(...)'.", ex);
          Logger.LogException(tmp);
          throw tmp;
        }
      }
      try
      {
        this.simConnect.RegisterDataDefineStruct<T>(eTypeId);
        this.typeManager.Register((int)eTypeId, t);
      }
      catch (Exception ex)
      {
        var tmp = new InternalException("Failed to invoke 'simConnect.RegisterDataDefineStruct<T>(...)'.", ex);
        Logger.LogException(tmp);
        throw tmp;
      }
      Logger.LogMethodEnd();
    }

    public void RequestData<T>(
          int? customRequestId = null, uint radius = 0,
      SIMCONNECT_SIMOBJECT_TYPE simObjectType = SIMCONNECT_SIMOBJECT_TYPE.USER)
    {
      Logger.LogMethodStart(new object?[] { customRequestId, radius, simObjectType });
      if (this.simConnect == null) throw new NotConnectedException();

      EEnum eTypeId = typeManager.GetIdAsEnum(typeof(T));
      EEnum eRequestId = IdProvider.GetNextAsEnum();
      this.simConnect.RequestDataOnSimObjectType(eRequestId, eTypeId, radius, simObjectType);
      requestIdManager.Register(customRequestId, typeof(T), eRequestId);
      Logger.LogMethodEnd();
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

      try
      {
        this.simConnect.RequestDataOnSimObject(eRequestId, eTypeId, SimConnect.SIMCONNECT_OBJECT_ID_USER, period,
          flag, (uint)initialDelayFrames, (uint)skipBetweenFrames, (uint)numberOfReturnedFrames);
        this.requestIdManager.Register(customRequestId, typeof(T), eRequestId);
      }
      catch (Exception ex)
      {
        var tmp = new InternalException($"Failed to invoke 'RequestDataOnSimObject(...)'.", ex);
        Logger.LogException(tmp);
        throw tmp;
      }
      Logger.LogMethodEnd();
    }

    public void UnregisterType<T>()
    {
      Logger.LogMethodStart();
      if (this.simConnect == null) throw new NotConnectedException();

      EEnum eTypeId = typeManager.GetIdAsEnum(typeof(T));

      try
      {
        this.simConnect.ClearDataDefinition(eTypeId);
        this.typeManager.Unregister(typeof(T));
      }
      catch (Exception ex)
      {
        throw new InternalException($"Failed to unregister type {typeof(T).Name}.", ex);
      }
      Logger.LogMethodEnd();
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
      requestIdManager.Recall(iRequest, out Type type, out int? userRequestId);
      ESimConnectDataReceivedEventArgs e = new(userRequestId, type, ret);
      this.DataReceived?.Invoke(this, e);
    }
    private void SimConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
    {
      Logger.LogInvokedEvent(this, nameof(SimConnect_OnRecvSimobjectDataBytype), data);
      EEnum iRequest = (EEnum)data.dwRequestID;
      requestIdManager.Recall(iRequest, out Type type, out int? userRequestId);
      object ret = data.dwData[0];

      ESimConnectDataReceivedEventArgs e = new(userRequestId, type, ret);
      this.DataReceived?.Invoke(this, e);
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
  }
}
