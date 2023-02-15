using ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Eng.Chlaot.ChlaotModuleBase
{
  public interface IModule : INotifyPropertyChanged
  {
    public bool IsReady { get; }
    public void Init(LogHandler logHandler);
    void Start();

    public Control InitControl { get; }
    public Control RunControl { get; }
    public string Name { get; }
  }
}
