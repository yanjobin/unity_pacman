using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pinky : Ghost
{
    public void Init(Map map, Vector2Int defaultTargetTile, Pacman pacman)
    {
        base.Init(map, defaultTargetTile, pacman, GetComponent<SpriteRenderer>().color);
        InitDotCounter();
    }

    protected override void setInitialPosition(Map map)
    {
        GetComponent<Movement>().Init(map, new Vector2(13.5f, 17), Movement.Direction.Left);
    }

    protected override Vector2Int findChaseTarget()
    {
        //4 tiles ahead of Pacman's current tile
        Movement.Direction pacmanDirection = pacman.GetComponent<Movement>().CurrentDirection;
        Vector2Int pacmanPosition = pacman.GetComponent<Movement>().CurrentGridPosition;
        Vector2Int pacmanDirectionVector = Movement.GetDirectionVector(pacmanDirection);

        if (pacmanDirection == Movement.Direction.Up)
        {
            //Special bug. Overflow error in original game. Target is 4 tiles ahead and 4 tiles to the left
            pacmanDirectionVector.x = -1;
        }

        return  pacmanPosition + pacmanDirectionVector * 4;
    }

    public override void InitDotCounter()
    {
        int dotCounter = LevelManager.DotCounters.Last(x => x.Key <= LevelManager.INSTANCE.CurrentLevel).Value.pinky;
        GetComponent<DotCounter>().Init(dotCounter);
    }
}
