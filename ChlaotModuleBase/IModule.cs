using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Eng.Chlaot.ChlaotModuleBase
{
  public interface IModule
  {
    public delegate void LogDelegate(LogLevel level, string message);
    public event LogDelegate Log;

    public Control InitControl { get; }
    public Control RunControl { get; }
    public IModuleProcessor Processor { get; }
    public string Name { get; }
  }
}
