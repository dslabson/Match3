using UnityEngine;
using UnityEngine.Events;
using static AnimationManager;

public class ItemsGroupAnimation : MovementAnimationSystem
{
    public static UnityEvent OnFinish = new UnityEvent();
    public UnityEvent OnFinishForInstance = new UnityEvent();

    private AnimationType animType;


    public override void ChangePosition(Vector2 destination, AnimationType animType = null)
    {
        this.animType = animType ?? AnimManager.FallDownItems;
        base.ChangePosition(destination, animType);
    }



    protected override float GetEvaluate(float timer, float time)
    {
        return animType.Curve.Evaluate(timer / time);
    }

    protected override void OnAnimationFinish()
    {
        OnFinishForInstance.Invoke();
        
        lock (SyncLock)
        {
            if (AnimatingObjects.Count == 0)
                OnFinish.Invoke();
        }

        base.OnAnimationFinish();
    }
}