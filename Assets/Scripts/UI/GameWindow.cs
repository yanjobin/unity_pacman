using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameWindow : MonoBehaviour
{
    public static GameWindow INSTANCE { get; set; }

    private Text TimerText;
    private Text ScoreText;
    private Text LevelText;
    private GameObject Life1;
    private GameObject Life2;
    private GameObject Life3;

    private void Awake()
    {
        INSTANCE = this;

        TimerText = transform.Find("TimerText").GetComponent<Text>();
        ScoreText = transform.Find("ScoreText").GetComponent<Text>();
        LevelText = transform.Find("LevelText").GetComponent<Text>();
        Life1 = transform.Find("Life1").gameObject;
        Life2 = transform.Find("Life2").gameObject;
        Life3 = transform.Find("Life3").gameObject;
    }

    void Start()
    {
        GameManager.INSTANCE.OnTimeChange += OnTimerChange;
        ScoreManager.OnScoreChange += OnScoreChange;
        LevelManager.INSTANCE.OnLevelChange += OnLevelChange;
        Pacman.INSTANCE.OnLivesChange += OnLivesChange;

        GameManager.INSTANCE.OnTimeChange?.Invoke(null, EventArgs.Empty);
        ScoreManager.OnScoreChange?.Invoke(null, EventArgs.Empty);
        LevelManager.INSTANCE.OnLevelChange?.Invoke(null, EventArgs.Empty);
    }

    public void HideScore()
    {
        ScoreText.gameObject.SetActive(false);
    }

    private void OnTimerChange(object sender, EventArgs args)
    {
        TimerText.text = "Time\n" + GameManager.INSTANCE.FormattedTimer;
    }

    private void OnScoreChange(object sender, EventArgs args)
    {
        ScoreText.text = ScoreManager.Score.ToString();
    }

    private void OnLevelChange(object sender, EventArgs args)
    {
        LevelText.text = "Level\n" + LevelManager.INSTANCE.CurrentLevel;
    }

    private void OnLivesChange(object sender, EventArgs args)
    {
        Life3.gameObject.SetActive(Pacman.INSTANCE.Lives >= 3);
        Life2.gameObject.SetActive(Pacman.INSTANCE.Lives >= 2);
        Life1.gameObject.SetActive(Pacman.INSTANCE.Lives >= 1);
    }
}
