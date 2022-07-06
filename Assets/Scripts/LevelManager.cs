using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager
{
    public static LevelManager INSTANCE;

    public EventHandler OnLevelChange;

    private int currentLevel;
    public int CurrentLevel
    {
        get
        {
            return currentLevel;
        }
        set
        {
            currentLevel = value;
            OnLevelChange?.Invoke(null, EventArgs.Empty);
        }
    }

    public LevelManager()
    {
        if (INSTANCE == null)
        {
            INSTANCE = this;
            OnLevelChange = null;
            CurrentLevel = 1;
        }
    }

    #region Ghost Mode Phases

    public static Dictionary<int, List<(GhostMode.Mode phase, float time)>> GhostModeTimers = new Dictionary<int, List<(GhostMode.Mode phase, float time)>> {
        {
            1,  new List<(GhostMode.Mode phase, float time)>()
                {
                    (GhostMode.Mode.Scatter,         0f), //7
                    (GhostMode.Mode.Chase,           7f), //20
                    (GhostMode.Mode.Scatter,        27f), //7
                    (GhostMode.Mode.Chase,          34f), //20
                    (GhostMode.Mode.Scatter,        54f), //5
                    (GhostMode.Mode.Chase,          59f), //20
                    (GhostMode.Mode.Scatter,        79f), //5
                    (GhostMode.Mode.Chase,          84f), //99999
                }
        },
        {
            2,  new List<(GhostMode.Mode phase, float time)>()
                {
                    (GhostMode.Mode.Scatter,         0f), //7
                    (GhostMode.Mode.Chase,           7f), //20
                    (GhostMode.Mode.Scatter,        27f), //7
                    (GhostMode.Mode.Chase,          34f), //20
                    (GhostMode.Mode.Scatter,        54f), //5
                    (GhostMode.Mode.Chase,          59f), //1033
                    (GhostMode.Mode.Scatter,      1092f), //1/60
                    (GhostMode.Mode.Chase,        1092f + (1f/60f)), //99999
                }
        },
        {
            5,  new List<(GhostMode.Mode phase, float time)>()
                {
                    (GhostMode.Mode.Scatter,         0f), //5
                    (GhostMode.Mode.Chase,           5f), //20
                    (GhostMode.Mode.Scatter,        25f), //5
                    (GhostMode.Mode.Chase,          30f), //20
                    (GhostMode.Mode.Scatter,        50f), //5
                    (GhostMode.Mode.Chase,          55f), //1037
                    (GhostMode.Mode.Scatter,      1092f), //1/60
                    (GhostMode.Mode.Chase,        1092f + (1f/60f)), //99999
                }
        },
    };

    #endregion

    #region FrightenedTime

    public static Dictionary<int, float> FrightenedTime = new Dictionary<int, float> {
        {  1, 6f },
        {  2, 5f },
        {  3, 4f },
        {  4, 3f },
        {  5, 2f },
        {  6, 5f },
        {  7, 2f },
        {  8, 2f },
        {  9, 1f },
        { 10, 5f },
        { 11, 2f },
        { 12, 1f },
        { 13, 1f },
        { 14, 3f },
        { 15, 1f },
        { 16, 1f },
        { 17, 1f },
        { 18, 1f },
        { 19, 1f },
        { 20, 1f },
        { 21, 1f },
    };

    #endregion

    #region Speeds

    public static Dictionary<int, (float pacman, float ghost, float ghostTunnel, float ghostFrighten, float pacmanFrighten)> Speeds = new Dictionary<int, (float pacman, float ghost, float ghostTunnel, float ghostFrighten, float pacmanFrighten)> {
        {  1, (0.80f, 0.75f, 0.40f, 0.50f, 0.90f) },
        {  2, (0.90f, 0.85f, 0.45f, 0.55f, 0.95f) },
        {  3, (0.90f, 0.85f, 0.45f, 0.55f, 0.95f) },
        {  4, (0.90f, 0.85f, 0.45f, 0.55f, 0.95f) },
        {  5, (1.00f, 0.95f, 0.50f, 0.60f, 1.00f) },
        {  6, (1.00f, 0.95f, 0.50f, 0.60f, 1.00f) },
        {  7, (1.00f, 0.95f, 0.50f, 0.60f, 1.00f) },
        {  8, (1.00f, 0.95f, 0.50f, 0.60f, 1.00f) },
        {  9, (1.00f, 0.95f, 0.50f, 0.60f, 1.00f) },
        { 10, (1.00f, 0.95f, 0.50f, 0.60f, 1.00f) },
        { 11, (1.00f, 0.95f, 0.50f, 0.60f, 1.00f) },
        { 12, (1.00f, 0.95f, 0.50f, 0.60f, 1.00f) },
        { 13, (1.00f, 0.95f, 0.50f, 0.60f, 1.00f) },
        { 14, (1.00f, 0.95f, 0.50f, 0.60f, 1.00f) },
        { 15, (1.00f, 0.95f, 0.50f, 0.60f, 1.00f) },
        { 16, (1.00f, 0.95f, 0.50f, 0.60f, 1.00f) },
        { 17, (1.00f, 0.95f, 0.50f, 0.60f, 1.00f) },
        { 18, (1.00f, 0.95f, 0.50f, 0.60f, 1.00f) },
        { 19, (1.00f, 0.95f, 0.50f, 0.60f, 1.00f) },
        { 20, (1.00f, 0.95f, 0.50f, 0.60f, 1.00f) },
        { 21, (0.90f, 0.95f, 0.50f, 1.00f, 0.90f) },
    };

    #endregion

    #region Cruise Elroy

    public static Dictionary<int, (int dotsLeft1, float speed1, int dotsLeft2, float speed2)> Elroy = new Dictionary<int, (int dotsLeft1, float speed1, int dotsLeft2, float speed2)> {
        {  1, ( 20, 0.80f, 10, 0.85f) },
        {  2, ( 30, 0.90f, 15, 0.95f) },
        {  3, ( 40, 0.90f, 20, 0.95f) },
        {  4, ( 40, 0.90f, 20, 0.95f) },
        {  5, ( 40, 1.00f, 20, 1.05f) },
        {  6, ( 50, 1.00f, 25, 1.05f) },
        {  7, ( 50, 1.00f, 25, 1.05f) },
        {  8, ( 50, 1.00f, 25, 1.05f) },
        {  9, ( 60, 1.00f, 30, 1.05f) },
        { 10, ( 60, 1.00f, 30, 1.05f) },
        { 11, ( 60, 1.00f, 30, 1.05f) },
        { 12, ( 80, 1.00f, 40, 1.05f) },
        { 13, ( 80, 1.00f, 40, 1.05f) },
        { 14, ( 80, 1.00f, 40, 1.05f) },
        { 15, (100, 1.00f, 50, 1.05f) },
        { 16, (100, 1.00f, 50, 1.05f) },
        { 17, (100, 1.00f, 50, 1.05f) },
        { 18, (100, 1.00f, 50, 1.05f) },
        { 19, (120, 1.00f, 60, 1.05f) },
        { 20, (120, 1.00f, 60, 1.05f) },
        { 21, (120, 1.00f, 60, 1.05f) },
    };

    #endregion

    #region Dot Counters

    public static Dictionary<int, (int pinky, int inky, int clyde)> DotCounters = new Dictionary<int, (int pinky, int inky, int clyde)> {
        {  1, (0, 30, 60) },
        {  2, (0,  0, 50) },
        {  3, (0,  0,  0) },
    };

    public static Dictionary<int, float> DotTimeControl = new Dictionary<int, float> {
        {  1, 4 },
        {  5, 3 },
    };

    #endregion
}
