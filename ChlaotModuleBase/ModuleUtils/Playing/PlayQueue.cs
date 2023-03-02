using System.Collections.Generic;

namespace ChlaotModuleBase.ModuleUtils.Playing
{
  internal class PlayQueue
  {
    public delegate void ChangedDelegate();
    public event ChangedDelegate? NewItemInserted;
    private readonly List<InternalPlayer> inner = new();

    internal void Clear()
    {
      lock (inner)
      {
        this.inner.Clear();
      }
    }

    internal void Enqueue(InternalPlayer ip)
    {
      lock (inner)
      {
        inner.Add(ip);
        this.NewItemInserted?.Invoke();
      }
    }

    internal InternalPlayer? TryDequeue()
    {
      InternalPlayer? ret = null;
      lock (inner)
      {
        if (inner.Count > 0)
        {
          ret = inner[0];
          inner.RemoveAt(0);
        }
      }
      return ret;
    }
  }
}
