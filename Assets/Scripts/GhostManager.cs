using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class GhostManager : MonoBehaviour
{
    public static GhostManager INSTANCE;

    private static Dictionary<int, int> EatBonusPoint = new Dictionary<int, int> {
        {  1,  200 },
        {  2,  400 },
        {  3,  800 },
        {  4, 1600 },
    };
    private int EatGhostCounter { get; set; }

    public bool LifeLost { get; set; }

    public bool FrightenedMode { get; private set; }

    private List<Ghost> ghosts;

    private GhostMode.Mode currentMode;

    private Pinky pinky;
    private Inky inky;
    private Clyde clyde;
    private Blinky blinky;

    private int GlobalCounter { get; set; }
    private List<DotCounter> dotCounters;
    private float currentDotTimerControl;

    private bool timerStopped;
    private float internalTimer;

    private void Awake()
    {
        if (INSTANCE == null)
        {
            INSTANCE = this;
        }
    }

    private void Start()
    {
        timerStopped = false;
        FrightenedMode = false;
        internalTimer = 0;
        currentMode = GhostMode.Mode.Scatter;
        ghosts.ForEach(x => x.GetComponent<GhostMode>().CurrentMode = currentMode);
        ghosts.ForEach(x => x.GetComponent<Ghost>().OnDie += OnEatGhost);
        GlobalCounter = 0;
        currentDotTimerControl = 0;
    }

    public void Init(Blinky blinky, Pinky pinky, Inky inky, Clyde clyde)
    {
        ghosts = new List<Ghost>() { blinky, pinky, inky, clyde };
        this.blinky = blinky;
        this.pinky = pinky;
        this.inky = inky;
        this.clyde = clyde;

        dotCounters = new List<DotCounter>();
        dotCounters.Add(pinky.GetComponent<DotCounter>());
        dotCounters.Add(inky.GetComponent<DotCounter>());
        dotCounters.Add(clyde.GetComponent<DotCounter>());
        GlobalCounter = 0;

        Pacman.INSTANCE.OnLifeLost += OnLifeLost;
        LevelManager.INSTANCE.OnLevelChange += OnNewLevelStart;
    }

    public void Restart()
    {
        StopAllCoroutines();
        internalTimer = 0;
        currentDotTimerControl = 0;
        timerStopped = false;
        FrightenedMode = false;
        currentMode = GhostMode.Mode.Scatter;
        ghosts.ForEach(x => x.GetComponent<GhostMode>().Restart(currentMode));
        ghosts.ForEach(x => x.GetComponent<Ghost>().Restart());
    }

    public void NewLevel()
    {
        Restart();
        ghosts.Where(x => x.GetComponent<DotCounter>() != null).ToList().ForEach(x => x.GetComponent<DotCounter>().Restart());
    }

    private void Update()
    {
        currentDotTimerControl += GameManager.INSTANCE.DeltaTime;
        var dotControl = LevelManager.DotTimeControl.Last(x => x.Key <= LevelManager.INSTANCE.CurrentLevel).Value;
        if (currentDotTimerControl >= dotControl)
        {
            currentDotTimerControl = 0;
            ReleaseNextAvailableGhost();
        }

        //If GhostManager timer is stopped, do not update modes timer. Timer will be stopped during Frightened mode (Eating super pacgomme). also ignore dot counter releasing
        if (timerStopped)
            return;

        internalTimer += GameManager.INSTANCE.DeltaTime;

        var timers = LevelManager.GhostModeTimers.Last(x => x.Key <= LevelManager.INSTANCE.CurrentLevel);
        GhostMode.Mode newMode = timers.Value.Last(x => internalTimer >= x.time).phase;

        currentMode = newMode;
        ghosts.ForEach(x => x.GetComponent<GhostMode>().CurrentMode = currentMode);

        if (!LifeLost)
        {
            foreach (DotCounter dc in dotCounters)
            {
                if (!dc.GetComponent<Ghost>().IsInGhostHouse)
                {
                    //Not in ghost house, don't care
                    continue;
                }

                if (!dc.Disabled)
                {
                    if (dc.IsFull)
                    {
                        dc.OnReachDotCounterLimit?.Invoke(null, EventArgs.Empty);
                    }
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Called to release the next ghost in the house. Called when pacman has not eaten a pacgomme in some time.
    /// </summary>
    private void ReleaseNextAvailableGhost()
    {
        ghosts.FirstOrDefault(x => x.IsInGhostHouse)?.ReleaseFromGhostHouse();
    }

    public Ghost GetGhostAtGridPosition(Vector2Int gridPosition)
    {
        return ghosts.FirstOrDefault(x => x.GetComponent<Movement>().CurrentGridPosition == gridPosition);
    }

    public void OnEatPacgomme(object sender, EventArgs args)
    {
        //Reset timer controlling last time pacman ate a pacgomme
        currentDotTimerControl = 0;
        if (LifeLost)
        {
            //Life was lost, global counter is being used
            GlobalCounter++;

            if (GlobalCounter == 7)
            {
                inky.GetComponent<DotCounter>().OnReachDotCounterLimit?.Invoke(null, EventArgs.Empty);
            }
            else if (GlobalCounter == 17)
            {
                pinky.GetComponent<DotCounter>().OnReachDotCounterLimit?.Invoke(null, EventArgs.Empty);
            }
            else if (GlobalCounter == 32)
            {
                if (clyde.IsInGhostHouse)
                {
                    //If Clyde is in ghost house when global dot counter reaches 32, deactivate it and reactivate ghosts personal counters
                    LifeLost = false;
                    GlobalCounter = 0;
                }
            }
        }
        else
        {
            DotCounter currentCounter = dotCounters.FirstOrDefault(x => !x.Disabled && x.GetComponent<Ghost>().IsInGhostHouse);
            if (currentCounter != null)
            {
                currentCounter.Counter++;
            }
        }
    }

    public void OnEatSuperPacGomme(object sender, EventArgs args)
    {
        EatGhostCounter = 0;
        ghosts.ForEach(x => x.GetComponent<GhostMode>().Frighten());
        StartCoroutine(StopGameWhileFrightened());
    }

    private void OnEatGhost(object sender, EventArgs args)
    {
        EatGhostCounter++;
        int points = EatBonusPoint.Last(x => x.Key <= EatGhostCounter).Value;
        ScoreManager.Score += points;
    }

    private IEnumerator StopGameWhileFrightened()
    {
        timerStopped = true;
        FrightenedMode = true;
        yield return new WaitWhile(() => ghosts.Exists(x => x.GetComponent<GhostMode>().CurrentMode == GhostMode.Mode.Frightened));
        timerStopped = false;
        FrightenedMode = false;
    }

    private void OnLifeLost(object sender, EventArgs args)
    {
        LifeLost = true;
        GlobalCounter = 0;
    }

    private void OnNewLevelStart(object sender, EventArgs args)
    {
        LifeLost = false;
    }
}
