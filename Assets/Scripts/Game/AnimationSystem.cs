using System.Collections.Generic;
using UnityEngine;
using static AnimationManager;

public class AnimationSystem : MonoBehaviour
{
    public static Queue<bool> movingItems = new Queue<bool>();
    protected static object syncLock = new object();

    private Vector2 startPosition;
    private Vector2 destination;
    private float time;
    private float timer;
    private float evaluate;

    private void Awake()
    {
        startPosition = destination = transform.position;
        enabled = false;
    }


    private void Update()
    {
        GameplayController.Gameplay.InputBlocked = true;

        timer += Time.deltaTime;
        evaluate = GetEvaluate(timer, time);

        if (evaluate >= 1.0f)
        {
            timer = 0.0f;
            lock (syncLock)
            {
                movingItems.Dequeue();
            }
            transform.position = destination;
        }
        else
        {
            transform.position = Vector2.Lerp(startPosition, destination, evaluate);
        }

        if ((Vector2)transform.position == destination)
        {
            EndFunction();
            enabled = false;
        }
    }
    

    public virtual void ChangePosition(Vector2 destination, AnimationType animType)
    {
        StartAnimation(destination, animType.Duration);
    }


    protected void StartAnimation(Vector2 destination, float time)
    {
        this.time = time;
        timer = 0;
        evaluate = 0;
        startPosition = transform.position;
        this.destination = destination;

        lock(syncLock)
        {
            movingItems.Enqueue(true);
        }
        
        enabled = true;
    }


    protected virtual float GetEvaluate(float timer, float time)
    {
        return AnimManager.SwapItems.Curve.Evaluate(timer / time);
    }


    protected virtual void EndFunction() { }
}