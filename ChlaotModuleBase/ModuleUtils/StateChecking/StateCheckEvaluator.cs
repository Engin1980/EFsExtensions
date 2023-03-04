using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Speech.Synthesis.TtsEngine;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace ChlaotModuleBase.ModuleUtils.StateChecking
{
  public class StateCheckEvaluator : LogIdAble
  {
    private enum EPassingState
    {
      Above,
      Below
    }

    private static Random random = new();
    private Dictionary<StateCheckProperty, double> randomizedValue = new();
    private Dictionary<StateCheckProperty, double> sensitivityValue = new();
    private readonly Dictionary<StateCheckDelay, int> historyCounter = new();
    private readonly Dictionary<StateCheckProperty, EPassingState> passingPropertiesStates = new();
    //private readonly Dictionary<string, double> variables = new();
    private readonly IPlaneData planeData;
    private readonly NewLogHandler logHandler;

    public StateCheckEvaluator(IPlaneData planeData)
    {
      this.planeData = planeData;
      this.logHandler = Logger.RegisterSender(this);
      this.logHandler.Invoke(LogLevel.INFO, "Created");
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
      if (autostart == null) throw new ArgumentNullException(nameof(autostart));
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

      Log(condition, $"op (a, b, ..)", $"{condition.Operator} ({string.Join(",", subs)})", ret);
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
      Log(delay, $"current / target /= inner", $"{historyCounter[delay]} / {delay.Seconds} /= {tmp}", ret);
      return ret;
    }

    private bool EvaluateProperty(StateCheckProperty property)
    {
      double expected = property.RandomizedValue;
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

      bool ret;
      switch (property.Direction)
      {
        case StateCheckPropertyDirection.Above:
          ret = actual > expected;
          Log(property, "actual > expected", $"{actual} > {expected}", ret);
          break;
        case StateCheckPropertyDirection.Below:
          ret = actual < expected;
          Log(property, "actual < expected", $"{actual} < {expected}", ret);
          break;
        case StateCheckPropertyDirection.Exactly:
          double epsilon = property.SensitivityEpsilon;
          ret = Math.Abs(actual - expected) < epsilon;
          Log(property, "Math.Abs(actual - expected) < epsilon", $"Math.Abs({actual} - {expected}) < {epsilon}", ret);
          break;
        case StateCheckPropertyDirection.Passing:
          EPassingState nowState = actual > expected ? EPassingState.Above : EPassingState.Below;
          if (passingPropertiesStates.ContainsKey(property) == false)
          {
            ret = false;
            passingPropertiesStates[property] = nowState;
            Log(property, "//new// actual (vs) expected", $"//new// {actual} {nowState} {expected}", ret);
          }
          else
          {
            EPassingState befState = passingPropertiesStates[property];
            if (nowState == befState)
              ret = false;
            else
            {
              passingPropertiesStates[property] = nowState;
              ret = true;
            }
            Log(property, "//bef:// actual (vs) expected", $"//{befState}// {actual} {nowState} {expected}", ret);
          }
          break;
        default:
          throw new NotImplementedException($"Unknown property direction '{property.Direction}'.");
      }

      return ret;
    }

    private void Log(IStateCheckItem property, string expl, string data, bool ret)
    {
      this.logHandler.Invoke(LogLevel.INFO, $"EVAL {property.DisplayString} \t {expl} \t {data} \t {ret}");
    }

    public void Reset()
    {
      this.historyCounter.Clear();
      this.passingPropertiesStates.Clear();
    }
  }
}
