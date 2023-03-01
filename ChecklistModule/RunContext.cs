using ChecklistModule.Support;
using ChecklistModule.Types;
using ChecklistModule.Types.Autostarts;
using ChecklistModule.Types.RunViews;
using ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace ChecklistModule
{
  public class RunContext : NotifyPropertyChangedBase
  {
    public class AutoplayChecklistEvaluator
    {
      private readonly Dictionary<AutostartDelay, int> historyCounter = new();

      private readonly object lck = new();

      private readonly RunContext parent;

      private readonly SimData simData;

      private bool autoplaySuppressed = false;

      private CheckList? prevList = null;

      public AutoplayChecklistEvaluator(RunContext parent)
      {
        this.parent = parent;
        this.simData = parent.sim.SimData;
      }
      public bool EvaluateIfShouldPlay(CheckList checkList)
      {
        if (this.simData.IsSimPaused) return false;
        if (Monitor.TryEnter(lck) == false) return false;

        Log($"Evaluation started for {checkList.Id}");

        if (prevList != checkList)
        {
          this.autoplaySuppressed = false;
          historyCounter.Clear();
          prevList = checkList;
        }

        bool ret;
        if (this.autoplaySuppressed)
          ret = false;
        else
          ret = checkList.MetaInfo?.Autostart != null && Evaluate(checkList.MetaInfo.Autostart);

        Log($"Evaluation finished for {checkList.Id} as={ret}, autoplaySupressed={autoplaySuppressed}");
        Monitor.Exit(lck);
        return ret;
      }

      internal void SuppressAutoplayForCurrentChecklist()
      {
        this.autoplaySuppressed = true;
      }

      private static int ResolveEngineStarted(AutostartProperty property, SimData sd)
      {
        Trace.Assert(property.Name == AutostartPropertyName.EngineStarted);
        int index = property.NameIndex - 1;
        int ret = sd.EngineCombustion[index] ? 1 : 0;
        return ret;
      }

      private bool Evaluate(IAutostart autostart)
      {
        var ret = autostart switch
        {
          AutostartCondition condition => EvaluateCondition(condition),
          AutostartDelay delay => EvaluateDelay(delay),
          AutostartProperty property => EvaluateProperty(property),
          _ => throw new NotImplementedException(),
        };
        return ret;
      }

      private bool EvaluateCondition(AutostartCondition condition)
      {
        List<bool> subs = condition.Items.Select(q => Evaluate(q)).ToList();
        var ret = condition.Operator switch
        {
          AutostartConditionOperator.Or => subs.Any(q => q),
          AutostartConditionOperator.And => subs.All(q => q),
          _ => throw new NotImplementedException(),
        };

        Log($"Eval {condition.DisplayString} = {ret} (datas = {string.Join(";", subs)})");
        return ret;
      }

      private bool EvaluateDelay(AutostartDelay delay)
      {
        bool ret;
        bool tmp = Evaluate(delay.Item);
        if (tmp)
        {
          if (historyCounter.ContainsKey(delay))
            historyCounter[delay]++;
          else
            historyCounter[delay] = 1;
        }
        else
          historyCounter[delay] = 0;

        ret = historyCounter[delay] >= delay.Seconds;

        Log($"Eval {delay.DisplayString} = {ret} (delay = {historyCounter[delay]})");

        return ret;
      }

      private bool EvaluateProperty(AutostartProperty property)
      {
        SimData sd = this.simData ?? throw new ApplicationException("Not expecting simData to be null here.");
        double expected = property.Value;
        double actual = property.Name switch
        {
          AutostartPropertyName.Altitude => sd.Altitude,
          AutostartPropertyName.IAS => sd.IndicatedSpeed,
          AutostartPropertyName.GS => sd.GroundSpeed,
          AutostartPropertyName.Height => sd.Height,
          AutostartPropertyName.Bank => sd.BankAngle,
          AutostartPropertyName.ParkingBrakeSet => sd.ParkingBrakeSet ? 1 : 0,
          AutostartPropertyName.VerticalSpeed => sd.VerticalSpeed,
          AutostartPropertyName.PushbackTugConnected => sd.PushbackTugConnected ? 1 : 0,
          AutostartPropertyName.Acceleration => sd.Acceleration,
          AutostartPropertyName.EngineStarted => ResolveEngineStarted(property, sd),
          _ => throw new NotImplementedException()
        };

        bool ret = property.Direction switch
        {
          AutostartPropertyDirection.Above => actual > expected,
          AutostartPropertyDirection.Below => actual < expected,
          _ => throw new NotImplementedException()
        };

        Log($"Eval {property.DisplayString} = {ret} (actual = {actual})");
        return ret;
      }

      private void Log(string message)
      {
        if (parent.settings.VerboseAutostartEvaluation)
          this.parent.Log(LogLevel.INFO, message);
        try
        {
          System.IO.File.AppendAllText("innerlog.txt",
            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} :: {message}\n");
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Unable to write to inner log.", ex);
        }
      }
    }
    public class PlaybackManager
    {
      private readonly RunContext parent;
      private int currentItemIndex = 0;
      private CheckListView currentList;
      private bool isCallPlayed = false;
      private bool isEntryPlayed = false;
      private bool isPlaying = false;
      private CheckListView? previousList;
      public bool IsWaitingForNextChecklist { get => currentItemIndex == 0 && isEntryPlayed == false; }

      public PlaybackManager(RunContext parent)
      {
        this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
      }

      public CheckList GetCurrentChecklist() => currentList.CheckList;

      public void PauseAsync()
      {
        this.isPlaying = false;
        this.isCallPlayed = false;
        this.isEntryPlayed = false;
      }

      public void Play()
      {
        lock (this)
        {
          this.isPlaying = true;
          byte[] playData = ResolveAndMarkNexPlayBytes(out bool stopPlaying);
          InternalPlayer player = new(playData);
          player.PlaybackFinished += Player_PlaybackFinished;
          player.PlayAsync();
          if (stopPlaying) this.isPlaying = false;
        }
        AdjustRunStates();
      }

      public void Reset()
      {
        this.currentList = parent.CheckListViews.First();
        this.currentItemIndex = 0;
        this.AdjustRunStates();
      }

      public void TogglePlay()
      {
        if (isPlaying)
          PauseAsync();
        else
          Play();
      }

      internal void SkipToNext()
      {
        currentList = parent.CheckListViews.First(q => q.CheckList == currentList.CheckList.NextChecklist);
        currentItemIndex = 0;
        isEntryPlayed = false;
        isCallPlayed = false;
        AdjustRunStates();
        if (!isPlaying) this.Play();
      }

      internal void SkipToPrev()
      {
        if (isEntryPlayed || currentItemIndex > 0)
        {
          currentItemIndex = 0;
          isCallPlayed = false;
          isEntryPlayed = false;
        }
        else if (previousList != null)
        {
          currentList = previousList;
          previousList = null;
        }
        AdjustRunStates();
        if (!isPlaying) this.Play();
      }

      private void AdjustRunStates()
      {
        parent.CheckListViews
          .Where(q => q.State == RunState.Current && q != currentList)
          .ToList()
          .ForEach(q => q.State = RunState.Runned);
        for (int i = 0; i < currentList.Items.Count; i++)
        {
          if (i < currentItemIndex)
            currentList.Items[i].State = RunState.Runned;
          else if (i > currentItemIndex)
            currentList.Items[i].State = RunState.NotYet;
          else
            currentList.Items[i].State = RunState.Current;
        }
        if (currentList.State != RunState.Current)
          currentList.State = RunState.Current;
        if (currentItemIndex < currentList.Items.Count)
          currentList.Items[currentItemIndex].State = RunState.Current;
      }
      private void Player_PlaybackFinished(InternalPlayer sender)
      {
        lock (this)
        {
          if (!this.isPlaying) return;
          this.Play();
        }
      }

      private byte[] ResolveAndMarkNexPlayBytes(out bool stopPlaying)
      {
        byte[] ret;
        if (currentItemIndex == 0 && !this.isEntryPlayed)
        {
          // playing checklist entry speech
          ret = currentList.CheckList.EntrySpeechBytes;
          this.isEntryPlayed = true;
          stopPlaying = false;
        }
        else if (currentItemIndex < currentList.Items.Count)
        {
          // play checklist item and increase index
          if (isCallPlayed == false)
          {
            ret = currentList.Items[currentItemIndex].CheckItem.Call.Bytes;
            isCallPlayed = true;

            if (!this.parent.settings.ReadConfirmations)
            {
              currentItemIndex++;
              isCallPlayed = false;
            }
          }
          else
          {
            ret = currentList.Items[currentItemIndex].CheckItem.Confirmation.Bytes;
            currentItemIndex++;
            isCallPlayed = false;
          }
          stopPlaying = false;
        }
        else
        {
          // playing at the end
          ret = currentList.CheckList.ExitSpeechBytes;
          this.isEntryPlayed = false;
          previousList = currentList;
          currentList = parent.CheckListViews.First(q => q.CheckList == currentList.CheckList.NextChecklist);
          currentItemIndex = 0;
          isCallPlayed = false;
          stopPlaying = true;
        }
        return ret;
      }
    }

    private const int INITIAL_CONNECTION_TIMER_INTERVAL = 2000;
    private const int REPEATED_CONNECTION_TIMER_INTERVAL = 10000;

    private readonly AutoplayChecklistEvaluator autoplayEvaluator;

    private readonly LogHandler logHandler;

    private readonly PlaybackManager playback;

    private readonly Settings settings;

    private readonly Sim sim;

    private System.Timers.Timer? connectionTimer = null;

    private int keyHookPlayPauseId = -1;

    private int keyHookSkipNextId = -1;

    private int keyHookSkipPrevId = -1;

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

    public SimData SimData
    {
      get => base.GetProperty<SimData>(nameof(SimData))!;
      set => base.UpdateProperty(nameof(SimData), value);
    }
    public RunContext(InitContext initContext, LogHandler logHandler)
    {
      this.CheckSet = initContext.ChecklistSet;
      this.settings = initContext.Settings;
      this.logHandler = logHandler ?? throw new ArgumentNullException(nameof(logHandler));

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

      this.playback = new PlaybackManager(this);
      this.sim = new(logHandler, settings.LogSimConnectToFile);
      this.sim.SimSecondElapsed += Sim_SimSecondElapsed;
      this.SimData = this.sim.SimData;
      this.autoplayEvaluator = new(this);
    }

    internal void Run(KeyHookWrapper keyHookWrapper)
    {
      logHandler?.Invoke(LogLevel.VERBOSE, "Run");

      logHandler?.Invoke(LogLevel.VERBOSE, "Resetting playback");
      playback.Reset();

      logHandler?.Invoke(LogLevel.VERBOSE, "Adding key hooks");
      this.keyHookWrapper = keyHookWrapper ?? throw new ArgumentNullException(nameof(keyHookWrapper));
      ConnectKeyHooks();

      logHandler?.Invoke(LogLevel.VERBOSE, "Starting connection timer");
      this.connectionTimer = new System.Timers.Timer(INITIAL_CONNECTION_TIMER_INTERVAL)
      {
        AutoReset = true,
        Enabled = true
      };
      this.connectionTimer.Elapsed += ConnectionTimer_Elapsed;

      logHandler?.Invoke(LogLevel.VERBOSE, "Run done");
    }

    internal void Stop()
    {
      this.sim?.Close();
    }

    private static KeyHookWrapper.KeyHookInfo ConvertShortcutToKeyHookInfo(Settings.KeyShortcut shortcut)
    {
      KeyHookWrapper.KeyHookInfo ret = new(
        shortcut.Alt,
        shortcut.Control,
        shortcut.Shift,
        shortcut.Key);
      return ret;
    }

    private void ConnectionTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
      if (this.connectionTimer!.Interval == INITIAL_CONNECTION_TIMER_INTERVAL)
        this.connectionTimer!.Interval = REPEATED_CONNECTION_TIMER_INTERVAL;
      try
      {
        Log(LogLevel.VERBOSE, "Opening connection");
        sim.Open();
        Log(LogLevel.VERBOSE, "Opening connection - done");
        this.connectionTimer!.Stop();
        this.connectionTimer = null;

        this.sim.Start();
        Log(LogLevel.INFO, "Connected to FS2020, starting updates");
      }
      catch (Exception ex)
      {
        Log(LogLevel.WARNING, "Failed to connect to FS2020, will try it again in a few seconds...");
        Log(LogLevel.WARNING, "Fail reason: " + ex.GetFullMessage());
      }
    }

    private void ConnectKeyHooks()
    {
      Settings.KeyShortcut s = null;

      if (keyHookWrapper == null) throw new ApplicationException("KeyHookWrapper not set.");

      try
      {
        s = settings.Shortcuts.PlayPause;
        logHandler?.Invoke(LogLevel.INFO, "Assigning play-pause keyboard shortcut " + s);
        this.keyHookPlayPauseId = this.keyHookWrapper.RegisterKeyHook(ConvertShortcutToKeyHookInfo(s));
      }
      catch (Exception ex)
      {
        logHandler?.Invoke(LogLevel.ERROR, $"Failed to bind key-hook for shortcut {s}. Reason: {ex.GetFullMessage()}");
      }

      try
      {
        s = settings.Shortcuts.SkipToNext;
        logHandler?.Invoke(LogLevel.INFO, "Assigning skip-to-next keyboard shortcut " + s);
        this.keyHookSkipNextId = this.keyHookWrapper.RegisterKeyHook(ConvertShortcutToKeyHookInfo(s));
      }
      catch (Exception ex)
      {
        logHandler?.Invoke(LogLevel.ERROR, $"Failed to bind key-hook for shortcut {s}. Reason: {ex.GetFullMessage()}");
      }

      try
      {
        s = settings.Shortcuts.SkipToPrevious;
        logHandler?.Invoke(LogLevel.INFO, "Assigning skip-to-previous keyboard shortcut " + s);
        this.keyHookSkipPrevId = this.keyHookWrapper.RegisterKeyHook(ConvertShortcutToKeyHookInfo(s));
      }
      catch (Exception ex)
      {
        logHandler?.Invoke(LogLevel.ERROR, $"Failed to bind key-hook for shortcut {s}. Reason: {ex.GetFullMessage()}");
      }

      this.keyHookWrapper.KeyHookInvoked += keyHookWrapper_KeyHookInvoked;
    }

    [SuppressMessage("", "IDE1006")]
    private void keyHookWrapper_KeyHookInvoked(int hookId, KeyHookWrapper.KeyHookInfo keyHookInfo)
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
      logHandler?.Invoke(level, "[RunContext] :: " + message);
    }

    private void Sim_SimSecondElapsed()
    {
      if (this.sim.SimData.IsSimPaused) return;

      if (playback.IsWaitingForNextChecklist == false) return;
      CheckList checkList = playback.GetCurrentChecklist();
      bool shouldPlay = this.settings.UseAutoplay
        && autoplayEvaluator.EvaluateIfShouldPlay(checkList);
      if (shouldPlay)
      {
        autoplayEvaluator.SuppressAutoplayForCurrentChecklist();
        this.playback.TogglePlay();
      }
    }
  }
}
