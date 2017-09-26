using System;
using UnityEngine;

[Serializable]
public class AnimationType
{
    [SerializeField]
    private float duration;
    public float Duration { get { return duration; } }

    [SerializeField]
    private AnimationCurve curve;
    public AnimationCurve Curve { get { return curve; } }


    public AnimationType(float duration = 0.2f, AnimationCurve curve = null)
    {
        this.duration = duration;
        this.curve = curve;
    }
}