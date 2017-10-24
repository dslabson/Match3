using System;
using UnityEngine;
using UnityEngine.Events;
using static AnimationManager;

public class SwapItemsAnimation : MovementAnimationSystem
{
    public static UnityEvent OnFinish = new UnityEvent();
    public bool skipOnFinish = false;
    private AnimationType animType;


    public void ChangePosition(Vector2 destination, bool skipOnFinish = false, AnimationType animType = null)
    {
        this.skipOnFinish = skipOnFinish;
        ChangePosition(destination, animType);
    }


    public override void ChangePosition(Vector2 destination, AnimationType animType)
    {
        lock(SyncLock)
        {
            if (AnimatingObjects.Count >= 2)
                return;
        }

        this.animType = animType ?? AnimManager.SwapItems;
        base.ChangePosition(destination, this.animType);
    }


    protected override void OnAnimationFinish()
    {
        base.OnAnimationFinish();

        if (skipOnFinish)
        {
            skipOnFinish = false;
            GameplayController.Gameplay.InputBlocked = false;
            return;
        }
        
        lock (SyncLock)
        {
            if (!IsAnimatingSomeObject())
            {
                OnFinish.Invoke();
            }
        }
    }
}