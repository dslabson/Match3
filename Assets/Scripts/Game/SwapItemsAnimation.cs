using UnityEngine;
using UnityEngine.Events;
using static AnimationManager;

public class SwapItemsAnimation : AnimationSystem
{
    public static UnityEvent OnFinish = new UnityEvent();
    public bool skipOnFinish = false;
    //private AnimationType animType; //You can use it when you override GetEvaluate() 


    public void ChangePosition(Vector2 destination, bool skipOnFinish = false, AnimationType animType = null)
    {
        this.skipOnFinish = skipOnFinish;
        ChangePosition(destination, animType);
    }


    public override void ChangePosition(Vector2 destination, AnimationType animType)
    {
        lock(syncLock)
        {
            if (movingItems.Count > 2)
                return;
        }

        //this.animType = animType == null ? AnimManager.SwapItems : animType;
        StartAnimation(destination, animType.Duration);
    }


    protected override void EndFunction()
    {
        if (skipOnFinish)
        {
            skipOnFinish = false;
            GameplayController.Gameplay.InputBlocked = false;
            return;
        }

        if (movingItems.Count == 0)
        {
            lock (syncLock)
            {
                if (movingItems.Count == 0)
                    OnFinish.Invoke();
            }
        }
    }
}