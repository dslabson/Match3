using System.Collections.Generic;
using UnityEngine;
using static AnimationManager;

public class AnimationSystem : MonoBehaviour
{
    protected virtual Queue<bool> AnimatingObjects { get; set; }
    protected virtual object SyncLock { get; set; }

    public bool IsAnimating { get; protected set; }
    
    protected float time;
    protected float timer;
    protected float evaluate;


    protected void Update()
    {
        timer += Time.deltaTime;
        evaluate = GetEvaluate(timer, time);
        if (evaluate >= 0.999f)
        {
            timer = 0.0f;
            lock (SyncLock)
            {
                AnimatingObjects.Dequeue();
            }

            AnimationFinishing();
        }
        else
        {
            Animation();
        }

        if (CanFinish())
        {
            OnAnimationFinish();
            enabled = false;
        }
    }

    protected void StartAnimation(float time)
    {
        this.time = time;
        timer = 0;
        evaluate = 0;

        lock (SyncLock)
        {
            AnimatingObjects.Enqueue(true);
        }

        IsAnimating = true;
        enabled = true;
    }


    protected virtual float GetEvaluate(float timer, float time)
    {
        return AnimManager.SwapItems.Curve.Evaluate(timer / time);
    }

    
    protected virtual void Animation()
    {
        //For example:
        //transform.position = Vector2.Lerp(start, destination, evaluate);
        Debug.LogWarning("Animation method is not set");
    }

    protected virtual void AnimationFinishing()
    {
        //For example:
        //transform.position = destination;
        Debug.LogWarning("AnimationFinish method is not set");
    }

    protected virtual bool CanFinish()
    {
        //For example:
        //return (Vector2)transform.position == destination;
        Debug.LogWarning("CanFinish method is not set");
        return false;
    }


    protected virtual void OnAnimationFinish()
    {
        IsAnimating = false;
    }
}