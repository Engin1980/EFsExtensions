using Eng.Chlaot.ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChlaotModuleBase.ModuleUtils.StateChecking
{
  public class StateCheckEvaluator
  {
    private enum EPassingState
    {
      Above,
      Below
    }

    private readonly Dictionary<StateCheckDelay, int> historyCounter = new();
    private readonly Dictionary<StateCheckProperty, EPassingState> passingPropertiesStates = new();
    private IPlaneData planeData;
    private LogHandler? logHandler;

    public StateCheckEvaluator(IPlaneData planeData, LogHandler? logHandler = null)
    {
      this.planeData = planeData;
      this.logHandler = logHandler;
    }

    private int ResolveEngineStarted(StateCheckProperty property)
    {
      Trace.Assert(property.Name == StateCheckPropertyName.EngineStarted);
      int index = property.NameIndex - 1;
      int ret = planeData.EngineCombustion[index] ? 1 : 0;
      return ret;
    }

    public bool Evaluate(IStateCheckItem autostart)
    {
      var ret = autostart switch
      {
        StateCheckCondition condition => EvaluateCondition(condition),
        StateCheckDelay delay => EvaluateDelay(delay),
        StateCheckProperty property => EvaluateProperty(property),
        _ => throw new NotImplementedException(),
      };
      return ret;
    }

    private bool EvaluateCondition(StateCheckCondition condition)
    {
      List<bool> subs = condition.Items.Select(q => Evaluate(q)).ToList();
      var ret = condition.Operator switch
      {
        StateCheckConditionOperator.Or => subs.Any(q => q),
        StateCheckConditionOperator.And => subs.All(q => q),
        _ => throw new NotImplementedException(),
      };

      Log($"Eval {condition.DisplayString} = {ret} (datas = {string.Join(";", subs)})");
      return ret;
    }

    private bool EvaluateDelay(StateCheckDelay delay)
    {
      bool ret;
      bool tmp = Evaluate(delay.Item);
      if (tmp)
      {
        if (historyCounter.ContainsKey(delay))
          historyCounter[delay]++;
        else
          historyCounter[delay] = 1;
      }
      else
        historyCounter[delay] = 0;

      ret = historyCounter[delay] >= delay.Seconds;

      Log($"Eval {delay.DisplayString} = {ret} (delay = {historyCounter[delay]})");

      return ret;
    }

    private bool EvaluateProperty(StateCheckProperty property)
    {
      double expected = property.GetValue();
      double actual = property.Name switch
      {
        StateCheckPropertyName.Altitude => planeData.Altitude,
        StateCheckPropertyName.IAS => planeData.IndicatedSpeed,
        StateCheckPropertyName.GS => planeData.GroundSpeed,
        StateCheckPropertyName.Height => planeData.Height,
        StateCheckPropertyName.Bank => planeData.BankAngle,
        StateCheckPropertyName.ParkingBrakeSet => planeData.ParkingBrakeSet ? 1 : 0,
        StateCheckPropertyName.VerticalSpeed => planeData.VerticalSpeed,
        StateCheckPropertyName.PushbackTugConnected => planeData.PushbackTugConnected ? 1 : 0,
        StateCheckPropertyName.Acceleration => planeData.Acceleration,
        StateCheckPropertyName.EngineStarted => ResolveEngineStarted(property),
        _ => throw new NotImplementedException()
      };

      bool ret = property.Direction switch
      {
        StateCheckPropertyDirection.Above => actual > expected,
        StateCheckPropertyDirection.Below => actual < expected,
        _ => throw new NotImplementedException()
      };

      Log($"Eval {property.DisplayString} = {ret} (actual = {actual})");
      return ret;
    }

    public void Reset()
    {
      this.historyCounter.Clear();
    }

    private void Log(string message)
    {
      logHandler?.Invoke(LogLevel.INFO, message);
    }
  }
}
