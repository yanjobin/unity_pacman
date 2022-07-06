using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inky : Ghost
{
    private Ghost blinky;

    public void Init(Map map, Vector2Int defaultTargetTile, Pacman pacman, Ghost blinky)
    {
        base.Init(map, defaultTargetTile, pacman, GetComponent<SpriteRenderer>().color);
        this.blinky = blinky;
        InitDotCounter();
    }

    protected override void setInitialPosition(Map map)
    {
        GetComponent<Movement>().Init(map, new Vector2(11.5f, 17), Movement.Direction.Left);
    }

    protected override Vector2Int findChaseTarget()
    {
        //Target tile is determined by taking the vector from Blink's position to 2 tiles ahead of Pacman, and doubling it
        Movement.Direction pacmanDirection = pacman.GetComponent<Movement>().CurrentDirection;
        Vector2Int pacmanPosition = pacman.GetComponent<Movement>().CurrentGridPosition;
        Vector2Int pacmanDirectionVector = Movement.GetDirectionVector(pacmanDirection);

        if (pacmanDirection == Movement.Direction.Up)
        {
            //Special bug. Overflow error in original game. Target is 2 tiles ahead and 2 tiles to the left before doubling
            pacmanDirectionVector.x = -1;
        }

        Vector2Int twoTilesInFrontOfPacman = pacmanPosition + pacmanDirectionVector * 2;

        Vector3 blinkyPosition = blinky.transform.position;
        Vector3 blinkyToPacmanVector = new Vector3(twoTilesInFrontOfPacman.x - blinkyPosition.x, twoTilesInFrontOfPacman.y - blinkyPosition.y);

        return map.GetGridPositionAt(blinkyPosition + blinkyToPacmanVector * 2);
    }

    public override void InitDotCounter()
    {
        int dotCounter = LevelManager.DotCounters.Last(x => x.Key <= LevelManager.INSTANCE.CurrentLevel).Value.inky;
        GetComponent<DotCounter>().Init(dotCounter);
    }
}
