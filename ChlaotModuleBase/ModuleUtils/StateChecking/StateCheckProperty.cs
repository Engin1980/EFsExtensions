using ELogging;
using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ChlaotModuleBase.ModuleUtils.StateChecking
{
  public class StateCheckProperty : IStateCheckItem
  {
    public const string EXPRESSION_REGEX = "(^\\{[a-zA-Z][a-zA-Z0-9\\-_]*\\}$)|(^[+-]?\\d+\\.?\\d*$)";
    public const string RANGE_STRING_REGEX = "([+-]{0,2})(\\d+\\.?\\d*)(\\%?)";

    private static readonly Random random = new();
    private double randomizedValue = double.NaN;
    private double sensitivityEpsilon = double.NaN;
    public StateCheckPropertyDirection Direction { get; set; }
    public string DisplayName
    {
      get
      {
        if (NameIndex == 0)
          return $"{Name}";
        else
          return $"{Name}:{NameIndex}";
      }
    }
    public string DisplayString => $"({DisplayName} {Direction.ToString().ToLower()} {Expression ?? "(null)"})";
    public string? _Expression = null;
    public string? Expression
    {
      get => this._Expression;
      set
      {
        this._Expression = value;
        EnsureExpressionIsValid();
      }
    }
    private void EnsureExpressionIsValid()
    {
      if (this.Expression == null || Regex.IsMatch(this.Expression, EXPRESSION_REGEX) == false)
        throw new ApplicationException(
          $"{nameof(StateCheckProperty)}.{nameof(Expression)} is not valid for '{DisplayString}'. It must match regex '{EXPRESSION_REGEX}')");
    }
    public StateCheckPropertyName Name { get; set; }
    public int NameIndex { get; set; } = 0;
    public string? Randomize { get; set; } = "0";
    public double RandomizedValue
    {
      get
      {
        if (double.IsNaN(randomizedValue)) AdjustSensitivityAndRandomize();
        return randomizedValue;
      }
    }

    public string? Sensitivity { get; set; } = "+-10%";
    public double SensitivityEpsilon
    {
      get
      {
        if (double.IsNaN(sensitivityEpsilon)) AdjustSensitivityAndRandomize();
        return sensitivityEpsilon;
      }
    }

    private double? _Value = null;
    public double? Value
    {
      get
      {
        double? ret;
        if (this._Value is null)
        {
          EnsureExpressionIsValid();
          ret = double.TryParse(this.Expression, out double tmp) ? tmp : null;
        }
        else
          ret = this._Value;
        return ret;
      }
      set
      {
        if (this._Value != value)
        {
          this._Value = value;
          this.randomizedValue = double.NaN;
          this.sensitivityEpsilon = double.NaN;
        }
      }
    }

    public string? VariableName
    {
      get
      {
        EnsureExpressionIsValid();
        if (Expression![0] == '{')
          return Expression[1..^1];
        else
          return null;
      }
    }

    private static (double lower, double upper, bool isPercentage) ExpandRangeString(string rangeString)
    {
      Match match = Regex.Match(rangeString, RANGE_STRING_REGEX);
      if (!match.Success)
        throw new ApplicationException($"Failed to parse '{rangeString}' as range-string.");

      string pm = match.Groups[1].Value;
      double val = double.Parse(match.Groups[2].Value);
      bool isProc = match.Groups[3].Value.Length > 0;

      double lower = pm.Contains('-') ? -val : 0;
      double upper = pm.Contains('+') ? +val : 0;

      Logger.Log(nameof(StateCheckProperty), LogLevel.VERBOSE,
        $"ExpandRangeString source={rangeString} => ({lower}, {upper}, {(isProc ? "proc" : "abs")}");
      return (lower, upper, isProc);
    }

    private void AdjustSensitivityAndRandomize()
    {
      if (Value == null)
        throw new ApplicationException(
          $"Cannot adjust sensitivity/randomize-value. " +
          $"${nameof(StateCheckProperty)}.{nameof(Value)} is null ({DisplayName}).");
      if (Randomize == null)
        throw new ApplicationException(
          $"Cannot adjust sensitivity/randomize-value. " +
          $"${nameof(StateCheckProperty)}.{nameof(Randomize)} is null ({DisplayName}).");
      if (Sensitivity == null)
        throw new ApplicationException(
          $"Cannot adjust sensitivity/randomize-value. " +
          $"${nameof(StateCheckProperty)}.{nameof(Sensitivity)} is null ({DisplayName}).");

      // randomization
      (double lower, double upper, bool isPerc) = ExpandRangeString(this.Randomize);
      double shift = random.NextDouble() * (upper - lower) + lower;
      if (isPerc)
        randomizedValue = Value.Value * (1 + shift / 100d);
      else
        randomizedValue = Value.Value + shift;
      Logger.Log(this, LogLevel.VERBOSE,
        $"{this.DisplayString} adjusted randomizedValue, str={Randomize}" +
        $", value={Value}, randomizedValue={randomizedValue}");

      // sensitivity
      (lower, upper, isPerc) = ExpandRangeString(this.Sensitivity);
      if (-lower != upper)
      {
        Logger.Log(this, LogLevel.WARNING,
          $"Different lower/upper sensitivity ({lower} vs {upper}) value not supported. The higher abs value is used");
        upper = Math.Max(Math.Abs(lower), Math.Abs(upper));
      }
      if (isPerc)
        sensitivityEpsilon = randomizedValue * upper / 100d;
      else
        sensitivityEpsilon = upper;
      Logger.Log(this, LogLevel.VERBOSE,
        $"{this.DisplayString} adjusted sensitivity, str={Sensitivity}" +
        $", randomizedValue={randomizedValue}, epsilon={sensitivityEpsilon}");
    }
  }
}
