using ChlaotModuleBase.ModuleUtils.SimConWrapping.Exceptions;
using ESystem.Asserting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media.Animation;

namespace ChlaotModuleBase.ModuleUtils.SimConWrapping
{
  public class SimConWrapperWithOpenAsync : SimConWrapper
  {
    private const int INITIAL_CONNECTION_DELAY = 1000;
    private const int REPEATED_CONNECTION_DELAY = 5000;

    private readonly Timer connectionTimer;
    private Action onSuccess = null!;
    private Action<SimConWrapperOpenException> onError = null!;
    public SimConWrapperWithOpenAsync(ESimConnect.ESimConnect simCon) : base(simCon)
    {
      connectionTimer = new();
      connectionTimer.Elapsed += ConnectionTimer_Elapsed;
      connectionTimer.Enabled = false;
    }

    private void ConnectionTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
      try
      {
        base.Open();
        onSuccess.Invoke();
      }
      catch (SimConWrapperOpenException ex)
      {
        onError.Invoke(ex);
        connectionTimer.Interval = REPEATED_CONNECTION_DELAY; // also resets timer countdown
      }
      catch (Exception ex)
      {
        throw new ApplicationException("Unexpected exception when starting simcon on background.", ex);
      }
    }

    public void OpenAsync(Action onSuccess, Action<SimConWrapperOpenException> onError)
    {
      EAssert.Argument.IsNotNull(onSuccess);
      EAssert.Argument.IsNotNull(onError);
      this.onSuccess = onSuccess;
      this.onError = onError;

      connectionTimer.Interval = INITIAL_CONNECTION_DELAY;
    }

    protected override void StartProtected()
    {
      // intentionally blank
    }
  }
}
