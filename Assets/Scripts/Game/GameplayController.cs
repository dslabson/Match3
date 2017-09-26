/**To do:
 * - Object pooling.
 * - Change Invoke on own method.
 * - Another thread for checking matches.
**/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static SwapItemsSystem;
using static GridItemsGenerator;
using static AnimationManager;

public class GameplayController : MonoBehaviour
{
    #region Singleton
    private static object syncLock = new object();
    private static GameplayController gameplay;
    public static GameplayController Gameplay
    {
        get
        {
            if (gameplay == null)
            {
                lock (syncLock)
                {
                    if (gameplay == null)
                    {
                        gameplay = FindObjectOfType<GameplayController>();

                        if (gameplay == null)
                        {
                            GameObject singleton = new GameObject();
                            gameplay = singleton.AddComponent<GameplayController>();
                            singleton.name = "GameplayController";
                        }
                    }
                }
            }

            return gameplay;
        }
    }
    #endregion

    
    public GridItem[,] GridItems { get; set; }
    public int Points { get; private set; }
    public bool InputBlocked { get; set; } = false;

    /** int -> x position, List<int> -> list of y positions. For example:
    *** for x = 3, list of y positions = {3,4,5}, move down items for x = 3 and y above 5 (List.Max) about 3 (List.Count) places,
    *** and generate 3 items on top.
    **/
    private Dictionary<int, List<int>> positionsForNewItems = new Dictionary<int, List<int>>();
    //All matches
    private List<GridItem> matchesItems = new List<GridItem>();
    //Parent of generated items
    private GameObject grid;
    //Helps with changing kinds of reshuffle
    private int switchReshuffle = 0;
    bool existsPossibleMatch = false;


    private void Awake()
    {
        grid = GameObject.FindGameObjectWithTag("Grid");

        SwapItemsAnimation.OnFinish.AddListener(OnFinishSwap);
        SwapItemsAnimation.OnFinish.AddListener(CheckMove);
        ItemsGroupAnimation.OnFinish.AddListener(new UnityAction(() => Invoke("CheckMatches", 0.1f)));
    }


    public void OnFinishSwap()
    {
        CheckMatches(IntVector2.Zero, true);
    }


    public void CheckMatches()
    {
        CheckMatches(IntVector2.Zero);
    }
  
    
    public void CheckMatches(IntVector2 field, bool skipReshuffle = false)
    {
        matchesItems.Clear();
        positionsForNewItems.Clear();

        if (field == IntVector2.Zero)
            field = new IntVector2(GridItems.GetLength(0), GridItems.GetLength(1));

        CheckMatchesOnField(field, skipReshuffle);
        
        if (matchesItems.Count >= 3 && AnimationSystem.movingItems.Count == 0)
        {
            int points = (int)(Mathf.Pow(matchesItems.Count, 2) * 10);
            AddPionts(points);
            DestroyMatchesAndSpawnNewItems();
        }
        else if(matchesItems.Count < 3 && switchReshuffle == 0)
        {
            InputBlocked = false;
        }
    }


    private void CheckMatchesOnField(IntVector2 field, bool skipReshuffle = false)
    {
        existsPossibleMatch = false;

        for (int y = 0; y < field.y; y++)
        {
            for (int x = 0; x < field.x; x++)
            {
                IntVector2 centerItemPosition = new IntVector2(x, y);

                //Matches
                if (ExistMatch(centerItemPosition, new IntVector2(0, 1), new IntVector2(0, -1), true))
                {
                    /** Preview
                                            * * * * *  * * * * *
                     IntVector2(0,1)    ->  * * # * *  * * * * *
                     centerItemPosition ->  * * # * *  * # # # *
                     IntVector2(0,-1)   ->  * * # * *  * * * * *
                     */
                    existsPossibleMatch = true;
                }
                //Possible Matches
                else if (ExistMatch(centerItemPosition, new IntVector2(0, 2), new IntVector2(0, -1)))
                {
                    /** Preview
                     * * # * *  * * # * *  * * * * *  * * * * *
                     * * * * *  * * # * *  * # * # #  * # # * #
                     * * # * *  * * * * *  * * * * *  * * * * *
                     * * # * *  * * # * *  * * * * *  * * * * *
                     */
                    existsPossibleMatch = true;
                    //You can do something or replace else-if's to "||" in one if.
                }
                else if (ExistMatch(centerItemPosition, new IntVector2(1, 1), new IntVector2(0, -1)))
                {
                    /** Preview
                     * * * * *  * * * * *  * * * * *  * * * * *
                     * # * * *  * * * # *  * * * # *  * * * * *
                     * * # * *  * * # * *  * # # * *  * # # * *
                     * * # * *  * * # * *  * * * * *  * * * # *
                     */
                    existsPossibleMatch = true;
                }
                else if (ExistMatch(centerItemPosition, new IntVector2(0, 1), new IntVector2(1, -1)))
                {
                    /** Preview
                     * * * * *  * * * * *  * * * * *  * * * * *
                     * * # * *  * * # * *  * # * * *  * * * * *
                     * * # * *  * * # * *  * * # # *  * * # # *
                     * # * * *  * * * # *  * * * * *  * # * * *
                     */
                    existsPossibleMatch = true;
                }
                else if (ExistMatch(centerItemPosition, new IntVector2(-1, 1), new IntVector2(-1, -1)))
                {
                    /** Preview
                     * * * * *  * * * * *  * * * * *  * * * * *
                     * * # * *  * * # * *  * * * * *  * * # * *
                     * * * # *  * # * * *  * # * # *  * # * # *
                     * * # * *  * * # * *  * * # * *  * * * * *
                     */
                    existsPossibleMatch = true;
                }
            }
        }
        
        if(!existsPossibleMatch && !skipReshuffle)
        {
            Reshuffle();
        }
        else
        {
            switchReshuffle = 0;
        }
    }


    private bool ExistMatch(IntVector2 centerItemPos, IntVector2 nextItemRelativePos, IntVector2 previousItemRelativePos, bool isMatch = false)
    {
        IntVector2[] nextItemPositions = new IntVector2[4]
        {
            //Vertical cases
            centerItemPos + nextItemRelativePos,
            centerItemPos - nextItemRelativePos,
            //Horizontal cases
            centerItemPos + nextItemRelativePos.ExchangeCoordinates(),
            centerItemPos - nextItemRelativePos.ExchangeCoordinates()
        };
        IntVector2[] previousItemPositions = new IntVector2[4]
        {
            //Vertical cases
            centerItemPos + previousItemRelativePos,
            centerItemPos - previousItemRelativePos,
            //Horizontal cases
            centerItemPos + previousItemRelativePos.ExchangeCoordinates(),
            centerItemPos - previousItemRelativePos.ExchangeCoordinates()
        };

        GridItem centerItem = GridItems[centerItemPos.x, centerItemPos.y];

        bool[] matchFlags = new bool[4];

        for (int i = 0; i < 4; i++)
        {
            //If check match, needs only 2 cases: 1 vertical and 1 horizontal, becouse next 2 cases give the same results.
            if ((isMatch && i % 2 != 0) || !InsideBorders(nextItemPositions[i], previousItemPositions[i]))
            {
                continue;
            }

            GridItem nextItem = GridItems[nextItemPositions[i].x, nextItemPositions[i].y];
            GridItem previousItem = GridItems[previousItemPositions[i].x, previousItemPositions[i].y];
            int centerItemTypeId = centerItem.Type.Id;

            //If next and previous items are the same, match is possible.
            if ((centerItemTypeId == nextItem.Type.Id && centerItemTypeId == previousItem.Type.Id))
            {
                matchFlags[i] = true;

                if (isMatch)
                {
                    AddToMatchesItemsIfNotExist(centerItem, nextItem, previousItem);
                    AddToPositionsForNewItemsIfNotExist(centerItemPos, nextItemPositions[i], previousItemPositions[i]);
                }
            }
        }

        int lenght = matchFlags.Length;
        for (int i = 0; i < lenght; i++)
        {
            if (matchFlags[i])
                return true;
        }

        return false;
    }


    private bool InsideBorders(params IntVector2[] coordinates)
    {
        int gridItemsXLenght = GridItems.GetLength(0);
        int gridItemsYLenght = GridItems.GetLength(1);

        //I am use "for" loop instead of foreach for GB oprimization
        int lenght = coordinates.Length;
        for (int i = 0; i < lenght; i++)
        {
            if (coordinates[i].x >= gridItemsXLenght || coordinates[i].y >= gridItemsYLenght || coordinates[i].x < 0 || coordinates[i].y < 0)
            {
                return false;
            }
        }

        /**foreach
        foreach (IntVector2 co in coordinates)
        {
            if (co.x >= gridItemsXLenght || co.y >= gridItemsYLenght || co.x < 0 || co.y < 0)
            {
                return false;
            }
        }**/

        return true;
    }


    private void AddToMatchesItemsIfNotExist(params GridItem[] gridItems)
    {
        int lenght = gridItems.Length;
        for (int i = 0; i < lenght; i++)
        {
            if (!matchesItems.Contains(gridItems[i]))
            {
                matchesItems.Add(gridItems[i]);
            }
        }

        /**foreach
        foreach (GridItem item in gridItems)
        {
            if (!matchesItems.Contains(item))
            {
                matchesItems.Add(item);
            }
        }**/
    }


    private void AddToPositionsForNewItemsIfNotExist(params IntVector2[] positions)
    {
        int lenght = positions.Length;
        for (int i = 0; i < lenght; i++)
        {
            if (!positionsForNewItems.ContainsKey(positions[i].x))
            {
                positionsForNewItems.Add(positions[i].x, new List<int> { positions[i].y });
            }
            else
            {
                if (!positionsForNewItems[positions[i].x].Contains(positions[i].y))
                {
                    positionsForNewItems[positions[i].x].Add(positions[i].y);
                }
            }
        }

        /**foreach
        foreach (IntVector2 position in positions)
        {
            if (!positionsForNewItems.ContainsKey(position.x))
            {
                positionsForNewItems.Add(position.x, new List<int> { position.y });
            }
            else
            {
                if (!positionsForNewItems[position.x].Contains(position.y))
                {
                    positionsForNewItems[position.x].Add(position.y);
                }
            }
        }**/
    }


    //Animation
    private void DestroyMatchesAndSpawnNewItems()
    {
        float animDuration = 0;

        for (int i = 0; i < matchesItems.Count; i++)
        {
            animDuration = matchesItems[i].PlayDestroyAnim();
        }
        
        Invoke("DestroyMatches", animDuration);
    }

    //Physical destroying
    private void DestroyMatches()
    {
        for (int i = 0; i < matchesItems.Count; i++)
        {
            SwapSystem.spriteRenderers.Remove(matchesItems[i].GetComponent<SpriteRenderer>());
            GridItems[matchesItems[i].Position.x, matchesItems[i].Position.y] = null;
            Destroy(matchesItems[i].gameObject);
        }

        SpawnNewItems();
    }


    private void SpawnNewItems()
    {
        //int -> number of position to move down, List<GridItem> -> list of items to move down about this(key) positions.
        Dictionary<int, List<GridItem>> itemsToMove = new Dictionary<int, List<GridItem>>();

        int lenghtY = GridItems.GetLength(1);
        
        foreach (int x in positionsForNewItems.Keys)
        {
            int numberOfDestroyedItems = 0;

            for (int y = 0; y < lenghtY; y++)
            {
                if (GridItems[x, y] == null)
                {
                    numberOfDestroyedItems++;
                }
                else if(numberOfDestroyedItems > 0)
                {
                    if (!itemsToMove.ContainsKey(numberOfDestroyedItems))
                        itemsToMove.Add(numberOfDestroyedItems, new List<GridItem>());
                    
                    int newY = y - numberOfDestroyedItems;
                    GridItems[x, newY] = GridItems[x, y];
                    GridItems[x, newY].Position = new IntVector2(x, newY);
                    itemsToMove[numberOfDestroyedItems].Add(GridItems[x, newY]);

                    #if UNITY_EDITOR
                    GridItems[x, newY].UpdateGameObjectName();
                    #endif
                }
            }

            if (!itemsToMove.ContainsKey(numberOfDestroyedItems))
                itemsToMove.Add(numberOfDestroyedItems, new List<GridItem>());

            //Spawn new items
            for (int i = lenghtY - numberOfDestroyedItems; i < lenghtY; i++)
            {
                ItemsGenerator.GenerateNewItem(x, i, numberOfDestroyedItems, true);
                itemsToMove[numberOfDestroyedItems].Add(GridItems[x, i]);
            }
        }
        
        MoveItems(itemsToMove, false, AnimManager.FallDownItems);
    }


    //Move down items (animation)
    private void MoveItems(Dictionary<int, List<GridItem>> itemsToMove, bool horizontal = false, AnimationType animType = null)
    {
        foreach (int numberOfPositionToMove in itemsToMove.Keys)
        {
            GameObject tmpGo = new GameObject("Wrapper " + numberOfPositionToMove);
            tmpGo.transform.SetParent(grid.transform);
            ItemsGroupAnimation anim = tmpGo.AddComponent<ItemsGroupAnimation>();
            anim.OnFinishForInstance.AddListener(new UnityAction(() => ResetParentTmp(itemsToMove[numberOfPositionToMove], grid, tmpGo)));


            int lenght = itemsToMove[numberOfPositionToMove].Count;
            for(int i = 0; i < lenght; i++)
            {
                itemsToMove[numberOfPositionToMove][i].transform.SetParent(tmpGo.transform);
            }

            if(horizontal)
            {
                float distance = ItemsGenerator.DistanceBetweenItems.x * numberOfPositionToMove;
                anim.ChangePosition(new Vector2(tmpGo.transform.position.x - distance, tmpGo.transform.position.y), animType);
            }
            else
            {
                float distance = ItemsGenerator.DistanceBetweenItems.y * numberOfPositionToMove;
                anim.ChangePosition(new Vector2(tmpGo.transform.position.x, tmpGo.transform.position.y - distance), animType);
            }
            
        }
    }


    //Destroys temporary parent for items. Parent has been used to move down items in one object.
    private void ResetParentTmp(List<GridItem> items, GameObject grid, GameObject tmpGo)
    {
        int lenght = items.Count;
        for(int i = 0; i < lenght; i++)
        {
            items[i].transform.SetParent(grid.transform);
        }

        Destroy(tmpGo);
    }


    private void CheckMove()
    {
        if (matchesItems.Count < 3)
        {
            InputBlocked = true;
            Invoke("UndoMove", 0.1f);
        }
    }


    private void UndoMove()
    {
        SwapSystem.UndoSwap();
    }


    private void AddPionts(int points)
    {
        Points += points;
        UIController.UI.ChangePoints(Points.ToString());
    }


    private void Reshuffle()
    {
        //int -> number of position to move (x or y), List<GridItem> -> list of items to move about this(key) positions.
        Dictionary<int, List<GridItem>> itemsToMove = new Dictionary<int, List<GridItem>>();
        itemsToMove.Add(1, new List<GridItem>());
        itemsToMove.Add(-1, new List<GridItem>());

        switchReshuffle = switchReshuffle > 5 ? 0 : switchReshuffle + 1;

        int LenghtY = GridItems.GetLength(1);
        int LenghtX = GridItems.GetLength(0);

        //Vertical
        if (switchReshuffle % 2 == 0)
        {
            for (int y = 0; y < LenghtY - 1; y += 2)
            {
                for (int x = 0; x < LenghtX; x++)
                {
                    if (switchReshuffle == 2 && x % 2 != 0)
                        continue;
                    else if (switchReshuffle == 4 && x % 2 == 0)
                        continue;
                    else if (switchReshuffle == 6)
                    {
                        y++;
                        x = 0;
                        continue;
                    }

                    //Changing items position
                    GridItem item = GridItems[x, y];
                    GridItems[x, y] = GridItems[x, y + 1];
                    GridItems[x, y].Position = new IntVector2(x, y);
                    item.Position = new IntVector2(x, y + 1);
                    GridItems[x, y + 1] = item;
                    itemsToMove[-1].Add(GridItems[x, y + 1]);
                    itemsToMove[1].Add(GridItems[x, y]);

                    #if UNITY_EDITOR
                    GridItems[x, y].UpdateGameObjectName();
                    GridItems[x, y + 1].UpdateGameObjectName();
                    #endif
                }
            }

            MoveItems(itemsToMove, false, AnimManager.ReshuffleItems);
        }
        //Horizontal
        else
        {
            for (int x = 0; x < LenghtX - 1; x += 2)
            {
                for (int y = 0; y < LenghtY; y++)
                {
                    if (switchReshuffle == 1 && y % 2 != 0)
                        continue;
                    else if (switchReshuffle == 3 && y % 2 == 0)
                        continue;
                    else if (switchReshuffle == 5)
                    {
                        x++;
                        y = 0;
                        switchReshuffle = 0;
                        continue;
                    }

                    //Changing items position
                    GridItem item = GridItems[x, y];
                    GridItems[x, y] = GridItems[x + 1, y];
                    GridItems[x, y].Position = new IntVector2(x, y);
                    item.Position = new IntVector2(x + 1, y);
                    GridItems[x + 1, y] = item;
                    itemsToMove[-1].Add(GridItems[x + 1, y]);
                    itemsToMove[1].Add(GridItems[x, y]);

                    #if UNITY_EDITOR
                    GridItems[x, y].UpdateGameObjectName();
                    GridItems[x + 1, y].UpdateGameObjectName();
                    #endif
                }
            }

            MoveItems(itemsToMove, true, AnimManager.ReshuffleItems);
        }
    }
}