using Eng.Chlaot.ChlaotModuleBase;

namespace Eng.Chlaot.Modules.ChecklistModule
{
  public partial class RunContext
  {
    public class PropertyValue : NotifyPropertyChangedBase
    {
      public PropertyValue(string name, double value)
      {
        Name = name;
        Value = value;
      }

      public string Name
      {
        get => base.GetProperty<string>(nameof(Name))!;
        set => base.UpdateProperty(nameof(Name), value);
      }

      public double Value
      {
        get => base.GetProperty<double>(nameof(Value))!;
        set => base.UpdateProperty(nameof(Value), value);
      }
    }
  }
}
