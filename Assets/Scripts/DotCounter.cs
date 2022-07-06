using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotCounter : MonoBehaviour
{
    private int dotLimit;
    public int Counter { get; set; }
    public bool Disabled { get; set; }
    public bool IsFull
    {
        get
        {
            return Counter >= dotLimit;
        }
    }

    public EventHandler OnReachDotCounterLimit;

    private void Awake()
    {
        OnReachDotCounterLimit = null;
        Restart();
    }

    public void Init(int dotLimit)
    {
        this.dotLimit = dotLimit;
    }

    public void Restart()
    {
        Counter = 0;
        Disabled = false;
    }
}
