using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OreClass
{
    public string name;
    [Range(0f, 1f)]
    public float rarity;
    [Range(0f, 1f)]
    public float veinSize;
    public int maxSpawnHeight;
    public Texture2D spreadTexture;
}
