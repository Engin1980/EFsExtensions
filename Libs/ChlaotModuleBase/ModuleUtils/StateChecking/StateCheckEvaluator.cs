using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.Exceptions;
using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using ESystem.Asserting;
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
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.StateModel;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking
{
  public class StateCheckEvaluator : LogIdAble
  {
    #region Public Classes

    public class HistoryRecord
    {
      #region Public Properties

      public IStateCheckItem Item { get; set; }

      public string Message { get; set; }

      public bool Result { get; set; }

      #endregion Public Properties

      #region Public Constructors

      public HistoryRecord(IStateCheckItem item, bool result, string message)
      {
        Item = item ?? throw new ArgumentNullException(nameof(item));
        Result = result;
        Message = message ?? throw new ArgumentNullException(nameof(message));
      }

      #endregion Public Constructors
    }

    #endregion Public Classes

    #region Private Enums

    private enum EPassingState
    {
      Above,
      Below
    }

    #endregion Private Enums

    #region Private Fields

    private static Random random = new();
    private readonly Dictionary<StateCheckProperty, double> extractedValues = new();
    private readonly Dictionary<StateCheckDelay, int> historyCounter = new();
    private readonly NewLogHandler logHandler;
    private readonly Dictionary<StateCheckProperty, EPassingState> passingPropertiesStates = new();
    private readonly Dictionary<string, double> propertyValues;
    private readonly Dictionary<string, double> variableValues;
    private List<HistoryRecord>? evaluationHistoryContext;

    #endregion Private Fields

    #region Public Constructors

    public StateCheckEvaluator(Dictionary<string, double> variableValues, Dictionary<string, double> propertyValues)
    {
      EAssert.Argument.IsNotNull(variableValues, nameof(variableValues));
      EAssert.Argument.IsNotNull(propertyValues, nameof(propertyValues));

      this.variableValues = variableValues;
      this.propertyValues = propertyValues;

      this.logHandler = Logger.RegisterSender(this);
      this.logHandler.Invoke(LogLevel.INFO, "Created");
    }

    #endregion Public Constructors

    #region Public Methods

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

    public void Reset()
    {
      this.historyCounter.Clear();
      this.passingPropertiesStates.Clear();
    }

    #endregion Public Methods

    #region Private Methods

    private double ApplyPropertyRandomness(StateCheckProperty property, double value)
    {
      var randomness = property.Randomness;
      double absUpper = randomness.Above.GetValue(value);
      double absLower = randomness.Below.GetValue(value);
      double ret = random.NextDouble(absLower, absUpper);
      return ret;
    }

    private bool EvalauteTrueFalse(StateCheckTrueFalse trueFalse, out string message)
    {
      bool ret = trueFalse.Value;
      message = $"T/F => {trueFalse.Value}";
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

      message = $"op(a,b,..) => {condition.Operator} ({string.Join(",", subs)})";
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

      message = $"curr_sec/trg_sec/inner => {historyCounter[delay]} / {delay.Seconds} /= {tmp}";
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
      };

      Log(item, msg, ret);
      evaluationHistoryContext?.Add(new HistoryRecord(item, ret, msg));
      return ret;
    }
    private bool EvaluateProperty(StateCheckProperty property, out string message)
    {
      double expected = ExtractExpectedPropertyValue(property, true);
      double actual = ResolveRealPropertyValue(property.Name);

      bool ret;
      switch (property.Direction)
      {
        case StateCheckPropertyDirection.Above:
          ret = actual > expected;
          message = $"act > exp => {actual:N2} > {expected:N2}";
          break;
        case StateCheckPropertyDirection.Below:
          ret = actual < expected;
          message = $"act < exp => {actual:N2} < {expected:N2}";
          break;
        case StateCheckPropertyDirection.Exactly:
          double min, max;
          (min, max) = ExtractPropertySensitivity(property, expected);
          ret = min <= actual && actual <= max;
          message = $"expMin<=act<=expMax => {min:N2} <= {actual:N2} <= {max:N2}";
          break;
        case StateCheckPropertyDirection.Passing:
        case StateCheckPropertyDirection.PassingDown:
        case StateCheckPropertyDirection.PassingUp:
          EPassingState nowState = actual > expected ? EPassingState.Above : EPassingState.Below;
          if (passingPropertiesStates.ContainsKey(property) == false)
          {
            ret = false;
            passingPropertiesStates[property] = nowState;
            message = $"mode={property.Direction} // state=new // act state exp => {actual:N2} {nowState} {expected:N2}";
          }
          else
          {
            EPassingState befState = passingPropertiesStates[property];
            if (property.Direction == StateCheckPropertyDirection.PassingDown)
            {
              ret = nowState == EPassingState.Below && befState == EPassingState.Above;
            }
            else if (property.Direction == StateCheckPropertyDirection.PassingUp)
            {
              ret = nowState == EPassingState.Above && befState == EPassingState.Below;
            }
            else
            {
              ret = nowState != befState; // direction is just "passing", unequality means change
            }
            passingPropertiesStates[property] = nowState;

            message = $"mode={property.Direction} // state={befState} // act state exp => {actual:N2} {nowState} {expected:N2}";
          }
          break;
        default:
          throw new NotImplementedException($"Unknown property direction '{property.Direction}'.");
      }

      return ret;
    }

    private double ExtractExpectedPropertyValue(StateCheckProperty property, bool applyRandomness)
    {
      double ret;
      if (extractedValues.ContainsKey(property))
        ret = extractedValues[property];
      else
      {
        if (property.IsVariableBased == false)
          ret = property.GetExpressionAsDouble();
        else
        {
          var variableName = property.GetExpressionAsVariableName();
          if (!this.variableValues.TryGetValue(variableName, out ret))
            throw new StateCheckException($"Unable resolve value of variable '{variableName}'.");
        }
        if (applyRandomness) ret = ApplyPropertyRandomness(property, ret);
        extractedValues[property] = ret;
      }
      return ret;
    }

    private (double, double) ExtractPropertySensitivity(StateCheckProperty property, double value)
    {
      double max = property.Sensitivity.Above.GetValue(value);
      double min = property.Sensitivity.Below.GetValue(value);
      return (min, max);
    }
    private void Log(IStateCheckItem property, string msg, bool ret)
    {
      this.logHandler.Invoke(LogLevel.INFO, $"EVAL {property.DisplayString} \t {msg} \t {ret}");
    }

    private double ResolveRealPropertyValue(string propertyName)
    {
      if (propertyValues.ContainsKey(propertyName) == false)
        throw new ApplicationException($"Property {propertyName} not found in property-value dictionary.");
      double ret = propertyValues[propertyName];
      return ret;
    }

    #endregion Private Methods

    public static void UpdateDictionaryByObject(object source, Dictionary<string, double> target)
    {
      var props = source.GetType().GetProperties().Where(q=>q.PropertyType == typeof(double));
      foreach (var prop in props)
      {
        object? obj = prop.GetValue(source, null);
        if (obj is double d)
        {
          target[prop.Name] = d;
        }
      }
    }
  }
}
