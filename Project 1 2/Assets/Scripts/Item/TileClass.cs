using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New tile class", menuName = "Tile Class")]
public class TileClass : ScriptableObject
{
    [Header("Data")]
    public string tileName;
    public Sprite[] sprites;
    public Sprite dropSprite;
    public TileClass wall;

    [Header("Attributes")]
    public int stackSize = 99;
    public bool stackable;
    public bool naturallyPlaced = false;
    public bool isInBackground = false;
    public bool isSolid = true;
    public bool doesDrop = true;

    public static TileClass CreateInstance(TileClass tile, bool naturallyPlaced)
    {
        TileClass thisTile = ScriptableObject.CreateInstance<TileClass>();

        thisTile.Init(tile, naturallyPlaced);

        return thisTile;
    }

    public void Init(TileClass tile, bool naturallyPlaced)
    {
        this.tileName = tile.tileName;
        this.sprites = tile.sprites;
        this.dropSprite = tile.dropSprite;
        this.wall = tile.wall;
        this.isInBackground = tile.isInBackground;
        this.isSolid = tile.isSolid;
        this.doesDrop = tile.doesDrop;
        this.naturallyPlaced = naturallyPlaced;
    }
}