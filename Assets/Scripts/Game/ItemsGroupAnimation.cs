using UnityEngine;
using UnityEngine.Events;
using static AnimationManager;

public class ItemsGroupAnimation : AnimationSystem
{
    public static UnityEvent OnFinish = new UnityEvent();
    public UnityEvent OnFinishForInstance = new UnityEvent();

    private AnimationType animType;


    public override void ChangePosition(Vector2 destination, AnimationType animType = null)
    {
        lock (syncLock)
        {
            if (movingItems.Count > 2)
                return;
        }


        this.animType = animType == null ? AnimManager.FallDownItems : animType;
        StartAnimation(destination, animType.Duration);
    }



    protected override float GetEvaluate(float timer, float time)
    {
        return animType.Curve.Evaluate(timer / time);
    }


    protected override void EndFunction()
    {
        OnFinishForInstance.Invoke();

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