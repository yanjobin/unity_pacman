using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager INSTANCE;

    public EventHandler OnTimeChange;

    private float gameTimer;
    public float GameTimer
    {
        get
        {
            return gameTimer;
        }
        set
        {
            gameTimer = value;
            OnTimeChange?.Invoke(null, EventArgs.Empty);
        }
    }
    public float DeltaTime { get; set; }
    public String FormattedTimer
    {
        get
        {
            int minutes = Mathf.FloorToInt(gameTimer / 60F);
            int seconds = Mathf.FloorToInt(gameTimer - minutes * 60);
            return string.Format("{0:0}:{1:00}", minutes, seconds);
        }
    }

    private bool isGameRunning;
    public bool GameRunning
    {
        get
        {
            return isGameRunning;
        }
        set
        {
            isGameRunning = value;
        }
    }

    [SerializeField] private Map map;
    [SerializeField] private Pacman pacman;
    [SerializeField] private Blinky blinky;
    [SerializeField] private Pinky pinky;
    [SerializeField] private Inky inky;
    [SerializeField] private Clyde clyde;
    [SerializeField] private GhostManager ghostManager;

    private LevelManager levelManager;

    private void Awake()
    {
        INSTANCE = this;
        OnTimeChange = null;
        GameRunning = false;
        ScoreManager.Init();

        levelManager = new LevelManager();
    }

    private void Start()
    {
        pacman.Init(map);
        pinky.Init(map, new Vector2Int(2, 35), pacman);
        inky.Init(map, new Vector2Int(27, 0), pacman, blinky);
        clyde.Init(map, new Vector2Int(0, 0), pacman);
        blinky.Init(map, new Vector2Int(25, 35), pacman, clyde);

        ghostManager.Init(blinky, pinky, inky, clyde);

        GameRunning = false;
        RestartLevel();
    }

    private void Update()
    {
        if (GameRunning)
        {
            DeltaTime = Time.deltaTime;
            GameTimer += Time.deltaTime;

            CheckWinCondition();
        }
        else
        {
            DeltaTime = 0f;
        }
    }

    private void CheckWinCondition()
    {
        if (map.IsLevelCompleted())
        {
            LevelManager.INSTANCE.CurrentLevel++;
            NextLevel();
        }
    }

    public void OnPacmanDie()
    {
        GameRunning = false;
        Pacman.INSTANCE.Lives--;

        if (Pacman.INSTANCE.Lives <= 0)
        {
            GameWindow.INSTANCE.HideScore();
            GameOverWindow.INSTANCE.Show(ScoreManager.TrySetNewHighscore());
        }
        else
        {
            RestartLevel();
        }
    }

    private IEnumerator DoBeginRound()
    {
        yield return new WaitForSeconds(3f);
        GameRunning = true;
    }

    private void RestartLevel()
    {
        GameTimer = 0f;

        pacman.Restart();
        ghostManager.Restart();

        StartCoroutine(DoBeginRound());
    }

    public void NextLevel()
    {
        GameRunning = false;
        GameTimer = 0f;

        pacman.Restart();
        ghostManager.NewLevel();
        map.NewLevel();

        StartCoroutine(DoBeginRound());
    }

    public void OnEatGhostPause()
    {
        StartCoroutine(DoPauseFor(0.5f));
    }

    private IEnumerator DoPauseFor(float time)
    {
        GameRunning = false;
        yield return new WaitForSeconds(time);
        GameRunning = true;
    }

    public void NewGame()
    {
        GameOverWindow.INSTANCE.Hide();
        ScoreManager.Score = 0;
        levelManager.CurrentLevel = 1;
        Pacman.INSTANCE.Lives = 3;
        NextLevel();
    }
}
