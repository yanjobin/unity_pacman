using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Clyde : Ghost
{
    public void Init(Map map, Vector2Int defaultTargetTile, Pacman pacman)
    {
        base.Init(map, defaultTargetTile, pacman, GetComponent<SpriteRenderer>().color);
        InitDotCounter();
    }

    protected override void setInitialPosition(Map map)
    {
        GetComponent<Movement>().Init(map, new Vector2(15.5f, 17), Movement.Direction.Left);
    }

    protected override Vector2Int findChaseTarget()
    {
        //Target tile is Pacman's tile if pacman is farther than 8 tiles. Otherwise, target is default tile
        Vector2Int pacmanPosition = pacman.GetComponent<Movement>().CurrentGridPosition;
        Vector2Int currentPosition = GetComponent<Movement>().CurrentGridPosition;

        float distance = Vector2Int.Distance(pacmanPosition, currentPosition);

        if (distance > 8f)
        {
            return pacmanPosition;
        }
        else
        {
            return target.DefaultTile;
        }
    }

    public override void InitDotCounter()
    {
        int dotCounter = LevelManager.DotCounters.Last(x => x.Key <= LevelManager.INSTANCE.CurrentLevel).Value.clyde;
        GetComponent<DotCounter>().Init(dotCounter);
    }
}
