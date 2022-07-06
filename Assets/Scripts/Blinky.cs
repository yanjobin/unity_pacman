using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blinky : Ghost
{
    private Elroy CruisElroy { get; set; }

    public void Init(Map map, Vector2Int defaultTargetTile, Pacman pacman, Clyde clyde)
    {
        base.Init(map, defaultTargetTile, pacman, GetComponent<SpriteRenderer>().color);
        CruisElroy = GetComponent<Elroy>();
        CruisElroy.Init(map, clyde);
    }

    protected override void setInitialPosition(Map map)
    {
        GetComponent<Movement>().Init(map, new Vector2(13.5f, 21), Movement.Direction.Left);
    }

    public override void Restart()
    {
        base.Restart();
        CruisElroy.Restart();
    }

    protected override Vector2Int findChaseTarget()
    {
        //Pacman's current tile
        return pacman.GetComponent<Movement>().CurrentGridPosition;
    }

    protected override Vector2Int findScatterTarget()
    {
        if (CruisElroy.Activated)
        {
            //Cruise elroy mode activated. Blinky will target pacman even in scatter mode.
            return findChaseTarget();
        }
        else
        {
            return base.findScatterTarget();
        }
    }

    protected override float GetCurrentSpeedModifier()
    {
        if (CruisElroy.Activated)
        {
            //Cruise elroy mode activated. Blinky's speed is modified
            return CruisElroy.GetSpeedModifier();
        }
        else
        {
            return base.GetCurrentSpeedModifier();
        }
    }

    public override void InitDotCounter()
    {
        //No dot counter
    }
}
