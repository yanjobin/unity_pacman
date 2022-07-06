using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Map : MonoBehaviour
{
    private static Vector2Int TILE_TELEPORT_LEFT = new Vector2Int(0, 18);
    private static Vector2Int TILE_TELEPORT_RIGHT = new Vector2Int(27, 18);
    public static Vector2Int TILE_GHOST_HOUSE = new Vector2Int(14, 19);

    /// <summary>
    /// Special intersections in the map where ghosts cannot go up when in Chase mode.
    /// </summary>
    public static List<Vector2Int> SPECIAL_TILES_UP_CONSTRAINT = new List<Vector2Int>() {
        new Vector2Int(12, 21),
        new Vector2Int(15, 21),
        new Vector2Int(12, 9),
        new Vector2Int(15, 9)
    };

    public enum TileType
    {
        NULL = 'n',
        TELEPORT = 't',
        SUPERPACGOMME = 's',
        PACGOMME = 'f',
        PATH = 'p',
        GATE = 'g',
        INVISIBLEWALL = 'i',
        WALL = 'w'
    }

    public int TileX = 28;
    public int TileY = 36;

    private MapTile[][] map;
    private Dictionary<Vector2Int, GameItem> items;

    private GameObject GameItems;

    private void Awake()
    {
        GameItems = transform.Find("GameItems").gameObject;

        InitMapArray();
        FillMapArray();
        NewLevel(); ;
    }

    /// <summary>
    /// Set map ready for a next level
    /// </summary>
    public void NewLevel()
    {
        CreateItems();
    }

    private void InitMapArray()
    {
        //Create map array of size TileX, TileY
        map = new MapTile[TileY][];
        for (int i = 0; i < TileY; i++)
        {
            map[i] = new MapTile[TileX];
        }
    }

    private void FillMapArray()
    {
        int ax = 0;
        int ay = 0;

        for (int i = 0; i < MapFiles.DefaultMap.Count; i++)
        {
            ax = i % TileX;
            ay = (int)(i / (float)TileX);
            map[ay][ax] = CreateMapTile(MapFiles.DefaultMap[i], ax, ay);
        }
    }

    private MapTile CreateMapTile(TileType type, int x, int y)
    {
        return new MapTile(type, x, y, this.gameObject);
    }

    private void CreateItems()
    {
        if (items != null)
        {
            foreach (GameItem o in items.Values)
            {
                GameObject.Destroy(o.gameItemObject);
            }
        }

        items = new Dictionary<Vector2Int, GameItem>();
        for (int y = 0; y < TileY; y++)
        {
            for (int x = 0; x < TileX; x++)
            {
                switch (map[y][x].type)
                {
                    case TileType.PACGOMME:
                        items[new Vector2Int(x, y)] = new Pacgomme(x, y, GameItems);
                        break;
                    case TileType.SUPERPACGOMME:
                        items[new Vector2Int(x, y)] = new SuperPacgomme(x, y, GameItems);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Checks current tile. If teleport tile, change the location
    /// </summary>
    /// <param name="position"></param>
    /// <param name="direction"></param>
    /// <returns>New tile</returns>
    public Vector2Int OnEnterCenterTile(Vector2Int gridPosition, Movement.Direction direction)
    {
        TileType tileType = map[gridPosition.y][gridPosition.x].type;
        Vector2Int newGridPosition = gridPosition;

        if (tileType == TileType.TELEPORT)
        {
            if (gridPosition == TILE_TELEPORT_LEFT && direction == Movement.Direction.Left)
            {
                newGridPosition = TILE_TELEPORT_RIGHT;
            }
            else if (gridPosition == TILE_TELEPORT_RIGHT && direction == Movement.Direction.Right)
            {
                newGridPosition = TILE_TELEPORT_LEFT;
            }
        }

        return newGridPosition;
    }

    public Vector2Int GetGridPositionAt(Vector3 position)
    {
        return new Vector2Int((int)Math.Floor(position.x + 0.5f), (int)Math.Floor(position.y + 0.5f));
    }

    public bool IsInCenterTile(Vector3 position)
    {
        Vector2 corrected = new Vector2(position.x + 0.5f, position.y + 0.5f);
        double withinTileX = Mathf.Floor(corrected.x) == 0 ? corrected.x : corrected.x % Mathf.Floor(corrected.x);
        double withinTileY = Mathf.Floor(corrected.y) == 0 ? corrected.y : corrected.y % Mathf.Floor(corrected.y);

        //If value is clamped, then it was outside bounds, so outside center tile
        float clampedX = Mathf.Clamp((float)withinTileX, 0.5f - (MapTile.CENTER_TILE_SIZE / 2), 0.5f + (MapTile.CENTER_TILE_SIZE / 2));
        float clampedY = Mathf.Clamp((float)withinTileY, 0.5f - (MapTile.CENTER_TILE_SIZE / 2), 0.5f + (MapTile.CENTER_TILE_SIZE / 2));

        if (withinTileX == clampedX && withinTileY == clampedY)
        {
            //Given position is within defined center of the tile
            return true;
        }

        return false;
    }

    public bool IsAllowedGridPosition(Vector2Int gridPosition, bool canGoThroughGate = false)
    {
        //Out of bounds
        if (!IsInBounds(gridPosition)) return false;

        TileType type = map[gridPosition.y][gridPosition.x].type;

        return type == TileType.PATH || type == TileType.TELEPORT || type == TileType.PACGOMME || type == TileType.SUPERPACGOMME || (type == TileType.GATE && canGoThroughGate);
    }

    public List<(Movement.Direction, TileType)> GetPossibleDirectionsAtGrid(Vector2Int gridPosition, bool goThroughGate = false)
    {
        List<(Movement.Direction, TileType)> result = new List<(Movement.Direction, TileType)>();
        foreach (Movement.Direction d in Enum.GetValues(typeof(Movement.Direction)))
        {
            Vector2Int directionVector = Movement.GetDirectionVector(d);
            Vector2Int newGridPosition = gridPosition + directionVector;
            if (IsAllowedGridPosition(newGridPosition, goThroughGate))
            {
                TileType type = map[newGridPosition.y][newGridPosition.x].type;
                result.Add((d, type));
            }
        }
        return result;
    }

    private bool IsInBounds(Vector2Int gridPosition)
    {
        if (gridPosition.x < 0 || gridPosition.x >= TileX) return false;
        if (gridPosition.y < 0 || gridPosition.y >= TileY) return false;
        return true;
    }

    public TileType GetTileTypeAt(Vector2Int gridPosition)
    {
        if (!IsInBounds(gridPosition)) return TileType.NULL;
        return map[gridPosition.y][gridPosition.x].type;
    }

    public void OnPacManEnterNewTile(object sender, EventArgs e)
    {
        Movement movement = sender as Movement;

        Ghost ghost = GhostManager.INSTANCE.GetGhostAtGridPosition(movement.CurrentGridPosition);
        if (ghost != null)
        {
            OnCollidePacmanGhost(Pacman.INSTANCE, ghost);
        }

        if (items.ContainsKey(movement.CurrentGridPosition))
        {
            GameItem item = items[movement.CurrentGridPosition];
            items.Remove(movement.CurrentGridPosition);
            item.OnInteract?.Invoke(null, EventArgs.Empty);
        }
    }    

    public void OnGhostEnterNewTile(object sender, EventArgs e)
    {
        Movement movement = sender as Movement;

        if (Pacman.INSTANCE.GetComponent<Movement>().CurrentGridPosition == movement.CurrentGridPosition)
        {
            OnCollidePacmanGhost(Pacman.INSTANCE, movement.GetComponent<Ghost>());
        }
    }

    private void OnCollidePacmanGhost(Pacman pacman, Ghost ghost)
    {
        //Ghost is dead, no need to go further
        if (ghost.CurrentState == Ghost.State.DEAD)
            return;

        if (ghost.GetComponent<GhostMode>().CurrentMode == GhostMode.Mode.Frightened)
        {
            ghost.Kill();
            GameManager.INSTANCE.OnEatGhostPause();
        }
        else
        {
            GameManager.INSTANCE.OnPacmanDie();
        }
    }

    public bool IsLevelCompleted()
    {
        bool pacgommeRemaining = false;
        pacgommeRemaining = items.Count(x => x.Value.IsWinCondition()) > 0;
        return !pacgommeRemaining;
    }

    public int PacGommesRemaining()
    {
        return items.Count(x => x.Value.IsWinCondition());
    }

    private class MapTile
    {
        public static float CENTER_TILE_SIZE = 1f / 5f;

        public TileType type { get; private set; }
        private int tileX;
        private int tileY;
        public Transform transform { get; private set; }

        public MapTile(TileType type, int x, int y, GameObject parent)
        {
            this.type = type;
            this.tileX = x;
            this.tileY = y;

            GameObject newTile = new GameObject("Tile", typeof(SpriteRenderer));
            newTile.GetComponent<SpriteRenderer>().sprite = AssetsManager.INSTANCE.GetTileTypeSprite(type).sprite;
            newTile.GetComponent<SpriteRenderer>().color = AssetsManager.INSTANCE.GetTileTypeSprite(type).color;
            newTile.GetComponent<SpriteRenderer>().sortingLayerName = "Background";
            newTile.transform.SetParent(parent.transform);
            newTile.transform.position = new Vector3(x, y);
            this.transform = newTile.transform;
        }
    }

    private abstract class GameItem
    {
        protected int tileX;
        protected int tileY;
        public GameObject gameItemObject;

        public EventHandler OnInteract;

        protected GameItem()
        {
            OnInteract = OnInteraction;
        }

        private void OnInteraction(object sender, EventArgs args)
        {
            DoInteraction();
            GameObject.Destroy(gameItemObject);
        }

        protected abstract void DoInteraction();
        public abstract bool IsWinCondition();
    }

    private class Pacgomme : GameItem
    {
        private float size = 0.3f;

        public Pacgomme(int x, int y, GameObject parent)
        {
            this.tileX = x;
            this.tileY = y;

            this.gameItemObject = new GameObject("Pacgomme", typeof(SpriteRenderer));
            this.gameItemObject.GetComponent<SpriteRenderer>().sprite = AssetsManager.INSTANCE.Sprite_Pacgomme;
            this.gameItemObject.GetComponent<SpriteRenderer>().color = Color.yellow;

            this.gameItemObject.transform.position = new Vector3(x, y);
            this.gameItemObject.transform.localScale = new Vector3(size, size);
            this.gameItemObject.transform.SetParent(parent.transform);

            this.OnInteract += GhostManager.INSTANCE.OnEatPacgomme;
        }

        protected override void DoInteraction()
        {
            ScoreManager.Score += 10;
            Pacman.INSTANCE.GetComponent<Movement>().SkipFrames = 1;
        }

        public override bool IsWinCondition()
        {
            return true;
        }
    }

    private class SuperPacgomme : GameItem
    {
        private float size = 0.8f;

        public SuperPacgomme(int x, int y, GameObject parent)
        {
            this.tileX = x;
            this.tileY = y;

            this.gameItemObject = new GameObject("Pacgomme", typeof(SpriteRenderer));
            this.gameItemObject.GetComponent<SpriteRenderer>().sprite = AssetsManager.INSTANCE.Sprite_Pacgomme;
            this.gameItemObject.GetComponent<SpriteRenderer>().color = Color.yellow;
            this.gameItemObject.AddComponent<SuperPacGomme>();

            this.gameItemObject.transform.position = new Vector3(x, y);
            this.gameItemObject.transform.localScale = new Vector3(size, size);
            this.gameItemObject.transform.SetParent(parent.transform);

            this.OnInteract += GhostManager.INSTANCE.OnEatSuperPacGomme;
        }

        protected override void DoInteraction()
        {
            ScoreManager.Score += 50;
            Pacman.INSTANCE.GetComponent<Movement>().SkipFrames = 3;
        }

        public override bool IsWinCondition()
        {
            return true;
        }
    }
}
