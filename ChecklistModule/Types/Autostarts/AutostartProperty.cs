namespace ChecklistModule.Types.Autostarts
{
  public class AutostartProperty : IAutostart
  {
    public AutostartPropertyName Name { get; set; }
    public int Value { get; set; }
    public AutostartPropertyDirection Direction { get; set; }

    public string DisplayString => $"({Name} {Direction.ToString().ToLower()} {Value})";
  }
}
