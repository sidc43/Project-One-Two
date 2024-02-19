using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class TileAtlas : ScriptableObject
{
    [Header("Environment")]
    public TileClass stone;
    public TileClass dirt;
    public TileClass grass;
    public TileClass log;
    public TileClass leaf;
    public TileClass foliage;
    public TileClass snow;
    public TileClass sand;
    public TileClass treeStump;
    public TileClass treeTop;

    [Header("Ored")]
    public TileClass aluminumOre;
    public TileClass ironOre;
    public TileClass goldOre;
    public TileClass titaniumOre;
}
