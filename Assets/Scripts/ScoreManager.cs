using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager
{
    private static int score;
    public static int Score
    {
        get
        {
            return score;
        }
        set
        {
            score = value;
            OnScoreChange?.Invoke(null, EventArgs.Empty);
        }
    }

    public static EventHandler OnScoreChange;

    public static void Init()
    {
        Score = 0;
        OnScoreChange = null;
    }

    public static int GetHighscore()
    {
        return PlayerPrefs.GetInt("highscore", 0);
    }

    public static bool TrySetNewHighscore()
    {
        return TrySetNewHighscore(Score);
    }

    public static bool TrySetNewHighscore(int score)
    {
        int highscore = GetHighscore();
        if (score > highscore)
        {
            PlayerPrefs.SetInt("highscore", score);
            PlayerPrefs.Save();
            return true;
        }

        return false;
    }
}
