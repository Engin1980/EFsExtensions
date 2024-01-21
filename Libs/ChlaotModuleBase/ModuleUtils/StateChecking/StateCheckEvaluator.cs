using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.Exceptions;
using ELogging;
using ESystem.Asserting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.StateModel;
using System.Reflection;
using ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimObjects;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.Interfaces;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking
{
  public class StateCheckEvaluator
  {
    public record RecentResult(IStateCheckItem StateCheckItem, bool Result, string Note);

    #region Private Enums

    private enum EPassingState
    {
      Above,
      Below
    }

    #endregion Private Enums

    #region Private Fields

    private readonly static Random random = new();
    private static Type[] updatedTypesNumerical =
    {
      typeof(int), typeof(double), typeof(bool)
    };

    private readonly Dictionary<StateCheckDelay, int> delayCounter = new();
    private readonly Dictionary<StateCheckProperty, double> extractedValues = new();
    private readonly Logger logger;
    private readonly Dictionary<StateCheckProperty, EPassingState> passingPropertiesStates = new();
    private readonly IPropertyValuesProvider propertyValuesProvider;
    private readonly IVariableValuesProvider variableValuesProvider;
    private Dictionary<string, double>? currentPropertyValues = null;
    private Dictionary<string, double>? currentVariableValues = null;
    private readonly List<RecentResult> recentResultSet = new();

    #endregion Private Fields

    #region Public Constructors

    public StateCheckEvaluator(
       IVariableValuesProvider variableValuesProvider,
      IPropertyValuesProvider propertyValuesProvider)
    {
      EAssert.Argument.IsNotNull(variableValuesProvider, nameof(variableValuesProvider));
      EAssert.Argument.IsNotNull(propertyValuesProvider, nameof(propertyValuesProvider));

      this.variableValuesProvider = variableValuesProvider;
      this.propertyValuesProvider = propertyValuesProvider;

      this.logger = Logger.Create(this);
      this.logger.Invoke(LogLevel.INFO, "Created");
    }

    #endregion Public Constructors

    #region Public Methods
    public List<RecentResult> GetRecentResultSet()
    {
      List<RecentResult> ret;
      lock (this)
      {
        ret = recentResultSet.ToList();
      }
      return ret;
    }

    public static void UpdateDictionaryByObject(object source, Dictionary<string, double> target)
    {
      var props = source.GetType().GetProperties();
      foreach (var prop in props)
      {
        var propType = prop.PropertyType;
        var att = prop.GetCustomAttribute<StateCheckNameAttribute>();
        var propName = att != null ? att.Name : prop.Name;
        if (updatedTypesNumerical.Contains(propType))
        {
          object? obj = prop.GetValue(source, null);
          EAssert.IsNotNull(obj);
          if (obj is double d)
            target[propName] = d;
          else if (obj is int i)
            target[propName] = (double)i;
          else if (obj is bool b)
            target[propName] = b ? 1 : 0;
        }
        else if (propType.IsArray)
        {
          Array? arr = (Array?)prop.GetValue(source, null);
          EAssert.IsNotNull(arr);
          for (int i = 0; i < arr.Length; i++)
          {
            string indexPropName = $"{propName}:{i + 1}";
            object? obj = arr.GetValue(i);
            EAssert.IsNotNull(obj);
            if (obj is double d)
              target[indexPropName] = d;
            else if (obj is int _i)
              target[indexPropName] = (double)_i;
            else if (obj is bool b)
              target[indexPropName] = b ? 1 : 0;
          }
        }
      }
    }



    public bool Evaluate(IStateCheckItem item)
    {
      bool ret;
      EAssert.Argument.IsNotNull(item, nameof(item));
      lock (this)
      {
        logger.Invoke(LogLevel.INFO, $"Evaluation of {item.DisplayString} started.");
        this.recentResultSet.Clear();
        this.currentPropertyValues = propertyValuesProvider.GetPropertyValues();
        this.currentVariableValues = variableValuesProvider.GetVariableValues();
        ret = EvaluateItem(item);
        this.currentVariableValues = null;
        this.currentPropertyValues = null;
        logger.Invoke(LogLevel.INFO, $"Evaluation of {item.DisplayString} resulted in {ret}.");
      }
      return ret;
    }

    public void Reset()
    {
      lock (this)
      {
        this.passingPropertiesStates.Clear();
        this.delayCounter.Clear();
      }
    }

    #endregion Public Methods

    #region Private Methods

    private double ApplyPropertyRandomness(StateCheckProperty property, double value)
    {
      var randomness = property.Randomness;
      double absUpper = value + randomness.Above.GetValue(value);
      double absLower = value + randomness.Below.GetValue(value);
      double ret = random.NextDouble(absLower, absUpper);
      return ret;
    }

    private bool EvalauteTrueFalse(StateCheckTrueFalse trueFalse, out string message)
    {
      bool ret = trueFalse.Value;
      message = $"T/F => {trueFalse.Value}";
      recentResultSet.Add(new(trueFalse, ret, string.Empty));
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
      recentResultSet.Add(new(condition, ret, string.Empty));
      return ret;
    }

    private bool EvaluateDelay(StateCheckDelay delay, out string message)
    {
      bool ret;
      bool tmp = EvaluateItem(delay.Item);
      if (tmp)
      {
        if (delayCounter.ContainsKey(delay))
          delayCounter[delay]++;
        else
          delayCounter[delay] = 1;
      }
      else
        delayCounter[delay] = 0;

      var value = delay.IsVariableBased
        ? GetVariableValue(delay.GetSecondsAsVariableName())
        : delay.GetSecondsAsDouble();

      ret = delayCounter[delay] >= value;

      message = $"curr_sec/trg_sec/inner => {delayCounter[delay]} / {delay.Seconds} /= {tmp}";
      recentResultSet.Add(new RecentResult(delay, ret, $"counter={delayCounter[delay]}, seconds={value}"));
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

      recentResultSet.Add(new(
        property,
        ret,
        new StringBuilder(passingPropertiesStates.ContainsKey(property) ? $"passState={passingPropertiesStates[property]}, " : string.Empty)
        .Append($"target({property.Expression})={expected}, property={actual}")
        .ToString()));
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
          ret = GetVariableValue(property.GetExpressionAsVariableName());
        if (applyRandomness) ret = ApplyPropertyRandomness(property, ret);
        extractedValues[property] = ret;
      }
      return ret;
    }

    private double GetVariableValue(string variableName)
    {
      if (!this.currentVariableValues!.TryGetValue(variableName, out double ret))
        throw new StateCheckException($"Unable resolve value of variable '{variableName}'.");
      return ret;
    }

    private (double, double) ExtractPropertySensitivity(StateCheckProperty property, double value)
    {
      double max = value + property.Sensitivity.Above.GetValue(value);
      double min = value - property.Sensitivity.Below.GetValue(value);
      return (min, max);
    }

    private void Log(IStateCheckItem property, string msg, bool ret)
    {
      this.logger.Invoke(LogLevel.INFO, $"EVAL {property.DisplayString} \t {msg} \t {ret}");
    }

    private double ResolveRealPropertyValue(string propertyName)
    {
      if (currentPropertyValues!.ContainsKey(propertyName) == false)
        throw new ApplicationException($"Property {propertyName} not found in property-value dictionary.");
      double ret = currentPropertyValues[propertyName];
      return ret;
    }

    #endregion Private Methods
  }
}
