using ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopilotModule.Types
{
  public class Variable : NotifyPropertyChangedBase
  {
    public double Value
    {
      get => base.GetProperty<double>(nameof(Value))!;
      set => base.UpdateProperty(nameof(Value), value);
    }
#pragma warning disable CS8618
    public string Name { get; set; }
    public double DefaultValue { get; set; } = 0;
    public string? Info { get; set; }
#pragma warning restore CS8618
  }
}
