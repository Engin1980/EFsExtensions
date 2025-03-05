using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimConWrapping.Exceptions;
using ESimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimConWrapping
{
  public abstract class SimConWrapper
  {
    public delegate void SimConErrorRaisedDelegate(SimConWrapperSimConException ex);
    public SimConErrorRaisedDelegate? SimConErrorRaised;
    #region Protected Fields

    protected readonly ESimConnect.ESimConnect simCon;

    #endregion Protected Fields

    #region Private Fields

    private bool isStarted = false;

    #endregion Private Fields

    #region Public Constructors

    public SimConWrapper(ESimConnect.ESimConnect simCon)
    {
      this.simCon = simCon ?? throw new ArgumentNullException(nameof(simCon));
      this.simCon.ThrowsException += SimCon_ThrowsException;
    }

    private void SimCon_ThrowsException(ESimConnect.ESimConnect sender, SimConnectException ex)
    {
      SimConWrapperSimConException scex = new SimConWrapperSimConException(ex.ToString());
      SimConErrorRaised?.Invoke(scex);
    }

    #endregion Public Constructors

    #region Public Methods

    public void Open()
    {
      try
      {
        simCon.Open();
      }
      catch (Exception ex)
      {
        throw new SimConWrapperOpenException(ex);
      }
    }

    public void Start()
    {
      if (simCon == null)
        throw new ApplicationException("SimConManager not opened().");
      if (simCon.IsOpened == false)
        simCon.Open();
      if (isStarted)
        return;

      StartProtected();

      isStarted = true;
    }

    #endregion Public Methods

    #region Protected Methods

    protected abstract void StartProtected();

    #endregion Protected Methods
  }
}
