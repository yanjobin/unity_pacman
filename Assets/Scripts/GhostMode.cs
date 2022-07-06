using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class GhostMode : MonoBehaviour
{
    public enum Mode
    {
        idle, Chase, Scatter, Frightened
    }

    public EventHandler<Mode> OnModeChange;

    private Mode nextMode;
    private Mode currentMode;
    public Mode CurrentMode
    {
        get
        {
            return currentMode;
        }
        set
        {
            if (value != currentMode)
            {
                if (currentMode == Mode.Frightened)
                {
                    nextMode = value;
                }
                else
                {
                    ChangeMode(value);
                }
            }
        }
    }

    private float frightenedTimer = 0f;
    private bool nearEndFrighten;

    private void Awake()
    {
        OnModeChange = null;
        nearEndFrighten = false;
    }

    private void ChangeMode(GhostMode.Mode newMode)
    {
        Mode lastMode = currentMode;
        currentMode = newMode;
        OnModeChange?.Invoke(null, lastMode);
    }

    public void Frighten()
    {
        Ghost ghost = GetComponent<Ghost>();

        if (ghost.CurrentState == Ghost.State.ALIVE)
        {
            if (currentMode == Mode.Frightened)
            {
                frightenedTimer = 0;
            }
            else
            {
                nextMode = CurrentMode;
                StartCoroutine(DoFrigthen());
            }
        }
    }

    private IEnumerator DoFrigthen()
    {
        CurrentMode = Mode.Frightened;

        float totalTimeFrightened = LevelManager.FrightenedTime.Last(x => x.Key <= LevelManager.INSTANCE.CurrentLevel).Value;
        frightenedTimer = 0f;
        nearEndFrighten = false;

        while (frightenedTimer < totalTimeFrightened)
        {
            frightenedTimer += Time.deltaTime;

            if (totalTimeFrightened - frightenedTimer < 1)
            {
                nearEndFrighten = true;
            }
            yield return null;
        }

        ChangeMode(nextMode);
        frightenedTimer = 0f;
    }

    public Color getFrigthenedColor()
    {
        if (nearEndFrighten)
        {
            Debug.Log(Mathf.PingPong(Time.time, 0.2f).ToString());
            Color color = Color.Lerp(Color.blue, Color.white, Mathf.PingPong(Time.time * 10, 1));
            return color;
        }
        else
        {
            return Color.blue;
        }
    }

    public Color getFrigthenedColorFace()
    {
        if (nearEndFrighten)
        {
            Debug.Log(Mathf.PingPong(Time.time, 0.2f).ToString());
            Color color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time * 10, 1));
            return color;
        }
        else
        {
            return Color.white;
        }
    }

    public void OnKill()
    {
        StopAllCoroutines();
        frightenedTimer = 0f;
        ChangeMode(nextMode);
    }

    public void Restart(GhostMode.Mode mode)
    {
        StopAllCoroutines();
        frightenedTimer = 0f;
        ChangeMode(mode);
    }
}
