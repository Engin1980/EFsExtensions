using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.AudioPlaying;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.AudioPlaying;
using Eng.Chlaot.Modules.ChecklistModule.Types.VM;
using ESystem.Asserting;
using System;
using System.Timers;

namespace Eng.Chlaot.Modules.ChecklistModule
{
  internal partial class RunContext
  {
    //TODO this class is somehow dulpicit with ChlaotModuleBase.Audio.AutoPlaybackManager?
    internal class PlaybackManager
    {
      private int currentItemIndex = 0;
      private bool isCallPlayed = false;
      private bool isEntryPlayed = false;
      private bool isMainLoopAbortRequested = false;
      private bool isMainLoopActive = false;
      private bool isCurrentLastSpeechPlaying = false;
      private Timer? pendingChecklistTimer = null;
      private readonly bool readConfirmations;
      private readonly int? pausedAlertIntervalIfUsed;
      public event Action? ChecklistPlayingCompleted;
      public bool IsWaitingForNextChecklist { get => currentItemIndex == 0 && isEntryPlayed == false; }
      public bool IsPartlyPlayed => currentItemIndex > 0;

      internal PlaybackManager(CheckListVM initialChecklist, bool readConfirmations, int? pausedAlertIntervalIfUsed)
      {
        EAssert.Argument.IsNotNull(initialChecklist, nameof(initialChecklist));
        this.readConfirmations = readConfirmations;
        this.pausedAlertIntervalIfUsed = pausedAlertIntervalIfUsed;
        this.Current = initialChecklist;
        this.SetCurrent(this.Current); // ensures correct initialization
      }

      public CheckListVM Current { get; private set; }
      public bool IsPlaying { get => this.isMainLoopActive; }

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
        currentItemIndex = 0;
        isEntryPlayed = false;
        isCallPlayed = false;
      }

      public void PauseAsync()
      {
        lock (this)
        {
          if (this.isCurrentLastSpeechPlaying == false)
          {
            this.isCallPlayed = false;
            this.isEntryPlayed = false;
            this.isMainLoopAbortRequested = true;
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
        EAssert.IsTrue(isMainLoopActive);
        EAssert.IsFalse(isMainLoopAbortRequested);
        lock (this)
        {
          byte[] playData = ResolveAndMarkNexPlayBytes(out isCurrentLastSpeechPlaying);
          AudioPlayer player = new(playData);
          player.PlayCompleted += Player_PlayCompleted;
          player.Play();
        }
        AdjustRunStates();
      }

      private void Player_PlayCompleted(AudioPlayer sender)
      {
        lock (this)
        {
          if (isMainLoopActive)
          {
            if (isMainLoopAbortRequested)
            {
              isMainLoopActive = false;
              isMainLoopAbortRequested = false;
              this.EnablePendingChecklistTimer();
            }
            else if (isCurrentLastSpeechPlaying)
            {
              isMainLoopActive = false;
              isMainLoopAbortRequested = false;
              isCurrentLastSpeechPlaying = false;
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
        AudioPlayer player = new(playData);
        player.PlayAsync();
      }

      public void Reset()
      {
        this.currentItemIndex = 0;
        this.isCallPlayed = false;
        this.isEntryPlayed = false;
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
        if (this.pausedAlertIntervalIfUsed == null) return;
        if (this.pendingChecklistTimer != null) return;
        // TODO can here be multithread issue?
        this.pendingChecklistTimer = new Timer(this.pausedAlertIntervalIfUsed.Value)
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
          if (!isMainLoopActive)
          {
            this.DisablePendingChecklistTimer();
            this.Current.RunTime.CanBeAutoplayed = false;
            this.isMainLoopActive = true;
            PlayNext();
          }
        }
      }

      public void TogglePlay()
      {
        lock (this)
        {
          if (isMainLoopActive)
            PauseAsync();
          else
            Play();
        }
      }

      private void AdjustRunStates()
      {
        for (int i = 0; i < Current.Items.Count; i++)
        {
          if (i < currentItemIndex)
            Current.Items[i].RunTime.State = RunState.Runned;
          else if (i > currentItemIndex)
            Current.Items[i].RunTime.State = RunState.NotYet;
          else
            Current.Items[i].RunTime.State = RunState.Current;
        }
      }

      private byte[] ResolveAndMarkNexPlayBytes(out bool isThisLastChecklistSpeech)
      {
        byte[] ret;
        if (currentItemIndex == 0 && !this.isEntryPlayed)
        {
          // playing checklist entry speech
          ret = Current.CheckList.EntrySpeechBytes;
          this.isEntryPlayed = true;
          isThisLastChecklistSpeech = false;
        }
        else if (currentItemIndex < Current.Items.Count)
        {
          // play checklist item and increase index
          if (isCallPlayed == false)
          {
            ret = Current.Items[currentItemIndex].CheckItem.Call.Bytes;
            isCallPlayed = true;

            if (!this.readConfirmations)
            {
              currentItemIndex++;
              isCallPlayed = false;
            }
          }
          else
          {
            ret = Current.Items[currentItemIndex].CheckItem.Confirmation.Bytes;
            currentItemIndex++;
            isCallPlayed = false;
          }
          isThisLastChecklistSpeech = false;
        }
        else
        {
          // playing at the end
          ret = Current.CheckList.ExitSpeechBytes;
          isThisLastChecklistSpeech = true;
        }
        return ret;
      }
    }
  }
}
