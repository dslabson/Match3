using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static AnimationManager;

[RequireComponent(typeof(Text))]
public class TextColorAnimation : AnimationSystem
{
    public static Queue<bool> animatingObjects { get; private set; } = new Queue<bool>();
    protected override Queue<bool> AnimatingObjects { get { return animatingObjects; } set { animatingObjects = value; } }

    private static object syncLook = new object();
    protected override object SyncLock { get { return syncLook; } set { syncLook = value; } }

    private AnimationType animType;

    private Text text;

    private Color startColor;
    private Color destinationColor;
    

    private void Awake()
    {
        text = GetComponent<Text>();
        startColor = destinationColor = text.color;
        enabled = false;
    }


    private new void Update()
    {
        base.Update();
    }


    public virtual void ChangeColor(Color start, Color destination, AnimationType animType = null)
    {
        startColor = start;
        destinationColor = destination;

        this.animType = animType ?? AnimManager.AddedPointsFadeInOut;

        StartAnimation(this.animType.Duration);
    }


    protected override void Animation()
    {
        text.color = Color.Lerp(startColor, destinationColor, evaluate);
    }


    protected override void AnimationFinishing()
    {
        text.color = destinationColor;
    }


    protected override bool CanFinish()
    {
         return text.color == destinationColor;
    }

    protected override float GetEvaluate(float timer, float time)
    {
        return AnimManager.AddedPointsFadeInOut.Curve.Evaluate(timer / time);
    }
}