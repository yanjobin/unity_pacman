using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Elroy : MonoBehaviour
{
    public bool Activated { get; set; }
    public bool LifeLost { get; set; }

    private Movement movement;
    private Map map;
    private Clyde clyde;

    private void Awake()
    {
        Activated = false;
        LifeLost = false;
        movement = GetComponent<Movement>();
    }

    public void Init(Map map, Clyde clyde)
    {
        Activated = false;
        LifeLost = false;
        this.map = map;
        this.clyde = clyde;
        Pacman.INSTANCE.OnLifeLost += OnLifeLost;
        LevelManager.INSTANCE.OnLevelChange += OnNewLevelStart;
    }

    public void Restart()
    {
        Activated = false;
    }

    public void OnNewLevelStart(object sender, EventArgs args)
    {
        LifeLost = false;
    }

    
    void Update()
    {
        if (!GameManager.INSTANCE.GameRunning)
            return;

        if (LifeLost)
        {
            //Life was lost while Blinky was in Elroy mode. Elroy mode will be reactivated only when clyde leaves ghost house
            if (clyde.IsInGhostHouse)
                return;
        }

        var current = LevelManager.Elroy.Last(x => x.Key <= LevelManager.INSTANCE.CurrentLevel).Value;
        int dotsRemaining = map.PacGommesRemaining();

        if (dotsRemaining <= current.dotsLeft1)
        {
            //Activate Cruise Elroy
            Activated = true;
        }
    }

    public float GetSpeedModifier()
    {
        if (Activated)
        {
            var current = LevelManager.Elroy.Last(x => x.Key <= LevelManager.INSTANCE.CurrentLevel).Value;
            int dotsRemaining = map.PacGommesRemaining();

            if (dotsRemaining <= current.dotsLeft2)
            {
                //Stage 2 Cruise Elroy
                return current.speed2;
            }
            else if (dotsRemaining <= current.dotsLeft1)
            {
                //Stage 1 Cruise Elroy
                return current.speed1;
            }
            else
            {
                return LevelManager.Speeds.Last(x => x.Key <= LevelManager.INSTANCE.CurrentLevel).Value.ghost;
            }
        }
        else
        {
            return LevelManager.Speeds.Last(x => x.Key <= LevelManager.INSTANCE.CurrentLevel).Value.ghost;
        }
    }

    private void OnLifeLost(object sender, EventArgs args)
    {
        if (Activated)
        {
            //Life is lost after Blinky has become Cruise Elroy
            LifeLost = true;
            Activated = false;
        }
    }
}
