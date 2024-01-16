using Eng.Chlaot.ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChlaotModuleBase
{
  public class BindingKeyValue<K, V> : NotifyPropertyChangedBase
  {
    public K Key
    {
      get => base.GetProperty<K>(nameof(Key))!;
      set => base.UpdateProperty(nameof(Key), value);
    }

    public V Value
    {
      get => base.GetProperty<V>(nameof(Value))!;
      set => base.UpdateProperty(nameof(Value), value);
    }

    public BindingKeyValue(K key, V value)
    {
      Key = key;
      Value = value;
    }
  }
}
