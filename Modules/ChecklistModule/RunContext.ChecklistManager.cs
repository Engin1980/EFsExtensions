using ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimObjects;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.Modules.ChecklistModule.Types.VM;
using ESystem.Asserting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Eng.Chlaot.Modules.ChecklistModule
{
  public partial class RunContext
  {
    public class ChecklistManager
    {
      private readonly PlaybackManager playbackManager;
      private CheckListVM? previous;
      private CheckListVM current;
      private readonly List<CheckListVM> active = new();
      private readonly List<CheckListVM> all;
      private readonly bool isAutoplayingEnabled;
      private readonly SimObject simObject;
      private readonly Dictionary<string, double> propertyValues = new();

      public ChecklistManager(List<CheckListVM> checkListViews, SimObject simObject,
        bool useAutoplay, bool readConfirmations)
      {
        EAssert.Argument.IsNotNull(checkListViews, nameof(checkListViews));
        EAssert.Argument.IsNotNull(simObject, nameof(simObject));

        this.all = checkListViews;
        this.all.ForEach(q => q.CreateRuntime(() => this.propertyValues));
        this.current = checkListViews.First();
        this.active.Add(current);
        this.all.ForEach(q => q.RunTime.IsActive = active.Contains(q));

        this.isAutoplayingEnabled = useAutoplay;
        this.simObject = simObject;

        this.playbackManager = new(this.current, readConfirmations);
        this.playbackManager.ChecklistPlayingCompleted += PlaybackManager_ChecklistPlayingCompleted;
      }

      private void PlaybackManager_ChecklistPlayingCompleted()
      {
        this.previous = this.current;
        var nextActiveViews = all.Where(q => this.current.CheckList.NextChecklists.Contains(q.CheckList));
        EAssert.IsTrue(nextActiveViews.Any(), "There must be at least one next checklist.");
        this.active.Clear();
        this.active.AddRange(nextActiveViews);
        this.all.ForEach(q => q.RunTime.IsActive = active.Contains(q));
        nextActiveViews.ForEach(q => q.RunTime.ResetEvaluator());
        this.current = nextActiveViews.First();
        this.playbackManager.SetCurrent(this.current);
      }

      public void Reset()
      {
        this.current.RunTime.ResetEvaluator();
        this.playbackManager.Reset();
      }

      internal void SkipToNext()
      {
        var nextActive = current.CheckList.NextChecklists;
        var nextActiveViews = this.all.Where(q => nextActive.Contains(q.CheckList));
        this.active.Clear();
        this.active.AddRange(nextActiveViews);
        this.all.ForEach(q => q.RunTime.IsActive = active.Contains(q));
        nextActiveViews.ForEach(q => q.RunTime.ResetEvaluator());

        this.previous = this.current;
        this.current = nextActiveViews.First();
        this.playbackManager.SetCurrent(this.current);
        this.playbackManager.Play();
      }

      internal void SkipToPrev()
      {
        if (playbackManager.IsPartlyPlayed)
          playbackManager.Reset();
        else if (this.previous != null)
        {
          this.current = this.previous;
          playbackManager.SetCurrent(this.previous);
          this.active.Clear();
          this.active.Add(this.previous);
          this.all.ForEach(q => q.RunTime.IsActive = active.Contains(q));
          this.previous.RunTime.ResetEvaluator();
          this.previous = this.all.FirstOrDefault(q => q.CheckList.NextChecklists.First() == this.previous.CheckList); // tries to get previous;
        }
        this.playbackManager.Play();
      }

      internal void TogglePlay()
      {
        this.playbackManager.TogglePlay();
      }

      internal void CheckForActiveChecklistsIfRequired()
      {
        if (playbackManager.IsPlaying) return;
        if (!isAutoplayingEnabled) return;
        if (simObject.IsSimPaused) return;

        StateCheckEvaluator.UpdateDictionaryBySimObject(this.simObject, this.propertyValues);

        CheckListVM? readyCheckList = this.active
          .Where(q => q.CheckList.Trigger != null)
          .FirstOrDefault(q => q.RunTime.Evaluate(q.CheckList.Trigger!));

        if (readyCheckList != null)
        {
          this.playbackManager.SetCurrent(readyCheckList);
          this.playbackManager.TogglePlay();
        }
      }
    }
  }
}
