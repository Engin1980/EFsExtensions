using Eng.EFsExtensions.EFsExtensionsModuleBase;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.KeyHooking;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimConWrapping.PrdefinedTypes;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimConWrapping;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects;
using ESystem;
using System.ComponentModel;
using System.Drawing.Imaging;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking.VariableModel;
using Eng.EFsExtensions.Modules.ChecklistModule.Types.VM;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.VMs;
using ESystem.Miscelaneous;
using ESystem.Logging;

namespace Eng.EFsExtensions.Modules.ChecklistModule
{
  internal partial class RunContext : NotifyPropertyChanged
  {
    #region Private Fields

    private readonly Logger logger;
    private readonly ChecklistManager manager;
    private readonly Settings settings;
    private readonly NewSimObject simObject;
    private int keyHookPlayPauseId = -1;
    private int keyHookSkipNextId = -1;
    private int keyHookSkipPrevId = -1;
    private KeyHookWrapper? keyHookWrapper;

    #endregion Private Fields

    #region Public Properties

    public CheckListVM EvaluatorRecentResultChecklistVM
    {
      get => base.GetProperty<CheckListVM>(nameof(EvaluatorRecentResultChecklistVM))!;
      set => base.UpdateProperty(nameof(EvaluatorRecentResultChecklistVM), value);
    }

    public List<CheckListVM> CheckListVMs
    {
      get => base.GetProperty<List<CheckListVM>>(nameof(CheckListVMs))!;
      set => base.UpdateProperty(nameof(CheckListVMs), value);
    }

    public PropertyVMS PropertyVMs
    {
      get => base.GetProperty<PropertyVMS>(nameof(PropertyVMs))!;
      set => base.UpdateProperty(nameof(PropertyVMs), value);
    }

    #endregion Public Properties

    #region Public Constructors

    public RunContext(InitContext initContext)
    {
      this.logger = Logger.Create(this, "CheckList.RunContext");
      this.settings = initContext.Settings;

      //this.Name = typeof(RunContext);
      this.CheckListVMs = initContext.CheckListVMs;
      this.PropertyVMs = PropertyVMS.Create(
        initContext.SimPropertyGroup.GetAllSimPropertiesRecursively()
        .Where(q => initContext.PropertyUsageCounts.Any(p => p.Property == q)));

      this.simObject = NewSimObject.GetInstance();
      this.simObject.SimSecondElapsed += SimObject_SimSecondElapsed;
      this.simObject.Started += SimObject_Started;
      this.simObject.Started += () => this.simObject.RegisterProperties(this.PropertyVMs.Select(q => q.Property));
      this.simObject.SimPropertyChanged += SimObject_SimPropertyChanged;

      this.manager = new ChecklistManager(this.PropertyVMs, this.CheckListVMs, this.simObject,
        this.settings.UseAutoplay,
        this.settings.ReadConfirmations,
        this.settings.AlertOnPausedChecklist ? this.settings.PausedChecklistAlertInterval * 1000 : null,
        this.settings.PlayPerItem
        ); // CheckListViews must be set before calling this
    }

    #endregion Public Constructors

    #region Internal Methods

    internal void SetCurrentChecklist(CheckListVM vm)
    {
      this.manager.SetCurrentChecklist(vm);
    }

    internal void Run(KeyHookWrapper keyHookWrapper)
    {
      logger.Log(LogLevel.INFO, "Run");

      logger.Log(LogLevel.DEBUG, "Resetting playback");
      manager.Reset();

      logger.Log(LogLevel.DEBUG, "Adding key hooks");
      this.keyHookWrapper = keyHookWrapper ?? throw new ArgumentNullException(nameof(keyHookWrapper));
      ConnectKeyHooks();

      logger.Log(LogLevel.DEBUG, "Starting simObject connection");
      this.simObject.StartInBackground();

      logger.Log(LogLevel.DEBUG, "Run done");
    }

    internal void Stop()
    {
      logger.Log(LogLevel.INFO, "Stopping");
      logger.Log(LogLevel.WARNING, "Stop for RunContext of CheckListModule is not implemented.");
      this.simObject.Started -= SimObject_Started;
      this.simObject.SimPropertyChanged -= SimObject_SimPropertyChanged;
      this.keyHookWrapper!.UnregisterAllKeyHooks();
      logger.Log(LogLevel.INFO, "Stopped");
    }

    #endregion Internal Methods

    #region Private Methods

    private static KeyHookInfo ConvertShortcutToKeyHookInfo(Settings.KeyShortcut shortcut)
    {
      KeyHookInfo ret = new(
        shortcut.Alt,
        shortcut.Control,
        shortcut.Shift,
        shortcut.Key);
      return ret;
    }

    private void ConnectKeyHooks()
    {
      Settings.KeyShortcut s = null!;

      if (keyHookWrapper == null) throw new ApplicationException("KeyHookWrapper not set.");

      try
      {
        s = settings.Shortcuts.PlayPause;
        logger.Log(LogLevel.INFO, "Assigning play-pause keyboard shortcut " + s);
        this.keyHookPlayPauseId = this.keyHookWrapper.RegisterKeyHook(ConvertShortcutToKeyHookInfo(s));
      }
      catch (Exception ex)
      {
        logger.Log(LogLevel.ERROR, $"Failed to bind key-hook for shortcut {s}. Reason: {ex.GetFullMessage()}");
      }

      try
      {
        s = settings.Shortcuts.SkipToNext;
        logger.Log(LogLevel.INFO, "Assigning skip-to-next keyboard shortcut " + s);
        this.keyHookSkipNextId = this.keyHookWrapper.RegisterKeyHook(ConvertShortcutToKeyHookInfo(s));
      }
      catch (Exception ex)
      {
        logger.Log(LogLevel.ERROR, $"Failed to bind key-hook for shortcut {s}. Reason: {ex.GetFullMessage()}");
      }

      try
      {
        s = settings.Shortcuts.SkipToPrevious;
        logger.Log(LogLevel.INFO, "Assigning skip-to-previous keyboard shortcut " + s);
        this.keyHookSkipPrevId = this.keyHookWrapper.RegisterKeyHook(ConvertShortcutToKeyHookInfo(s));
      }
      catch (Exception ex)
      {
        logger.Log(LogLevel.ERROR, $"Failed to bind key-hook for shortcut {s}. Reason: {ex.GetFullMessage()}");
      }

      this.keyHookWrapper.KeyHookInvoked += keyHookWrapper_KeyHookInvoked;
    }

    private void keyHookWrapper_KeyHookInvoked(int hookId, KeyHookInfo keyHookInfo)
    {
      if (hookId == this.keyHookPlayPauseId)
      {
        this.manager.TogglePlay();
      }
      else if (hookId == this.keyHookSkipNextId)
      {
        this.manager.SkipToNext();
      }
      else if (hookId == this.keyHookSkipPrevId)
      {
        this.manager.SkipToPrev();
      }
      else
      {
        throw new ApplicationException($"Unknown hook id '{hookId}'.");
      }
    }

    private void SimObject_SimPropertyChanged(SimProperty property, double value)
    {
      this.PropertyVMs.SetIfExists(property, value);
    }

    private void SimObject_SimSecondElapsed()
    {
      this.manager.CheckForActiveChecklistsIfRequired();
    }

    private void SimObject_Started()
    {
      // what to put here?
    }

    #endregion Private Methods

  }
}
