using UnityEngine;
using static GameplayController;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridItemsGenerator : MonoBehaviour
{
    #region Singleton
    private static object syncLock = new object();
    private static GridItemsGenerator itemsGenerator;
    public static GridItemsGenerator ItemsGenerator
    {
        get
        {
            if (itemsGenerator == null)
            {
                lock (syncLock)
                {
                    if (itemsGenerator == null)
                    {
                        itemsGenerator = FindObjectOfType<GridItemsGenerator>();

                        if (itemsGenerator == null)
                        {
                            GameObject singleton = new GameObject();
                            itemsGenerator = singleton.AddComponent<GridItemsGenerator>();
                            singleton.name = "GridItemsGenerator";
                        }
                    }
                }
            }

            return itemsGenerator;
        }
    }
    #endregion

    public Vector2 DistanceBetweenItems { get; private set; }

    public GameObject itemPrefab;
    public Transform parentOfItems;

    public ItemType[] gridItemTypes;
    public IntVector2 boardDimensions;
    public Vector2 itemsDimensions;
    [Range(0, 1)]
    public float spaceBetweenItems;
    
    //Object pooling
    public Queue<GridItem> itemsToRegenerate = new Queue<GridItem>();
    

    //Awake() and Start() are public for editor script
    public void Awake()
    {
        if (boardDimensions.x < 3)
            boardDimensions.x = 3;
        if (boardDimensions.y < 3)
            boardDimensions.y = 3;

        Gameplay.GridItems = new GridItem[boardDimensions.x, boardDimensions.y];
        DistanceBetweenItems = GetDistanceBetweedItems();

        #if UNITY_EDITOR
        if(!Application.isPlaying)
            SwapItemsSystem.SwapSystem.spriteRenderers.Clear();
        #endif
    }

    public void Start()
    {
        itemPrefab.transform.localScale = itemsDimensions;

        Generate(boardDimensions);

        if(Application.isPlaying)
            Gameplay.CheckMatches();
    }

    
    /// <param name="yAboveTopBorder">y and yAboveTopBorder will be a sum of item y pos. You can use this parameter if you want to spawn item above grid (for generate new items) </param>
    /// <param name="canMatch">If this parameter is true, it's possible generate items with matches immediately after generating</param>
    public void GenerateNewItem(int x, int y, float yAboveTopBorder = 0, bool canMatch = false)
    {
        GridItem item;
        SpriteRenderer spriteRenderer;

        if (itemsToRegenerate.Count == 0)
        {
            //Generate new item
            item = Gameplay.GridItems[x, y] = Instantiate(itemPrefab, new Vector3(0, 100, 0), Quaternion.identity).GetComponent<GridItem>();
            spriteRenderer = item.GetComponent<SpriteRenderer>();
            SwapItemsSystem.SwapSystem.spriteRenderers.Add(spriteRenderer);
        }
        else
        {
            //Use destroyed item
            item = Gameplay.GridItems[x, y] = itemsToRegenerate.Dequeue();
            item.GetComponent<Animator>().enabled = false;
            item.gameObject.SetActive(true);
            spriteRenderer = item.GetComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 0;
        }
        
        //Postition in the grid.
        item.Position = new IntVector2(x, y);
        
        bool xMinusOneSameType;
        bool xMinusTwoSameType;
        bool yMinusOneSameType;
        bool yMinusTwoSameType;

        do
        {
            item.Type = GetRandomGridItemType();

            if (canMatch)
                break;

            //Prevents generating 3 items with the same type (Prevents match)
            //Vertical
            yMinusOneSameType = y > 1 ? Gameplay.GridItems[x, y - 1].Type.Id == item.Type.Id : false;
            yMinusTwoSameType = y > 1 ? Gameplay.GridItems[x, y - 2].Type.Id == item.Type.Id : false;

            //Horizontal
            xMinusOneSameType = x > 1 ? Gameplay.GridItems[x - 1, y].Type.Id == item.Type.Id : false;
            xMinusTwoSameType = x > 1 ? Gameplay.GridItems[x - 2, y].Type.Id == item.Type.Id : false;
        }
        while ((xMinusOneSameType && xMinusTwoSameType) || (yMinusOneSameType && yMinusTwoSameType));

        #if UNITY_EDITOR || BUILD_DEBUG
            item.UpdateGameObjectName();
        #endif

        //Set item position on scene.
        item.transform.SetParent(parentOfItems);
        item.transform.localScale = new Vector3(itemsDimensions.x, itemsDimensions.y, 1);

        Vector2 DistanceBetweenItems = GetDistanceBetweedItems();
        float posX = DistanceBetweenItems.x * (x - (boardDimensions.x / 2.0f)) + (DistanceBetweenItems.x / 2.0f);
        float posY = DistanceBetweenItems.y * ((y + yAboveTopBorder) - (boardDimensions.y / 2.0f)) + (DistanceBetweenItems.y / 2.0f);
        item.transform.localPosition = new Vector3(posX, posY);

        spriteRenderer.sprite = item.Type.Sprite;
        spriteRenderer.color = item.Type.Color;
    }


    /// <summary>Generate items</summary>
    /// <param name="dimensions">Grid dimensions, amount of items vertical and horizontal</param>
    private void Generate(IntVector2 dimensions)
    {
        parentOfItems.RemoveChilds();

        for (int y = 0; y < boardDimensions.y; y++)
        {
            for(int x = 0; x < boardDimensions.x; x++)
            {
                GenerateNewItem(x, y);
            }
        }
    }

    
    private ItemType GetRandomGridItemType()
    {
        int index = Random.Range(0, gridItemTypes.Length);
        return gridItemTypes[index];
    }


    private Vector2 GetDistanceBetweedItems()
    {
        Vector2 spriteSize = GetSpriteSize();

        float x = (spriteSize.x / 2) + spaceBetweenItems;
        float y = (spriteSize.y / 2) + spaceBetweenItems;

        return new Vector2(x, y);
    }


    private Vector2 GetSpriteSize()
    {
        Sprite itemSprite = itemPrefab.GetComponent<SpriteRenderer>().sprite;

        float x = itemSprite.rect.width * itemPrefab.transform.localScale.x;
        float y = itemSprite.rect.height * itemPrefab.transform.localScale.y;
        
        return new Vector2(x, y) / 100;
    }



#if UNITY_EDITOR && !UNITY_2017_1_1 //Unity 2017.1 bug, works correctly in Unity 5, fixed in Unity 2017.2
    private void OnValidate()
    {
        Awake();
        Start();
    }
#endif

#if UNITY_EDITOR
    private void Reset()
    {
        itemPrefab = Resources.Load("GridItem") as GameObject;
        Sprite bean = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Candies/bean_white.png");
        Sprite heart = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Candies/heart_white.png");
        Sprite star = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Candies/star_white.png");

        boardDimensions = new IntVector2(9, 9);
        itemsDimensions = new Vector2(0.5f, 0.5f);
        spaceBetweenItems = 0.1f;
        parentOfItems = transform;

        gridItemTypes = new ItemType[] {
            new ItemType("Green", bean, Color.green),
            new ItemType("Blue", bean, Color.blue),
            new ItemType("Red", heart, Color.red),
            new ItemType("Yellow", star, Color.yellow),
            new ItemType("Cyan", bean, Color.cyan),
            new ItemType("Magenta", heart, Color.magenta)
        };

        Awake();
        Start();
    }
#endif
}