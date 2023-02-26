namespace ChecklistModule.Types.Autostarts
{
  public class AutostartProperty : IAutostart
  {
    public AutostartPropertyName Name { get; set; }
    public int NameIndex { get; set; } = 0;
    public int Value { get; set; }
    public AutostartPropertyDirection Direction { get; set; }

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
    public string DisplayString => $"({DisplayName} {Direction.ToString().ToLower()} {Value})";
  }
}
