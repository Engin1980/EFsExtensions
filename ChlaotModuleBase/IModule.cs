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
    public delegate void LogHandler(LogLevel level, string message);

    public bool IsReady { get; }
    public void Init(LogHandler logHandler);
    public Control InitControl { get; }
    public Control RunControl { get; }
    public string Name { get; }
  }
}
