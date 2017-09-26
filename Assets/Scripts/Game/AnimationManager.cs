using UnityEngine;
using System;

public class AnimationManager : MonoBehaviour
{
    #region Singleton
    private static object syncLock = new object();
    private static AnimationManager animManager;
    public static AnimationManager AnimManager
    {
        get
        {
            if (animManager == null)
            {
                lock (syncLock)
                {
                    if (animManager == null)
                    {
                        animManager = FindObjectOfType<AnimationManager>();

                        if (animManager == null)
                        {
                            GameObject singleton = new GameObject();
                            animManager = singleton.AddComponent<AnimationManager>();
                            singleton.name = "AnimationManager";
                        }
                    }
                }
            }

            return animManager;
        }
    }
    #endregion

    
    [SerializeField]
    private AnimationType swapItems;
    public AnimationType SwapItems { get { return swapItems; } }
    
    [Space(10)]
    [SerializeField]
    private AnimationType fallDownItems;
    public AnimationType FallDownItems { get { return fallDownItems; } }

    [Space(10)]
    [SerializeField]
    private AnimationType reshuffleItems;
    public AnimationType ReshuffleItems { get { return reshuffleItems; } }
    

    private void Reset()
    {
        swapItems = new AnimationType(0.15f);
        fallDownItems = new AnimationType(0.2f);
        reshuffleItems = new AnimationType(0.2f);
    }
}