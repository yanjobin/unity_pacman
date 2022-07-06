using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Ghost : MonoBehaviour
{
    private static float SPEED_DEAD = 16f;

    public enum State
    {
        ALIVE, DEAD
    }

    public EventHandler OnDie;

    protected TargetTile target;
    protected Pacman pacman;
    protected Map map;

    private Movement movement;
    private SpriteRenderer face;

    public bool IsInGhostHouse { get; set; }
    public bool IsInTunnel { get; set; }
    public bool JustWentThroughGate { get; set; }
    public bool CanGoThroughHouse { get; set; }
    public bool forceReverseDirection { get; set; }
    public State CurrentState { get; set; }

    private Color defaultColor;

    private void Awake()
    {
        target = GetComponent<TargetTile>();
        face = transform.Find("Face").GetComponent<SpriteRenderer>();
        OnDie = null;
    }

    public void Init(Map map, Vector2Int defaultTargetTile, Pacman pacman, Color defaultColor)
    {
        this.pacman = pacman;
        this.map = map;
        this.defaultColor = defaultColor;
        target.Init(map, defaultTargetTile);
        setInitialPosition(map);
        CanGoThroughHouse = false;
        forceReverseDirection = false;
        JustWentThroughGate = false;
        CurrentState = State.ALIVE;

        GhostMode mode = GetComponent<GhostMode>();
        mode.OnModeChange += OnChangeMode;

        movement = GetComponent<Movement>();
        movement.OnEnterNewTile += OnEnterNewTile;
        movement.OnEnterNewTile += map.OnGhostEnterNewTile;

        if (GetComponent<DotCounter>() != null)
        {
            GetComponent<DotCounter>().OnReachDotCounterLimit += OnReachDotCounterLimit;
        }
    }

    virtual public void Restart()
    {
        CurrentState = State.ALIVE;
        CanGoThroughHouse = false;
        forceReverseDirection = false;
        JustWentThroughGate = false;
        movement.Restart();
        InitDotCounter();
    }

    public abstract void InitDotCounter();

    protected abstract void setInitialPosition(Map map);

    public void Kill()
    {
        CurrentState = State.DEAD;
        CanGoThroughHouse = true;
        GetComponent<GhostMode>().OnKill();
        OnDie?.Invoke(null, EventArgs.Empty);
    }

    public void Revive()
    {
        CurrentState = State.ALIVE;
    }

    public void ReleaseFromGhostHouse()
    {
        CanGoThroughHouse = true;
    }

    public void OnReachDotCounterLimit(object sender, EventArgs args)
    {
        //Disable the dot counter and release the ghost from the ghost house
        GetComponent<DotCounter>().Disabled = true;
        ReleaseFromGhostHouse();
    }

    void Update()
    {
        UpdateTarget();
        UpdateSpeed();

        UpdateFaceSprite();
        UpdateBodysprite();
    }

    private void UpdateTarget()
    {
        GhostMode.Mode mode = GetComponent<GhostMode>().CurrentMode;

        switch (CurrentState)
        {
            case State.DEAD:
                target.Tile = Map.TILE_GHOST_HOUSE;
                break;
            case State.ALIVE:
                switch (mode)
                {
                    case GhostMode.Mode.Chase:
                        target.Tile = findChaseTarget();
                        break;
                    case GhostMode.Mode.Scatter:
                        target.Tile = findScatterTarget();
                        break;
                    case GhostMode.Mode.Frightened:
                        break;
                }
                break;
        }
    }

    protected abstract Vector2Int findChaseTarget();

    virtual protected Vector2Int findScatterTarget()
    {
        return target.DefaultTile;
    }

    private void UpdateSpeed()
    {
        switch (CurrentState)
        {
            case State.ALIVE:
                GhostMode.Mode mode = GetComponent<GhostMode>().CurrentMode;
                switch (mode)
                {
                    case GhostMode.Mode.idle:
                        movement.CurrentSpeed = 0f;
                        break;
                    case GhostMode.Mode.Frightened:
                        movement.CurrentSpeed = Movement.SPEED_DEFAULT * LevelManager.Speeds.Last(x => x.Key <= LevelManager.INSTANCE.CurrentLevel).Value.ghostFrighten;
                        break;
                    default:
                        if (IsInTunnel)
                        {
                            movement.CurrentSpeed = Movement.SPEED_DEFAULT * LevelManager.Speeds.Last(x => x.Key <= LevelManager.INSTANCE.CurrentLevel).Value.ghostTunnel;
                        }
                        else
                        {
                            movement.CurrentSpeed = Movement.SPEED_DEFAULT * GetCurrentSpeedModifier();
                        }
                        break;
                }
                break;
            case State.DEAD:
                movement.CurrentSpeed = SPEED_DEAD;
                break;
        }
    }

    virtual protected float GetCurrentSpeedModifier()
    {
        return LevelManager.Speeds.Last(x => x.Key <= LevelManager.INSTANCE.CurrentLevel).Value.ghost;
    }

    private void UpdateFaceSprite()
    {
        Movement.Direction direction = GetComponent<Movement>().CurrentDirection;
        switch (GetComponent<GhostMode>().CurrentMode)
        {
            case GhostMode.Mode.Frightened:
                face.sprite = AssetsManager.INSTANCE.Sprite_Ghost_Face_Frightened;
                face.color = GetComponent<GhostMode>().getFrigthenedColorFace();
                break;
            default:
                switch (direction)
                {
                    case Movement.Direction.Up:
                        face.sprite = AssetsManager.INSTANCE.Sprite_Ghost_Eye_Up;
                        break;
                    case Movement.Direction.Right:
                        face.sprite = AssetsManager.INSTANCE.Sprite_Ghost_Eye_Right;
                        break;
                    case Movement.Direction.Down:
                        face.sprite = AssetsManager.INSTANCE.Sprite_Ghost_Eye_Down;
                        break;
                    case Movement.Direction.Left:
                        face.sprite = AssetsManager.INSTANCE.Sprite_Ghost_Eye_Left;
                        break;
                }
                face.color = Color.white;
                break;
        }
    }

    private void UpdateBodysprite()
    {
        switch (CurrentState)
        {
            case State.ALIVE:
                switch (GetComponent<GhostMode>().CurrentMode)
                {
                    case GhostMode.Mode.Frightened:
                        GetComponent<SpriteRenderer>().color = GetComponent<GhostMode>().getFrigthenedColor();
                        break;
                    default:
                        GetComponent<SpriteRenderer>().color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 1f);
                        break;
                }
                break;
            case State.DEAD:
                GetComponent<SpriteRenderer>().color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0f);
                break;
        }
    }

    public void OnEnterNewTile(object sender, EventArgs args)
    {
        Map.TileType tileType = map.GetTileTypeAt(movement.CurrentGridPosition);

        if (JustWentThroughGate)
        {
            //Forcing Left on exit of the Ghost house, right on enter
            if (IsInGhostHouse)
                movement.ForceDirection = Movement.Direction.Right;
            else
                movement.ForceDirection = Movement.Direction.Left;
            JustWentThroughGate = false;
        }
        else if (CanGoThroughHouse && tileType == Map.TileType.GATE)
        {
            //Ghost was flagged to go through the gate and just entered gate tile.
            CanGoThroughHouse = false;
            JustWentThroughGate = true;
        } 
        else if (!IsInGhostHouse && forceReverseDirection)
        {
            //Only reverse direction if is outside the ghost house
            //Force reverse direction was flagged.
            movement.ForceDirection = Movement.GetOppositeDirection(movement.CurrentDirection);
            forceReverseDirection = false;
        }
    }

    public void OnChangeMode(object sender, GhostMode.Mode lastMode)
    {
        if (lastMode == GhostMode.Mode.Scatter || lastMode == GhostMode.Mode.Chase)
        {
            //Only flag if ghost is alive. If DEAD, ghost is running to the ghost house and we ignore the force left instruction
            if (CurrentState == State.ALIVE)
            {
                //Ghosts are forced to change direction when changing from: chase-to-scatter, chase-to-frightened, scatter-to-chase, and scatter-to-frightened.
                forceReverseDirection = true;
            }
        }
    }

    public void OnEnterGhostHouse()
    {
        if (GetComponent<DotCounter>() != null)
        {
            if (GetComponent<DotCounter>().IsFull)
            {
                StartCoroutine(AllowLeaveGhostHouseDelayed());
            }
        }
        else
        {
            StartCoroutine(AllowLeaveGhostHouseDelayed());
        }

        if (CurrentState == State.DEAD)
        {
            Revive();
        }
    }

    private IEnumerator AllowLeaveGhostHouseDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        ReleaseFromGhostHouse();
    }
}
