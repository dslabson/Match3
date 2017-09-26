using UnityEngine;
using static GameplayController;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(SwapItemsSystem))]
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


    //Awake() and Start() are public for editor script
    public void Awake()
    {
        Gameplay.GridItems = new GridItem[boardDimensions.x, boardDimensions.y];
        DistanceBetweenItems = GetDistanceBetweedItems();

        #if UNITY_EDITOR
        if(!Application.isPlaying)
            Gameplay.GetComponent<SwapItemsSystem>().spriteRenderers.Clear();
        #endif
    }

    public void Start()
    {
        itemPrefab.transform.localScale = itemsDimensions;

        Generate(boardDimensions);
        if(Application.isPlaying)
            Gameplay.CheckMatches();
    }


    public void GenerateNewItem(int x, int y, float yMR = 0, bool canMatch = false)
    {
        GridItem item = Gameplay.GridItems[x, y] = Instantiate(itemPrefab, new Vector3(0, 100, 0), Quaternion.identity).GetComponent<GridItem>();
        SpriteRenderer spriteRenderer = item.GetComponent<SpriteRenderer>();

        Gameplay.GetComponent<SwapItemsSystem>().spriteRenderers.Add(spriteRenderer);

        item.Position = new IntVector2(x, y);

        bool yCorrected = true;
        bool xMinusOneSameType = true;
        bool xMinusTwoSameType = true;
        while ((xMinusOneSameType && xMinusTwoSameType) || !yCorrected)
        {
            item.Type = GetRandomGridItemType();

            if (canMatch)
                break;

            //Prevents generating 3 items with the same type (Prevents match)
            //Vertical
            if (y > 1)
            {
                bool yMinusOneSameType = Gameplay.GridItems[x, y - 1].Type == item.Type;
                bool yMinusTwoSameType = Gameplay.GridItems[x, y - 2].Type == item.Type;

                if (yMinusOneSameType && yMinusTwoSameType)
                    yCorrected = false;
                else
                    yCorrected = true;
            }
            else
                yCorrected = true;

            //Horizontal
            xMinusOneSameType = x > 1 ? Gameplay.GridItems[x - 1, y].Type == item.Type : false;
            xMinusTwoSameType = x > 1 ? Gameplay.GridItems[x - 2, y].Type == item.Type : false;
        }

        #if UNITY_EDITOR
        item.UpdateGameObjectName();
        #endif

        item.transform.SetParent(parentOfItems);
        item.transform.localScale = new Vector3(itemsDimensions.x, itemsDimensions.y, 1);

        Vector2 DistanceBetweenItems = GetDistanceBetweedItems();
        float posX = DistanceBetweenItems.x * (x - (boardDimensions.x / 2.0f)) + (DistanceBetweenItems.x / 2.0f);
        float posY = DistanceBetweenItems.y * ((y + yMR) - (boardDimensions.y / 2.0f)) + (DistanceBetweenItems.y / 2.0f);
        item.transform.localPosition = new Vector3(posX, posY);

        spriteRenderer.sprite = item.Type.Sprite;
        spriteRenderer.color = item.Type.Color;
    }


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



#if !UNITY_2017_1_1 //Unity 2017.1 bug, works correctly in Unity 5, probably fixed in Unity 2017_2
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