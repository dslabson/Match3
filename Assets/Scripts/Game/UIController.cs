using UnityEngine;
using UnityEngine.UI;
using static GameplayController;

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

    [SerializeField]
    private Text textAddedPoints;
    public Text TextAddedPoints { get { return textAddedPoints; } }

    private TextColorAnimation fadeInOutAddedPointsAnimation;
    private Color textAddedPointsColor;
    private int tmpAddedPoints;

    [SerializeField]
    private Text textMoves;
    public Text TextMoves { get { return textMoves; } }
    

    private void Awake()
    {
        textPoints.text = "0";
        textAddedPoints.text = "";
        textMoves.text = "0";

        fadeInOutAddedPointsAnimation = textAddedPoints.GetOrAddComponent<TextColorAnimation>();
        textAddedPointsColor = textAddedPoints.color;
    }


    public void UpdatePoints(int addedPoints)
    {
        TextPoints.text = Gameplay.Points.ToString();
       
        if(fadeInOutAddedPointsAnimation.IsAnimating)
        {
            tmpAddedPoints += addedPoints;
        }
        else
        {
            tmpAddedPoints = addedPoints;
        }

        TextAddedPoints.text = tmpAddedPoints.ToString("+0");

        fadeInOutAddedPointsAnimation.ChangeColor(textAddedPointsColor, new Color(textAddedPointsColor.r, textAddedPointsColor.g, textAddedPointsColor.b, 0));
    }
}