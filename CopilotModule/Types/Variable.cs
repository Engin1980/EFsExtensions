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

    public bool HasValue
    {
      get => base.GetProperty<bool>(nameof(HasValue))!;
      private set => base.UpdateProperty(nameof(HasValue), value);
    }
    public double? Value
    {
      get => base.GetProperty<double?>(nameof(Value))!;
      set
      {
        base.UpdateProperty(nameof(Value), value);
        this.HasValue = this.Value != null;
      }
    }
#pragma warning disable CS8618
    public string Name { get; set; }
    public double? DefaultValue { get; set; } = null;
    public string? Info { get; set; }
#pragma warning restore CS8618
    public SpeechDefinition? Parent { get; set; }

  }
}
