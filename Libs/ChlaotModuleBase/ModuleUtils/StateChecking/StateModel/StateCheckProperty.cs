using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.Exceptions;
using ELogging;
using ESystem.Asserting;
using ESystem.ValidityChecking;
using Microsoft.FlightSimulator.SimConnect;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Media.Animation;
using static ESimConnect.SimUnits;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.StateModel
{
  public class StateCheckProperty : IStateCheckItem, IValidable
  {
    #region Fields
    #endregion Fields

    #region Properties

    public StateCheckPropertyDirection Direction { get; set; }
    public string DisplayString => $"({Name} {Direction.ToString().ToLower()} {Expression ?? "(null)"})";
    public string Expression { get; set; } = null!;
    public string Name { get; set; } = null!;
    public StateCheckPropertyDeviation Randomness { get; set; } = StateCheckPropertyDeviation.Parse("0");
    public StateCheckPropertyDeviation Sensitivity { get; set; } = StateCheckPropertyDeviation.Parse("+-10%");
    public bool IsVariableBased { get => Expression[0] == '{'; }

    public void CheckIsValid()
    {
      if (Expression == null) throw new StateCheckException($"{nameof(Expression)} is null.");
      if (Name == null) throw new StateCheckException($"{nameof(Name)} is null.");
      if (Randomness == null) throw new StateCheckException($"{nameof(Randomness)} is null.");
      if (Sensitivity == null) throw new StateCheckException($"{nameof(Sensitivity)} is null.");
    }

    public string GetExpressionAsVariableName()
    {
      EAssert.IsTrue(IsVariableBased);
      return Expression[1..^1];
    }

    public double GetExpressionAsDouble()
    {
      return Double.Parse(Expression, CultureInfo.GetCultureInfo("en-US"));
    }

    //public double Value
    //{
    //  get
    //  {
    //    double? ret;
    //    if (this._Value is null)
    //    {
    //      EnsureExpressionIsValid();
    //      ret = double.TryParse(this.Expression, out double tmp) ? tmp : null;
    //    }
    //    else
    //      ret = this._Value;
    //    return ret;
    //  }
    //  set
    //  {
    //    if (this._Value != value)
    //    {
    //      this._Value = value;
    //      this.randomizedValue = double.NaN;
    //      this.sensitivityEpsilon = double.NaN;
    //    }
    //  }
    //}
    //public string? VariableName
    //{
    //  get
    //  {
    //    EnsureExpressionIsValid();
    //    if (Expression![0] == '{')
    //      return Expression[1..^1];
    //    else
    //      return null;
    //  }
    //}

    #endregion Properties

    #region Methods

    //private static (double lower, double upper, bool isPercentage) ExpandRangeString(string rangeString)
    //{
    //  Match match = Regex.Match(rangeString, RANGE_STRING_REGEX);
    //  if (!match.Success)
    //    throw new ApplicationException($"Failed to parse '{rangeString}' as range-string.");

    //  string pm = match.Groups[1].Value;
    //  double val = double.Parse(match.Groups[2].Value);
    //  bool isProc = match.Groups[3].Value.Length > 0;

    //  double lower = pm.Contains('-') ? -val : 0;
    //  double upper = pm.Contains('+') ? +val : 0;

    //  Logger.Log(nameof(StateCheckProperty), LogLevel.VERBOSE,
    //    $"ExpandRangeString source={rangeString} => ({lower}, {upper}, {(isProc ? "proc" : "abs")}");
    //  return (lower, upper, isProc);
    //}

    //private void AdjustSensitivityAndRandomize()
    //{
    //  if (_Value == null)
    //    throw new ApplicationException(
    //      $"Cannot adjust sensitivity/randomize-value. " +
    //      $"${nameof(StateCheckProperty)}.{nameof(Value)} is null ({Name}).");
    //  if (Randomize == null)
    //    throw new ApplicationException(
    //      $"Cannot adjust sensitivity/randomize-value. " +
    //      $"${nameof(StateCheckProperty)}.{nameof(Randomize)} is null ({Name}).");
    //  if (Sensitivity == null)
    //    throw new ApplicationException(
    //      $"Cannot adjust sensitivity/randomize-value. " +
    //      $"${nameof(StateCheckProperty)}.{nameof(Sensitivity)} is null ({Name}).");

    //  // randomization
    //  randomizedValue = ExpandValueByDeviation(this.Value, this.Randomize);
    //  Logger.Log(this, LogLevel.VERBOSE,
    //    $"{this.DisplayString} adjusted randomizedValue, str={Randomize}" +
    //    $", value={Value}, randomizedValue={randomizedValue}");

    //  // sensitivity
    //  sensitivityEpsilonRange = (this.Sensitivity.Below.GetValue(this.Value), this.Sensitivity.Above.GetValue(this.Value));
    //  Logger.Log(this, LogLevel.VERBOSE,
    //    $"{this.DisplayString} adjusted sensitivity, str={Sensitivity}" +
    //    $", randomizedValue={randomizedValue}, epsilon={sensitivityEpsilonRange}");
    //}

    //private double ExpandValueByDeviation(double value, StateCheckPropertyDeviation deviation)
    //{
    //  double absUpper = deviation.Above.GetValue(value);
    //  double absLower = deviation.Below.GetValue(value);
    //  double ret = random.NextDouble(absLower, absUpper);
    //  return ret;
    //}

    //private void EnsureExpressionIsValid()
    //{
    //  if (this.Expression == null || Regex.IsMatch(this.Expression, EXPRESSION_REGEX) == false)
    //    throw new ApplicationException(
    //      $"{nameof(StateCheckProperty)}.{nameof(Expression)} is not valid for '{DisplayString}'. It must match regex '{EXPRESSION_REGEX}')");
    //}

    #endregion Methods
  }
}
