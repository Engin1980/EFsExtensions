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

    public void SetUp(ModuleSetUpInfo setUpInfo);

    public void Init();
    public void Run();

    public void Stop();

    public Control InitControl { get; }
    public Control RunControl { get; }
    public string Name { get; }
    public Dictionary<string, string>? TryGetRestoreData();
    public void Restore(Dictionary<string, string> restoreData);
  }
}
