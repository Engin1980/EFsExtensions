using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.KeyHooking;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.Modules.ChecklistModule.Types;
using Eng.Chlaot.Modules.ChecklistModule.Types.RunViews;
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
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimConWrapping.PrdefinedTypes;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimConWrapping;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimObjects;
using ESystem;
using System.ComponentModel;
using System.Drawing.Imaging;
using ChlaotModuleBase;

namespace Eng.Chlaot.Modules.ChecklistModule
{
  public partial class RunContext : NotifyPropertyChangedBase
  {
    #region Private Fields

    private readonly Logger logger;
    private readonly ChecklistManager manager;
    private readonly Dictionary<string, double> propertyValues = new();
    private readonly Dictionary<string, double> variableValues = new();
    private readonly Settings settings;
    private readonly SimObject simObject;
    private int keyHookPlayPauseId = -1;
    private int keyHookSkipNextId = -1;
    private int keyHookSkipPrevId = -1;
    private KeyHookWrapper? keyHookWrapper;

    #endregion Private Fields

    #region Public Properties

    public List<CheckListView> CheckListViews
    {
      get => base.GetProperty<List<CheckListView>>(nameof(CheckListViews))!;
      set => base.UpdateProperty(nameof(CheckListViews), value);
    }

    public CheckSet CheckSet
    {
      get => base.GetProperty<CheckSet>(nameof(CheckSet))!;
      set => base.UpdateProperty(nameof(CheckSet), value);
    }

    public BindingList<BindingKeyValue<string, double>> PropertyValuesView
    {
      get => base.GetProperty<BindingList<BindingKeyValue<string, double>>>(nameof(PropertyValuesView))!;
      set => base.UpdateProperty(nameof(PropertyValuesView), value);
    }

    public BindingList<StateCheckEvaluator.RecentResult> EvaluatorRecentResultView
    {
      get => base.GetProperty<BindingList<StateCheckEvaluator.RecentResult>>(nameof(EvaluatorRecentResultView))!;
      set => base.UpdateProperty(nameof(EvaluatorRecentResultView), value);
    }

    #endregion Public Properties

    #region Public Constructors

    public RunContext(InitContext initContext)
    {
      this.CheckSet = initContext.ChecklistSet;
      this.settings = initContext.Settings;
      this.logger = Logger.Create(this, "CheckList.RunContext");

      this.CheckListViews = CheckSet.Checklists
        .Select(q => new CheckListView()
        {
          CheckList = q,
          State = RunState.NotYet,
          Items = q.Items
            .Select(p => new CheckItemView()
            {
              CheckItem = p,
              State = RunState.NotYet
            })
            .ToList()
        })
        .ToList();

      var allProps = initContext.SimPropertyGroup.GetAllSimPropertiesRecursively()
        .Where(q => initContext.PropertyUsageCounts.Any(p => p.Property == q))
        .ToList();

      this.simObject = SimObject.GetInstance();
      this.simObject.SimSecondElapsed += SimObject_SimSecondElapsed;
      this.simObject.Started += SimObject_Started;
      this.simObject.Started += () => this.simObject.RegisterProperties(allProps);
      this.simObject.SimPropertyChanged += SimObject_SimPropertyChanged;
      this.EvaluatorRecentResultView = new();

      this.PropertyValuesView = new(
        allProps
        .Select(q => new BindingKeyValue<string, double>(q.Name, double.NaN))
        .OrderBy(q => q.Key)
        .ToList());

      this.manager = new ChecklistManager(this.CheckListViews, this.simObject,
        this.settings.UseAutoplay, this.settings.ReadConfirmations,
        () => variableValues); // CheckListViews must be set before calling this
    }

    #endregion Public Constructors

    #region Internal Methods

    internal void Run(KeyHookWrapper keyHookWrapper)
    {
      logger?.Invoke(LogLevel.INFO, "Run");

      logger?.Invoke(LogLevel.VERBOSE, "Resetting playback");
      manager.Reset();

      logger?.Invoke(LogLevel.VERBOSE, "Adding key hooks");
      this.keyHookWrapper = keyHookWrapper ?? throw new ArgumentNullException(nameof(keyHookWrapper));
      ConnectKeyHooks();

      logger?.Invoke(LogLevel.VERBOSE, "Starting simObject connection");
      this.simObject.StartAsync();

      logger?.Invoke(LogLevel.VERBOSE, "Run done");
    }

    internal void Stop()
    {
      throw new NotImplementedException();
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
        logger?.Invoke(LogLevel.INFO, "Assigning play-pause keyboard shortcut " + s);
        this.keyHookPlayPauseId = this.keyHookWrapper.RegisterKeyHook(ConvertShortcutToKeyHookInfo(s));
      }
      catch (Exception ex)
      {
        logger?.Invoke(LogLevel.ERROR, $"Failed to bind key-hook for shortcut {s}. Reason: {ex.GetFullMessage()}");
      }

      try
      {
        s = settings.Shortcuts.SkipToNext;
        logger?.Invoke(LogLevel.INFO, "Assigning skip-to-next keyboard shortcut " + s);
        this.keyHookSkipNextId = this.keyHookWrapper.RegisterKeyHook(ConvertShortcutToKeyHookInfo(s));
      }
      catch (Exception ex)
      {
        logger?.Invoke(LogLevel.ERROR, $"Failed to bind key-hook for shortcut {s}. Reason: {ex.GetFullMessage()}");
      }

      try
      {
        s = settings.Shortcuts.SkipToPrevious;
        logger?.Invoke(LogLevel.INFO, "Assigning skip-to-previous keyboard shortcut " + s);
        this.keyHookSkipPrevId = this.keyHookWrapper.RegisterKeyHook(ConvertShortcutToKeyHookInfo(s));
      }
      catch (Exception ex)
      {
        logger?.Invoke(LogLevel.ERROR, $"Failed to bind key-hook for shortcut {s}. Reason: {ex.GetFullMessage()}");
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
      this.propertyValues[property.Name] = value;
      this.PropertyValuesView.Where(q => q.Key == property.Name).ForEach(q => q.Value = value);
    }

    private void SimObject_SimSecondElapsed()
    {
      this.manager.CheckForActiveChecklistsIfRequired();
      this.EvaluatorRecentResultView = new(this.manager.GetRecentResults());
    }

    private void SimObject_Started()
    {
      // what to put here?
    }

    #endregion Private Methods
  }
}
