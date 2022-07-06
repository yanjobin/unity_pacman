using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public static float SPEED_DEFAULT = 10f;

    public enum Direction
    {
        Up, Left, Down, Right
    }

    private static float SPEED_ADJUST_CENTER = 25f;

    public EventHandler OnEnterNewTile;
    public EventHandler OnChangeDirection;

    private Map map;
    private Direction currentDirection;
    private Direction desiredDirection;
    [SerializeField] private float speed;

    public float CurrentSpeed
    {
        get
        {
            return speed;
        }
        set
        {
            speed = value;
        }
    }

    public Direction ForceDirection
    {
        set
        {
            currentDirection = value;
            desiredDirection = value;
        }
    }
    public Direction DesiredDirection
    {
        get
        {
            return desiredDirection;
        }
        set
        {
            desiredDirection = value;
        }
    }
    public Direction CurrentDirection
    {
        get
        {
            return currentDirection;
        }
    }
    public Vector2Int CurrentGridPosition
    {
        get
        {
            return map.GetGridPositionAt(transform.position);
        }
    }
    public int SkipFrames { get; set; }

    public bool IsMoving { get; private set; }

    private Vector2 startPosition;
    private Direction startDirection;

    private void Awake()
    {
        OnEnterNewTile = null;
        OnChangeDirection = null;
    }

    public void Init(Map map, Vector2 initialPosition, Direction initialDirection)
    {
        this.map = map;
        startPosition = initialPosition;
        startDirection = initialDirection;
        Restart();
    }

    public void Restart()
    {
        transform.position = new Vector3(startPosition.x, startPosition.y);
        currentDirection = startDirection;
        desiredDirection = startDirection;

        OnChangeDirection?.Invoke(null, EventArgs.Empty);
    }

    void Update()
    {
        if (speed == 0)
        {
            IsMoving = false;
            return;
        }

        if (SkipFrames > 0)
        {
            SkipFrames--;
            return;
        }

        float delta = CurrentSpeed * GameManager.INSTANCE.DeltaTime;
        Vector2Int dirVector = GetDirectionVector(currentDirection);
        Vector2Int gridPosition = CurrentGridPosition;

        //In the center of tile, check if we can move further or not
        if (map.IsInCenterTile(transform.position))
        {
            //Check current tile
            Vector2Int tile = map.OnEnterCenterTile(gridPosition, currentDirection);
            if (tile != gridPosition)
            {
                transform.position = new Vector3(tile.x, tile.y);
                gridPosition = tile;
            }

            //New direction requested
            if (desiredDirection != currentDirection)
            {
                Vector2Int desiredDirVector = GetDirectionVector(desiredDirection);
                Vector2Int nextDesiredGridPosition = tile + desiredDirVector;

                if (map.IsAllowedGridPosition(nextDesiredGridPosition, GetComponent<Ghost>() == null ? false : GetComponent<Ghost>().CanGoThroughHouse))
                {
                    //Grid position in requested direction is allowed, change direction.
                    currentDirection = desiredDirection;
                    dirVector = GetDirectionVector(currentDirection);

                    //Call event
                    if (OnChangeDirection != null) OnChangeDirection(null, EventArgs.Empty);
                }
            }

            Vector2Int nextGridPosition = tile + dirVector;

            if (!map.IsAllowedGridPosition(nextGridPosition, GetComponent<Ghost>() == null ? false : GetComponent<Ghost>().CanGoThroughHouse))
            {
                //Grid position in direction is NOT allowed. Stop movement.
                delta = 0;
            }
        }

        IsMoving = delta > 0;

        Vector3 moveDistance = new Vector3(dirVector.x * delta, dirVector.y * delta);

        transform.position += moveDistance;
        Vector2Int newGridPosition = map.GetGridPositionAt(transform.position);

        //Adjust position towards center tile
        if (currentDirection == Direction.Up || currentDirection == Direction.Down)
        {
            //Moving vertically, adjust horizontal position
            float newHorizontalPosition = Mathf.Lerp(transform.position.x, newGridPosition.x, GameManager.INSTANCE.DeltaTime * SPEED_ADJUST_CENTER);
            transform.position = new Vector3(newHorizontalPosition, transform.position.y, transform.position.z);
        }
        else
        {
            //Moving horizontally
            float newVerticalPosition = Mathf.Lerp(transform.position.y, newGridPosition.y, GameManager.INSTANCE.DeltaTime * SPEED_ADJUST_CENTER);
            transform.position = new Vector3(transform.position.x, newVerticalPosition, transform.position.z);
        }

        if (newGridPosition != gridPosition)
        {
            //Entered new tile. Update current grid position and call event
            if (OnEnterNewTile != null) OnEnterNewTile(this, EventArgs.Empty);
        }
    }
    
    public float GetDirectionRotation()
    {
        float angle = 0;
        switch (currentDirection)
        {
            case Direction.Up:
                angle = 90;
                break;
            case Direction.Down:
                angle = 270;
                break;
            case Direction.Left:
                angle = 180;
                break;
            case Direction.Right:
                angle = 0;
                break;
        }
        return angle;
    }

    public static Vector2Int GetDirectionVector(Direction dir)
    {
        Vector2Int gridMoveDirectionVector;
        switch (dir)
        {
            default:
            case Direction.Right: gridMoveDirectionVector = new Vector2Int(+1, 0); break;
            case Direction.Left: gridMoveDirectionVector = new Vector2Int(-1, 0); break;
            case Direction.Up: gridMoveDirectionVector = new Vector2Int(0, +1); break;
            case Direction.Down: gridMoveDirectionVector = new Vector2Int(0, -1); break;
        }
        return gridMoveDirectionVector;
    }

    public static Direction GetOppositeDirection(Direction d)
    {
        Direction result;
        switch (d)
        {
            default:
            case Direction.Right: result = Direction.Left; break;
            case Direction.Left: result = Direction.Right; break;
            case Direction.Up: result = Direction.Down; break;
            case Direction.Down: result = Direction.Up; break;
        }
        return result;
    }
}
