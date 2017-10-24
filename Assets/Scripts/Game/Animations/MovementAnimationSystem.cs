using System.Collections.Generic;
using UnityEngine;

public class MovementAnimationSystem : AnimationSystem
{
    public static Queue<bool> animatingObjects { get; private set; } = new Queue<bool>();
    protected override Queue<bool> AnimatingObjects { get { return animatingObjects; } set { animatingObjects = value; } }

    private static object syncLook = new object();
    protected override object SyncLock { get { return syncLook; } set { syncLook = value; } }

    private Vector2 startPosition;
    private Vector2 destinationPosition;


    private void Awake()
    {
        startPosition = destinationPosition = transform.position;
        enabled = false;
    }


    private new void Update()
    {
        GameplayController.Gameplay.InputBlocked = true;

        base.Update();
    }


    public virtual void ChangePosition(Vector2 destination, AnimationType animType)
    {
        startPosition = transform.position;
        destinationPosition = destination;

        StartAnimation(animType.Duration);
    }


    protected override void Animation()
    {
        transform.position = Vector2.Lerp(startPosition, destinationPosition, evaluate);
    }


    protected override void AnimationFinishing()
    {
        transform.position = destinationPosition;
    }


    protected override bool CanFinish()
    {
        return (Vector2)transform.position == destinationPosition;
    }


    public static bool IsAnimatingSomeObject()
    {
        lock(syncLook)
        {
            return animatingObjects.Count > 0;
        }
    }
}