using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimObjects;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.VMs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChlaotModuleBase.ModuleUtils.WPF.VMs
{
  public abstract class GenericVMS<T> : BindingList<T> where T : WithValueVM
  {
    private readonly Func<T, string> nameSelector;

    public Dictionary<string, double> GetAsDict() => this.ToDictionary(q => nameSelector(q), q => q.Value);

    public double this[string name]
    {
      get => this.Single(q => nameSelector(q) == name).Value;
      set => this.Single(q => nameSelector(q) == name).Value = value;
    }

    public double this[T index]
    {
      get => this.Single(q => q.Equals(index)).Value;
      set => this.Single(q => q.Equals(index)).Value = value;
    }

    protected GenericVMS(IEnumerable<T> items, Func<T, string> nameSelector)
    {
      this.nameSelector = nameSelector;
      foreach (var item in items)
      {
        this.Add(item);
      }
      this.AddingNew += GenericVMS_AddingNew;
      this.ListChanged += GenericVMS_ListChanged;
    }

    private void GenericVMS_ListChanged(object? sender, ListChangedEventArgs e)
    {
      switch (e.ListChangedType)
      {
        case ListChangedType.ItemAdded:
        case ListChangedType.ItemDeleted:
        case ListChangedType.Reset:
          throw new ApplicationException("List adjustment is not allowed.");
      }
    }

    private void GenericVMS_AddingNew(object? sender, AddingNewEventArgs e)
    {
      throw new ApplicationException("Adding is not allowed.");
    }
  }
}
