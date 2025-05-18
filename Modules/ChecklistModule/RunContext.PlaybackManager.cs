using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.AudioPlaying;
using Eng.EFsExtensions.Modules.ChecklistModule.Types.VM;
using ESystem.Asserting;
using System;
using System.Timers;

namespace Eng.EFsExtensions.Modules.ChecklistModule
{
  internal partial class RunContext
  {
    internal class PlaybackManagerSettings
    {
      public bool readConfirmations;
      public int? pausedAlertIntervalIfUsed;
      public bool isPlayPerItemEnabled;
    }
    private class PlaybackManagerState
    {
      public bool isCallPlayed;
      public bool isEntryPlayed;
      public bool isMainLoopActive = false;
      public int currentItemIndex = 0;
      public bool isMainLoopAbortRequested = false;
      public bool isCurrentLastSpeechPlaying = false;
    }

    internal class PlaybackManager
    {
      private readonly PlaybackManagerSettings sett;
      private readonly PlaybackManagerState state = new();
      private readonly AudioPlayManager apm = AudioPlayManagerProvider.Instance;
      private Timer? pendingChecklistTimer = null;

      public event Action? ChecklistPlayingCompleted;
      public bool IsWaitingForNextChecklist { get => state.currentItemIndex == 0 && state.isEntryPlayed == false; }
      public bool IsPartlyPlayed => state.currentItemIndex > 0;

      internal PlaybackManager(CheckListVM initialChecklist, PlaybackManagerSettings settings)
      {
        EAssert.Argument.IsNotNull(initialChecklist, nameof(initialChecklist));

        this.sett = settings;
        this.Current = initialChecklist;
        this.SetCurrent(this.Current); // ensures correct initialization
      }

      public CheckListVM Current { get; private set; }
      public bool IsPlaying { get => this.state.isMainLoopActive; }

      public void SetCurrent(CheckListVM value)
      {
        EAssert.Argument.IsNotNull(value, nameof(value));
        // reset old one
        Current.RunTime.State = RunState.Runned;
        Current.RunTime.CanBeAutoplayed = true;

        // setting new one
        Current = value;
        Current.RunTime.State = RunState.Current;
        Current.Items.ForEach(q => q.RunTime.State = RunState.NotYet);
        state.currentItemIndex = 0;
        state.isEntryPlayed = false;
        state.isCallPlayed = false;
      }

      public void PauseAsync()
      {
        lock (this)
        {
          if (this.state.isCurrentLastSpeechPlaying == false)
          {
            this.state.isCallPlayed = false;
            this.state.isEntryPlayed = false;
            this.state.isMainLoopAbortRequested = true;
          }
          else
          {
            // if this is last speech, let next speech is played
            // ... this causes switch to the next checklist
          }
        }
      }

      private void PlayNext()
      {
        EAssert.IsTrue(state.isMainLoopActive);
        EAssert.IsFalse(state.isMainLoopAbortRequested);
        lock (this)
        {
          byte[] playData = ResolveAndMarkNexPlayBytes(out state.isCurrentLastSpeechPlaying, out bool isOneChecklistItemCompleted);
          if (!state.isCurrentLastSpeechPlaying && this.sett.isPlayPerItemEnabled && isOneChecklistItemCompleted)
            state.isMainLoopAbortRequested = true;

          apm.Enqueue(playData, EFsExtensionsModuleBase.ModuleUtils.AudioPlaying.AudioPlayManager.CHANNEL_COPILOT, OnPlayCompleted);
        }
        AdjustRunStates();
      }

      private void OnPlayCompleted()
      {
        lock (this)
        {
          if (state.isMainLoopActive)
          {
            if (state.isMainLoopAbortRequested)
            {
              state.isMainLoopActive = false;
              state.isMainLoopAbortRequested = false;
              this.EnablePendingChecklistTimer();
            }
            else if (state.isCurrentLastSpeechPlaying)
            {
              state.isMainLoopActive = false;
              state.isMainLoopAbortRequested = false;
              state.isCurrentLastSpeechPlaying = false;
              this.ChecklistPlayingCompleted?.Invoke();
            }
            else
              this.PlayNext();
          }
        }
      }

      private void PendingChecklistTimer_Elapsed(object? sender, ElapsedEventArgs e)
      {
        byte[] playData = this.Current.CheckList.PausedAlertSpeechBytes;
        apm.Enqueue(playData, EFsExtensionsModuleBase.ModuleUtils.AudioPlaying.AudioPlayManager.CHANNEL_COPILOT);
      }

      public void Reset()
      {
        this.state.currentItemIndex = 0;
        this.state.isCallPlayed = false;
        this.state.isEntryPlayed = false;
        this.Current.Items.ForEach(q => q.RunTime.State = RunState.NotYet);
        this.DisablePendingChecklistTimer();
      }

      private void DisablePendingChecklistTimer()
      {
        if (this.pendingChecklistTimer != null)
        {
          // TODO can here be multithread issue?
          this.pendingChecklistTimer.Stop();
          this.pendingChecklistTimer = null;
        }
      }

      private void EnablePendingChecklistTimer()
      {
        if (this.sett.pausedAlertIntervalIfUsed == null) return;
        if (this.pendingChecklistTimer != null) return;
        // TODO can here be multithread issue?
        this.pendingChecklistTimer = new Timer(this.sett.pausedAlertIntervalIfUsed.Value)
        {
          AutoReset = true
        };
        this.pendingChecklistTimer.Elapsed += PendingChecklistTimer_Elapsed;
        this.pendingChecklistTimer.Start();
      }

      public void Play()
      {
        lock (this)
        {
          if (!state.isMainLoopActive)
          {
            this.DisablePendingChecklistTimer();
            this.Current.RunTime.CanBeAutoplayed = false;
            this.state.isMainLoopActive = true;
            PlayNext();
          }
        }
      }

      public void TogglePlay()
      {
        lock (this)
        {
          if (state.isMainLoopActive)
            PauseAsync();
          else
            Play();
        }
      }

      private void AdjustRunStates()
      {
        for (int i = 0; i < Current.Items.Count; i++)
        {
          if (i < state.currentItemIndex)
            Current.Items[i].RunTime.State = RunState.Runned;
          else if (i > state.currentItemIndex)
            Current.Items[i].RunTime.State = RunState.NotYet;
          else
            Current.Items[i].RunTime.State = RunState.Current;
        }
      }

      private byte[] ResolveAndMarkNexPlayBytes(out bool isThisLastChecklistSpeech, out bool isOneChecklistItemCompleted)
      {
        byte[] ret;
        if (state.currentItemIndex == 0 && !this.state.isEntryPlayed)
        {
          // playing checklist entry speech
          ret = Current.CheckList.EntrySpeechBytes;
          this.state.isEntryPlayed = true;
          isThisLastChecklistSpeech = false;
          isOneChecklistItemCompleted = false;
        }
        else if (state.currentItemIndex < Current.Items.Count)
        {
          // play checklist item and increase index
          if (this.state.isCallPlayed == false)
          {
            ret = Current.Items[state.currentItemIndex].CheckItem.Call.Bytes;
            this.state.isCallPlayed = true;

            if (!this.sett.readConfirmations)
            {
              state.currentItemIndex++;
              this.state.isCallPlayed = false;
              isOneChecklistItemCompleted = state.currentItemIndex != Current.Items.Count;
            }
            isOneChecklistItemCompleted = false;
          }
          else
          {
            ret = Current.Items[state.currentItemIndex].CheckItem.Confirmation.Bytes;
            state.currentItemIndex++;
            this.state.isCallPlayed = false;
            isOneChecklistItemCompleted = state.currentItemIndex != Current.Items.Count;
          }
          isThisLastChecklistSpeech = false;
        }
        else
        {
          // playing at the end
          ret = Current.CheckList.ExitSpeechBytes;
          isThisLastChecklistSpeech = true;
          isOneChecklistItemCompleted = true;
        }
        return ret;
      }
    }
  }
}
