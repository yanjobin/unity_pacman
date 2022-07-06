using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class TargetTile : MonoBehaviour
{
    private Vector2Int defaultTargetTile;
    private Vector2Int targetTile;
    public Vector2Int Tile
    {
        set 
        {
            if (value != targetTile)
            {
                targetTile = value;
            }            
        }
    }
    public Vector2Int DefaultTile
    {
        get
        {
            return defaultTargetTile;
        }
    }

    private Map map;
    private Movement movement;
    private Ghost ghost;

    private void Awake()
    {
        
    }

    public void Init(Map map, Vector2Int defaultTargetTile)
    {
        this.map = map;
        this.defaultTargetTile = defaultTargetTile;
        targetTile = this.defaultTargetTile;

        ghost = GetComponent<Ghost>();
        movement = GetComponent<Movement>();
        movement.OnEnterNewTile += OnEnterNewTile;
    }

    /// <summary>
    /// Logic to determine next direction based on target tile. Event fired when entity enters a new tile
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnEnterNewTile(object sender, System.EventArgs e)
    {
        Vector2Int currentGridPosition = movement.CurrentGridPosition;

        List<(Movement.Direction d, Map.TileType t)> possibleDirections = map.GetPossibleDirectionsAtGrid(currentGridPosition, ghost.CanGoThroughHouse);

        //Remove opposite direction to the current direction so it does not reverse position
        possibleDirections = possibleDirections.Where(x => x.d != Movement.GetOppositeDirection(movement.CurrentDirection)).ToList();

        //Only remove up if ghost is alive. When dead, it can go up on special tiles.
        if (GetComponent<Ghost>().CurrentState == Ghost.State.ALIVE && Map.SPECIAL_TILES_UP_CONSTRAINT.Contains(currentGridPosition))
        {
            //Special condition. Ghost is in one of the 4 special tile where it cannot go up.
            possibleDirections.Remove(possibleDirections.Where(x => x.d == Movement.Direction.Up).FirstOrDefault());
        }

        if (ghost.GetComponent<GhostMode>().CurrentMode == GhostMode.Mode.Frightened)
        {
            //Ghost is frightened. Direction is picked random from all directions (even if not in the list)
            //Is the random direction is NOT a possible direction, pick the first available Up, Left, Down, right
            Type dirType = typeof(Movement.Direction);
            Array values = dirType.GetEnumValues();
            int r = UnityEngine.Random.Range(0, values.Length - 1);
            Movement.Direction randomDirection = (Movement.Direction) values.GetValue(r);

            if (possibleDirections.Exists(x => x.d == randomDirection))
            {
                movement.DesiredDirection = randomDirection;
            }
            else
            {
                foreach (Movement.Direction dir in values)
                {
                    if (possibleDirections.Exists(x => x.d == dir))
                    {
                        movement.DesiredDirection = dir;
                        break;
                    }
                }
            }
        }
        else if (ghost.CanGoThroughHouse && ghost.GetComponent<GhostMode>().CurrentMode != GhostMode.Mode.Frightened && possibleDirections.Exists(x => x.t == Map.TileType.GATE))
        {
            //Ghost is marked as allowed to go through gate, so it goes through gate if the gate is one of the possibilities
            //Only if the current mode is NOT frigthened.
            movement.DesiredDirection = possibleDirections.FirstOrDefault(x => x.t == Map.TileType.GATE).d;
        }
        else if (possibleDirections.Count <= 1)
        {
            //Only 2 directions possible for the current tile, return whichever direction is not current direction
            movement.DesiredDirection = possibleDirections.FirstOrDefault().d;
        }
        else
        {
            //Find direction with the shortest distance in a straight line to target tile.
            movement.DesiredDirection = possibleDirections.OrderBy(x => Vector2Int.Distance(currentGridPosition + Movement.GetDirectionVector(x.d), targetTile)).FirstOrDefault().d;
        }
    }

    public void TargetDefaultTile()
    {
        Tile = defaultTargetTile;
    }
}
