using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BiomeClass
{
    [Header("General Settings")]
    public string biomeName;

    [Header("Tiles")]
    public TileAtlas tileAtlas;

    [Header("Noise Settings")]
    public Color biomeColor;
    public float terrainFrequency = 0.05f;

    [Tooltip("Higher value yields more caves at a smaller size, lower value yields fewer caves at a greater size")]
    public float caveFrequency = 0.05f;
    public Texture2D caveNoiseTexture;

    [Header("Generation Settings")]
    public bool generateCaves = true;
    public int dirtLayerHeight = 5;
    public float surfaceValue = 0.25f;
    public float heightMultiplier = 4f;

    [Header("Trees")]
    public int treeChance = 10;
    public int minTreeHeight = 4;
    public int maxTreeHeight = 6;

    [Header("Foliage")]
    public int foliageChance = 10;

    [Header("Ore Settings")]
    public OreClass[] ores;

    public override string ToString()
    {
        return this.biomeName;
    }
}
