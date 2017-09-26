using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    #region Singleton
    private static object syncLock = new object();
    private static UIController ui;
    public static UIController UI
    {
        get
        {
            if (ui == null)
            {
                lock (syncLock)
                {
                    if (ui == null)
                    {
                        ui = FindObjectOfType<UIController>();

                        if (ui == null)
                        {
                            GameObject singleton = new GameObject();
                            ui = singleton.AddComponent<UIController>();
                            singleton.name = "UIController";
                        }
                    }
                }
            }

            return ui;
        }
    }
    #endregion

    [SerializeField]
    private Text textPoints;
    public Text TextPoints { get { return textPoints; } }
    

    private void Awake()
    {
        textPoints.text = "0";
    }


    public void ChangePoints(string text)
    {
        textPoints.text = text;
    }
}