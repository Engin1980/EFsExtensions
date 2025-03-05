using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking.Exceptions;
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
using EXmlLib.Interfaces;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking.StateModel
{
  public class StateCheckProperty : IStateCheckItem, IXmlObjectPostDeserialize
  {

    #region Properties

    public StateCheckPropertyDirection Direction { get; set; }
    public string DisplayString => $"({(IsTrendBased ? "Δ" : "")}{Name}  {Direction.ToString().ToLower()} {Expression ?? "(null)"})";
    public string Expression { get; set; } = null!;
    public bool IsVariableBased { get => Expression[0] == '{'; }
    public string Name { get; set; } = null!;
    public StateCheckPropertyDeviation Randomness { get; set; } = StateCheckPropertyDeviation.Parse("+-0");
    public StateCheckPropertyDeviation Sensitivity { get; set; } = StateCheckPropertyDeviation.Parse("+-10%");
    public bool IsTrendBased { get; set; }

    #endregion Properties

    #region Methods

    public double GetExpressionAsDouble()
    {
      return Double.Parse(Expression, CultureInfo.GetCultureInfo("en-US"));
    }

    public string GetExpressionAsVariableName()
    {
      EAssert.IsTrue(IsVariableBased);
      return Expression[1..^1];
    }

    public override string ToString() => this.DisplayString + " {StateCheckProperty}";

    public void PostDeserialize()
    {
      if (Expression == null) throw new StateCheckException($"{nameof(Expression)} is null.");
      if (Name == null) throw new StateCheckException($"{nameof(Name)} is null.");
      if (Randomness == null) throw new StateCheckException($"{nameof(Randomness)} is null.");
      if (Sensitivity == null) throw new StateCheckException($"{nameof(Sensitivity)} is null.");
    }

    #endregion Methods

  }
}
