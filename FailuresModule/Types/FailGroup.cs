using Eng.Chlaot.ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FailuresModule.Types
{
  public class FailGroup : NotifyPropertyChangedBase
  {
    public enum ESelector
    {
      Any,
      One,
      All,
      None
    }

    public FailGroup() : this("?") { }

    public FailGroup(string title)
    {
      this.Title = title ?? throw new ArgumentNullException(nameof(title));
      this.Groups = new();
      this.Failures = new();
      void list_Changed(object? sender, ListChangedEventArgs e)
      {
        this.InvokePropertyChanged(nameof(Items));
      };

      this.Groups.ListChanged += list_Changed;
      this.Failures.ListChanged += list_Changed;
    }

    private void list_Changed(object? sender, ListChangedEventArgs e)
    {
      this.InvokePropertyChanged(nameof(Items));
    }

    public BindingList<FailGroup> Groups
    {
      get => base.GetProperty<BindingList<FailGroup>>(nameof(Groups))!;
      set
      {
        base.UpdateProperty(nameof(Groups), value);
        if (value != null)
          value.ListChanged += list_Changed;
      }
    }

    public BindingList<Failure> Failures
    {
      get => base.GetProperty<BindingList<Failure>>(nameof(Failures))!;
      set
      {
        base.UpdateProperty(nameof(Failures), value);
        if (value != null)
          value.ListChanged += list_Changed;
      }
    }

    public string Title { get; set; }
    public double? Probability { get; set; }
    public ESelector Selector { get; set; }

    public List<object> Items
    {
      get
      {
        List<object> ret = new();
        ret.AddRange(Groups.OrderBy(q => q.Title));
        ret.AddRange(Failures.OrderBy(q => q.Title));
        return ret;
      }
    }
  }
}
