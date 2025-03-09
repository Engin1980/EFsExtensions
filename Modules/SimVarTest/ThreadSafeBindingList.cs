using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.SimVarTestModule
{
  public class ThreadSafeBindingList<T> : BindingList<T>
  {
    private readonly object _lock = new();

    protected override void InsertItem(int index, T item)
    {
      lock (_lock)
      {
        base.InsertItem(index, item);
      }
    }

    protected override void RemoveItem(int index)
    {
      lock (_lock)
      {
        base.RemoveItem(index);
      }
    }

    protected override void SetItem(int index, T item)
    {
      lock (_lock)
      {
        base.SetItem(index, item);
      }
    }

    protected override void ClearItems()
    {
      lock (_lock)
      {
        base.ClearItems();
      }
    }

    /// <summary>
    /// Vrátí bezpečnou kopii kolekce pro iteraci bez rizika InvalidOperationException.
    /// </summary>
    public List<T> GetSafeCopy()
    {
      lock (_lock)
      {
        return new List<T>(this);
      }
    }
  }

}
