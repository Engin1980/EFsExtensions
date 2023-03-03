using System;

namespace ChlaotModuleBase.ModuleUtils.StateChecking
{
  public class StateCheckProperty : IStateCheckItem
  {
    public StateCheckPropertyName Name { get; set; }
    public int NameIndex { get; set; } = 0;
    public string Expression { get; set; }
    public StateCheckPropertyDirection Direction { get; set; }

    public string? Randomize { get; set; }
    public string? Sensitivity { get; set; }

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
    public string DisplayString => $"({DisplayName} {Direction.ToString().ToLower()} {Expression})";

    public string? VariableName
    {
      get
      {
        if (Expression != null && Expression.Length > 0 && Expression[0] == '{')
          return Expression[1..^1];
        else
          return null;
      }
    }

    internal double GetValue()
    {
      throw new NotImplementedException();
    }
  }
}
