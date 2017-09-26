using UnityEngine;
using static GameplayController;

[RequireComponent(typeof(Animator), typeof(SwapItemsAnimation))]
public class GridItem : MonoBehaviour
{
    public ItemType Type { get; set; }
    public IntVector2 Position { get; set; }

    private Animator animator;
    private SwapItemsAnimation sawpAnim;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        sawpAnim = GetComponent<SwapItemsAnimation>();
    }


    public void UpdateGameObjectName()
    {
        name = string.Format("{0}\t: [{1}][{2}]", Type.Name, Position.x, Position.y);
    }


    public float PlayDestroyAnim()
    {
        //Now item has only one anim, so it does the job.
        animator.enabled = true;
        GetComponent<SpriteRenderer>().sortingOrder = 10;
        return animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
    }


    public void Move(Vector2 dir, bool undo = false)
    {
        int newPositionXorY;
        if (!CanMove(dir, out newPositionXorY))
            return;

        GridItem itemNeighbor;

        //Horizontal
        if (IsHorizontalDirection(dir))
        {
            itemNeighbor = Gameplay.GridItems[newPositionXorY, Position.y];

            Gameplay.GridItems[Position.x, Position.y] = itemNeighbor;
            Gameplay.GridItems[newPositionXorY, Position.y] = this;

            itemNeighbor.Position = new IntVector2(Position.x, Position.y);
            Position = new IntVector2(newPositionXorY, Position.y);
        }
        //Vertical
        else
        {
            itemNeighbor = Gameplay.GridItems[Position.x, newPositionXorY];

            Gameplay.GridItems[Position.x, Position.y] = itemNeighbor;
            Gameplay.GridItems[Position.x, newPositionXorY] = this;

            itemNeighbor.Position = new IntVector2(Position.x, Position.y);
            Position = new IntVector2(Position.x, newPositionXorY);
        }

        Vector2 newPositionOnScene = itemNeighbor.transform.position;
        itemNeighbor.sawpAnim.ChangePosition(transform.position, undo, AnimationManager.AnimManager.SwapItems);
        sawpAnim.ChangePosition(newPositionOnScene, undo, AnimationManager.AnimManager.SwapItems);

        #if UNITY_EDITOR
        itemNeighbor.UpdateGameObjectName();
        UpdateGameObjectName();
        #endif
    }
    

    private bool CanMove(Vector2 dir, out int newPosition)
    {
        if (IsHorizontalDirection(dir))
        {
            newPosition = Position.x + (int)Mathf.Sign(dir.x);
            if (newPosition >= Gameplay.GridItems.GetLength(0) || newPosition < 0)
                return false;
        }
        else
        {
            newPosition = Position.y + (int)Mathf.Sign(dir.y);
            if (newPosition >= Gameplay.GridItems.GetLength(1) || newPosition < 0)
                return false;
        }

        return !(AnimationSystem.movingItems.Count > 0);
    }


    private bool IsHorizontalDirection(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return true;
        else
            return false;
    }
}