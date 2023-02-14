using ChecklistModule;
using Eng.Chlaot.ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Eng.Chlaot.Modules.ChecklistModule
{
  public class ChecklistModule : Eng.Chlaot.ChlaotModuleBase.IModule
  {
    private Control _InitControl;
    public Control InitControl => _InitControl;

    private Control _RunControl;
    public Control RunControl => _RunControl;

    private Context _Context;

    public event IModule.LogDelegate Log;

    public IModuleProcessor Processor => throw new NotImplementedException();
    public string Name => "Check-lists";

    public ChecklistModule()
    {
      this._Context = new Context();
      this._Context.Log += _Context_Log;

      this._InitControl = new CtrInit(this._Context);
      this._RunControl = new CtrRun(this._Context);
    }

    private void _Context_Log(LogLevel level, string message)
    {
      this.Log?.Invoke(level, message);
    }
  }
}
