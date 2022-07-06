using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pacman : MonoBehaviour
{
    public static Pacman INSTANCE;

    public EventHandler OnLivesChange;
    public EventHandler OnLifeLost;

    private float lastSwitchSpriteTime;
    private static float SWITCH_SPRITE_DELAY = 0.1f;
    private bool mouth = false;

    private int lives;
    public int Lives
    {
        get
        {
            return lives;
        }
        set
        {
            int oldValue = lives;
            lives = value;
            if (oldValue > lives)
            {
                OnLifeLost?.Invoke(null, EventArgs.Empty);
            }
            OnLivesChange?.Invoke(null, EventArgs.Empty);
        }
    }

    private Map map;
    private Movement movement;

    private void Awake()
    {
        INSTANCE = this;
        OnLivesChange = null;
        OnLifeLost = null;

        movement = GetComponent<Movement>();
        movement.OnChangeDirection += OnChangeDirection;
    }

    public void Init(Map map)
    {
        this.map = map;
        movement.Init(map, new Vector2(13.5f, 9), Movement.Direction.Left);
        movement.OnEnterNewTile += map.OnPacManEnterNewTile;
        Lives = 3;
    }

    public void Restart()
    {
        movement.Restart();
    }

    private void Update()
    {
        UpdateSpeed();
        HandleInputs();
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (movement.IsMoving)
        {
            if (GameManager.INSTANCE.GameTimer > lastSwitchSpriteTime + SWITCH_SPRITE_DELAY)
            {
                mouth = !mouth;
                lastSwitchSpriteTime = GameManager.INSTANCE.GameTimer;
            }

            if (mouth)
            {
                GetComponent<SpriteRenderer>().sprite = AssetsManager.INSTANCE.Sprite_Pacman_Open;
            }
            else
            {
                GetComponent<SpriteRenderer>().sprite = AssetsManager.INSTANCE.Sprite_Pacman_Close;
            }
            
        }
        else
        {
            GetComponent<SpriteRenderer>().sprite = AssetsManager.INSTANCE.Sprite_Pacman_Close;
        }
    }

    private void UpdateSpeed()
    {
        if (GhostManager.INSTANCE.FrightenedMode)
        {
            movement.CurrentSpeed = Movement.SPEED_DEFAULT * LevelManager.Speeds.Last(x => x.Key <= LevelManager.INSTANCE.CurrentLevel).Value.pacmanFrighten;
        }
        else
        {
            movement.CurrentSpeed = Movement.SPEED_DEFAULT * LevelManager.Speeds.Last(x => x.Key <= LevelManager.INSTANCE.CurrentLevel).Value.pacman;
        }        
    }

    private void HandleInputs()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            movement.DesiredDirection = Movement.Direction.Up;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            movement.DesiredDirection = Movement.Direction.Down;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            movement.DesiredDirection = Movement.Direction.Left;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            movement.DesiredDirection = Movement.Direction.Right;
        }
    }

    private void OnChangeDirection(object sender, System.EventArgs e)
    {
        float angle = movement.GetDirectionRotation();
        float angleY = 0;

        if (angle == 180)
        {
            angle = 0;
            angleY = 180;
        }

        transform.eulerAngles = new Vector3(0, angleY, angle);
    }
}
