using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChlaotModuleBase
{
  //TODO probably unused, delete
  public class BindingDictionary<K, V> : BindingList<BindingKeyValue<K, V?>> where K : notnull
  {
    private bool isDictResetRequired = false;
    private readonly Dictionary<K, V?> dict = new();

    public BindingDictionary()
    {
      this.ListChanged += BindingDictionary_ListChanged;
      this.AddingNew += BindingDictionary_AddingNew;
      this.AllowNew = false;
      this.AllowEdit = false;
    }

    private void BindingDictionary_AddingNew(object? sender, AddingNewEventArgs e)
    {
      isDictResetRequired = true;
    }

    public V? this[K key]
    {
      get
      {
        if (isDictResetRequired)
        {
          dict.Clear();
          foreach (var kv in this)
            dict[kv.Key] = kv.Value;
        }
        return dict[key];
      }
      set
      {
        var it = this.FirstOrDefault(q => q.Key.Equals(key));
        if (it == null)
          this.Add(new(key, value));
        else
          it.Value = value;
        isDictResetRequired = true;
      }
    }

    private void BindingDictionary_ListChanged(object? sender, ListChangedEventArgs e)
    {
      isDictResetRequired = true;
    }
  }
}
