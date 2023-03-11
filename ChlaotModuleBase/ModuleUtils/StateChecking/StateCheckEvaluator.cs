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
    public class HistoryRecord
    {
      public HistoryRecord(IStateCheckItem item, bool result, string message)
      {
        Item = item ?? throw new ArgumentNullException(nameof(item));
        Result = result;
        Message = message ?? throw new ArgumentNullException(nameof(message));
      }

      public IStateCheckItem Item { get; set; }
      public bool Result { get; set; }
      public string Message { get; set; }
    }

    private enum EPassingState
    {
      Above,
      Below
    }

    private readonly Dictionary<StateCheckDelay, int> historyCounter = new();
    private readonly Dictionary<StateCheckProperty, EPassingState> passingPropertiesStates = new();
    private readonly IPlaneData planeData;
    private readonly NewLogHandler logHandler;
    private List<HistoryRecord>? evaluationHistoryContext;

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

    public bool Evaluate(IStateCheckItem item)
    {
      return this.Evaluate(item, null);
    }

    public bool Evaluate(IStateCheckItem item, List<HistoryRecord>? evaluationHistory)
    {
      logHandler.Invoke(LogLevel.INFO, $"Evaluation of {item.DisplayString} started.");
      if (item == null) throw new ArgumentNullException(nameof(item));
      bool ret;
      lock (this)
      {
        this.evaluationHistoryContext = evaluationHistory;
        ret = EvaluateItem(item);
        this.evaluationHistoryContext = null;
      }
      logHandler.Invoke(LogLevel.INFO, $"Evaluation of {item.DisplayString} resulted in {ret}.");
      return ret;
    }

    private bool EvaluateItem(IStateCheckItem item)
    {
      string msg;
      bool ret = item switch
      {
        StateCheckCondition condition => EvaluateCondition(condition, out msg),
        StateCheckDelay delay => EvaluateDelay(delay, out msg),
        StateCheckProperty property => EvaluateProperty(property, out msg),
        StateCheckTrueFalse trueFalse => EvalauteTrueFalse(trueFalse, out msg),
        _ => throw new NotImplementedException(),
      }; ;
      if (evaluationHistoryContext != null)
        evaluationHistoryContext.Add(new HistoryRecord(item, ret, msg));
      return ret;
    }

    private bool EvalauteTrueFalse(StateCheckTrueFalse trueFalse, out string message)
    {
      bool ret = trueFalse.Value;
      message = $"{trueFalse.Value}";
      Log(trueFalse, "T/F", message, ret);
      return ret;
    }

    private bool EvaluateCondition(StateCheckCondition condition, out string message)
    {
      List<bool> subs = condition.Items.Select(q => EvaluateItem(q)).ToList();
      var ret = condition.Operator switch
      {
        StateCheckConditionOperator.Or => subs.Any(q => q),
        StateCheckConditionOperator.And => subs.All(q => q),
        _ => throw new NotImplementedException(),
      };

      message = $"{condition.Operator} ({string.Join(",", subs)})";
      Log(condition, $"op (a, b, ..)", $"{condition.Operator} ({string.Join(",", subs)})", ret);
      return ret;
    }

    private bool EvaluateDelay(StateCheckDelay delay, out string message)
    {
      bool ret;
      bool tmp = EvaluateItem(delay.Item);
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

      message = $"{historyCounter[delay]} / {delay.Seconds} /= {tmp}";
      Log(delay, $"current / target /= inner", message, ret);
      return ret;
    }

    private bool EvaluateProperty(StateCheckProperty property, out string message)
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

      string expr;
      bool ret;
      switch (property.Direction)
      {
        case StateCheckPropertyDirection.Above:
          ret = actual > expected;
          expr = "actual < expected";
          message = $"{actual} < {expected}";
          break;
        case StateCheckPropertyDirection.Below:
          ret = actual < expected;
          expr = "actual < expected";
          message = $"{actual} < {expected}";
          break;
        case StateCheckPropertyDirection.Exactly:
          double epsilon = property.SensitivityEpsilon;
          ret = Math.Abs(actual - expected) < epsilon;
          expr = "Math.Abs(actual - expected) < epsilon";
          message = $"Math.Abs({actual} - {expected}) < {epsilon}";
          break;
        case StateCheckPropertyDirection.Passing:
          EPassingState nowState = actual > expected ? EPassingState.Above : EPassingState.Below;
          if (passingPropertiesStates.ContainsKey(property) == false)
          {
            ret = false;
            passingPropertiesStates[property] = nowState;
            expr = "//new// actual (vs) expected";
            message = $"//new// {actual} {nowState} {expected}";
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
            expr = "//bef:// actual (vs) expected";
            message = $"//{befState}// {actual} {nowState} {expected}";
          }
          break;
        default:
          throw new NotImplementedException($"Unknown property direction '{property.Direction}'.");
      }

      Log(property, expr, message, ret);
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
