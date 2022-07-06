using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AssetsManager : MonoBehaviour
{
    public static AssetsManager INSTANCE;

    public Sprite Sprite_White1x1;

    public TileTypeSprite[] TileTypeSprites;

    public Sprite Sprite_Pacman_Open;
    public Sprite Sprite_Pacman_Close;

    public Sprite Sprite_Ghost_Eye_Up;
    public Sprite Sprite_Ghost_Eye_Right;
    public Sprite Sprite_Ghost_Eye_Down;
    public Sprite Sprite_Ghost_Eye_Left;
    public Sprite Sprite_Ghost_Face_Frightened;

    public Sprite Sprite_Pacgomme;

    private void Awake()
    {
        INSTANCE = this;
    }

    public TileTypeSprite GetTileTypeSprite(Map.TileType type)
    {
        return TileTypeSprites.FirstOrDefault(x => x.type == type);
    }

    [Serializable]
    public class TileTypeSprite
    {
        public Map.TileType type;
        public Sprite sprite;
        public Color color;
    }
}
