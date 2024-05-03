using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimObjects;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.VMs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static ESystem.Functions.TryCatch;

namespace ChlaotModuleBase.ModuleUtils.WPF.VMs
{
  public abstract class GenericVMS<VM, TItem> : BindingList<VM> where VM : WithValueVM where TItem : notnull
  {
    private readonly Func<VM, string> nameSelector;
    private readonly Func<VM, TItem> itemSelector;

    public Dictionary<string, double> GetAsDict() => this.ToDictionary(q => nameSelector(q), q => q.Value);

    public double this[string name]
    {
      get => GetBy(name).Value;
      set => GetBy(name).Value = value;
    }

    public double this[TItem item]
    {
      get => GetBy(item).Value;
      set => GetBy(item).Value = value;
    }

    public double this[VM vm]
    {
      get => GetBy(vm).Value;
      set => GetBy(vm).Value = value;
    }

    public void SetIfExists(string name, double value)
    {
      var trg = this.FirstOrDefault(q => nameSelector(q) == name);
      if (trg != null)
        trg.Value = value;
    }

    public void SetIfExists(TItem item, double value)
    {
      var trg = this.FirstOrDefault(q => itemSelector(q).Equals(item));
      if (trg != null)
        trg.Value = value;
    }

    public void SetIfExists(VM vm, double value)
    {
      var trg = this.FirstOrDefault(q => q.Equals(vm));
      if (trg != null)
        trg.Value = value;
    }

    private VM GetBy(string name) => Try(
      () => this.Single(q => nameSelector(q) == name),
      ex => new ApplicationException($"Unable to find unique name '{name}'", ex));

    private VM GetBy(VM item) => Try(
      () => this.Single(q => q.Equals(item)),
      ex => new ApplicationException($"Unable to find unique VM '{nameSelector(item)}'", ex));

    private VM GetBy(TItem item) => Try(
      () => this.Single(q => itemSelector(q).Equals(item)),
      ex => new ApplicationException($"Unable to find unique item '{item}'", ex));

    protected GenericVMS(IEnumerable<VM> items, Func<VM, string> nameSelector, Func<VM, TItem> itemSelector)
    {
      this.nameSelector = nameSelector;
      this.itemSelector = itemSelector;
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
