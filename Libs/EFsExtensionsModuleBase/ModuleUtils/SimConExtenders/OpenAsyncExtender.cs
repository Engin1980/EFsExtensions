using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimConWrapping.Exceptions;
using ESystem.Asserting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media.Animation;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimConExtenders
{
  public class OpenAsyncExtender
  {
    private const int INITIAL_CONNECTION_DELAY = 1000;
    private const int REPEATED_CONNECTION_DELAY = 5000;

    private readonly ESimConnect.ESimConnect simCon;
    private readonly Timer connectionTimer;
    public event Action? Opened = null!;
    public event Action<Exception>? OpeningFailed = null!;
    public bool IsOpened { get; private set; } = false;
    public bool IsOpening { get; private set; } = false;

    public OpenAsyncExtender(ESimConnect.ESimConnect simCon)
    {
      EAssert.Argument.IsNotNull(simCon, nameof(simCon));
      this.simCon = simCon;
      connectionTimer = new()
      {
        AutoReset = false,
        Enabled = false
      };
      connectionTimer.Elapsed += ConnectionTimer_Elapsed;
    }

    private void ConnectionTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
      try
      {
        this.simCon.Open();
        this.IsOpening = false;
        this.IsOpened = true;
      }
      catch (SimConWrapperOpenException ex)
      {
        OpeningFailed?.Invoke(ex);
        connectionTimer.Interval = REPEATED_CONNECTION_DELAY; // also resets timer countdown
        connectionTimer.Start();
      }
      catch (Exception ex)
      {
        var tmp = new ApplicationException("Unexpected exception when starting simcon on background.", ex);
        OpeningFailed?.Invoke(tmp);
        connectionTimer.Interval = REPEATED_CONNECTION_DELAY; // also resets timer countdown
        connectionTimer.Start();
      }

      if (this.IsOpened)
        Opened?.Invoke();
    }

    public void OpenAsync()
    {
      if (this.IsOpened) return;
      lock (this)
      {
        if (this.IsOpening) return;
        this.IsOpening = true;
      }
      connectionTimer.Interval = INITIAL_CONNECTION_DELAY;
      connectionTimer.Start();
    }
  }
}
