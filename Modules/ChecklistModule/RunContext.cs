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

namespace Eng.Chlaot.Modules.ChecklistModule
{
  public partial class RunContext : NotifyPropertyChangedBase
  {
    private readonly AutoplayChecklistEvaluator autoplayEvaluator;
    private readonly Logger logger;
    private readonly PlaybackManager playback;
    private readonly Settings settings;
    private readonly SimObject simObject;

    private int keyHookPlayPauseId = -1;
    private int keyHookSkipNextId = -1;
    private int keyHookSkipPrevId = -1;

    private readonly Dictionary<string, double> propertyValues = new();
    private readonly Dictionary<string, double> variableValues = new();
    public BindingList<PropertyValue> PropertyValues
    {
      get => base.GetProperty<BindingList<PropertyValue>>(nameof(PropertyValues))!;
      set => base.UpdateProperty(nameof(PropertyValues), value);
    }

    private KeyHookWrapper? keyHookWrapper;

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

      this.playback = new PlaybackManager(this);
      this.simObject = SimObject.GetInstance();
      this.simObject.SimSecondElapsed += SimObject_SimSecondElapsed;
      this.simObject.Started += SimObject_Started;
      this.simObject.Started += () => this.simObject.RegisterProperties(allProps);
      this.simObject.SimPropertyChanged += SimObject_SimPropertyChanged;
      this.autoplayEvaluator = new AutoplayChecklistEvaluator(this);

      this.PropertyValues = new BindingList<PropertyValue>(
        allProps
        .Select(q => new PropertyValue(q.Name, double.NaN))
        .OrderBy(q => q.Name)
        .ToList());
    }

    private void SimObject_SimPropertyChanged(SimProperty property, double value)
    {
      this.PropertyValues.Where(q => q.Name == property.Name).ForEach(q => q.Value = value);
    }

    private void SimObject_Started()
    {
      // what to put here?
    }

    internal void Run(KeyHookWrapper keyHookWrapper)
    {
      logger?.Invoke(LogLevel.INFO, "Run");

      logger?.Invoke(LogLevel.VERBOSE, "Resetting playback");
      playback.Reset();

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
        this.playback.TogglePlay();
      }
      else if (hookId == this.keyHookSkipNextId)
      {
        this.playback.SkipToNext();
      }
      else if (hookId == this.keyHookSkipPrevId)
      {
        this.playback.SkipToPrev();
      }
      else
      {
        throw new ApplicationException($"Unknown hook id '{hookId}'.");
      }
    }

    private void Log(LogLevel level, string message)
    {
      logger.Invoke(level, message);
    }

    private void SimObject_SimSecondElapsed()
    {
      if (playback.IsWaitingForNextChecklist == false) return;
      CheckList checkList = playback.GetCurrentChecklist();
      UpdatePropertyValues();
      bool shouldPlay = this.settings.UseAutoplay
        && this.autoplayEvaluator.EvaluateIfShouldPlay(checkList);
      if (shouldPlay)
      {
        autoplayEvaluator.SuppressAutoplayForCurrentChecklist();
        this.playback.TogglePlay();
      }
    }

    private void UpdatePropertyValues()
    {
      StateCheckEvaluator.UpdateDictionaryBySimObject(this.simObject, propertyValues);
    }
  }
}
