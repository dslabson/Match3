using UnityEngine;
using System.Collections.Generic;

public class SwapItemsSystem : MonoBehaviour
{
    #region Singleton
    private static object syncLock = new object();
    private static SwapItemsSystem swapSystem;
    public static SwapItemsSystem SwapSystem
    {
        get
        {
            if (swapSystem == null)
            {
                lock (syncLock)
                {
                    if (swapSystem == null)
                    {
                        swapSystem = FindObjectOfType<SwapItemsSystem>();

                        if (swapSystem == null)
                        {
                            GameObject singleton = new GameObject();
                            swapSystem = singleton.AddComponent<SwapItemsSystem>();
                            singleton.name = "SwapItemsSystem";
                        }
                    }
                }
            }

            return swapSystem;
        }
    }
    #endregion

    public float deadZone = 30;

    [HideInInspector] public List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
    private GridItem lastSwipedItem = null;
    private GridItem swipedItem = null;

    private bool swiping = false;
    private Vector3 mouseCurrentPosition;
    private Vector3 mouseBeginPositon;
    private Vector3 mouseDistanceBetweenPositions;
    private Vector2 dragDirection;


    private void Awake()
    {
        spriteRenderers.Clear();
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            OnBeginDrag();

        else if (Input.GetMouseButton(0) && !swiping)
            OnDrag();

        else if (Input.GetMouseButtonUp(0))
            OnEndDrag();
    }


    private void OnBeginDrag()
    {
        //Wait for end of swap
        if (swiping || AnimationSystem.movingItems.Count > 0 || GameplayController.Gameplay.InputBlocked)
            return;

        mouseBeginPositon = Input.mousePosition;

        Vector3 screenToWorldPoint = Camera.main.ScreenToWorldPoint(mouseBeginPositon);
        Vector3 mousePosition = new Vector3(screenToWorldPoint.x, screenToWorldPoint.y, 0);

        //Check if and which item was clicked
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer.bounds.Contains(mousePosition))
                swipedItem = lastSwipedItem = spriteRenderer.GetComponent<GridItem>();
        }
    }


    private void OnDrag()
    {
        //Wait for end of swap
        if (swiping || swipedItem == null || GameplayController.Gameplay.InputBlocked)
            return;
        
        mouseCurrentPosition = Input.mousePosition;
        mouseDistanceBetweenPositions = mouseCurrentPosition - mouseBeginPositon;

        //Mouse move must overcome a certain distance to works swap
        if (Mathf.Abs(mouseDistanceBetweenPositions.x) < deadZone && Mathf.Abs(mouseDistanceBetweenPositions.y) < deadZone)
            return;

        dragDirection = GetDirtection(mouseDistanceBetweenPositions);
        swipedItem.Move(dragDirection);

        swiping = true;
    }


    private void OnEndDrag()
    {
        swiping = false;
        swipedItem = null;
    }


    private Vector2 GetDirtection(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return new Vector2(Mathf.Sign(delta.x), 0);
        else
            return new Vector2(0, Mathf.Sign(delta.y));
    }


    public void UndoSwap()
    {
        if(lastSwipedItem != null && dragDirection != default(Vector2))
        {
            lastSwipedItem.Move(-dragDirection, true);
        }
    }
}