using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Performance")]
    [SerializeField] private float renderDistance;

    [Header("Lighting")]
    public Texture2D worldTilesMap;
    public Material lightShader;
    public float groundLightThreshold;
    public float airLightThreshold;
    public float lightRadius;
    private List<Vector2Int> unlitTiles;

    [Header("Tile Atlas")]
    public TileAtlas tileAtlas;

    [Header("Biomes")]
    public BiomeClass[] biomes;

    [Header("Biomes")]
    public float biomeFreq;
    public Texture2D biomeMap;
    public Gradient biomeGradient;

    [Header("Generation Settings")]
    [SerializeField] private int chunkSize;
    [SerializeField] private int worldSize;
    [SerializeField] public int heightAddition;
    [SerializeField] private bool generateCaves;

    [Header("Noise Settings")]
    [SerializeField] private Texture2D caveNoiseTexture;
    private float seed;

    [Header("Misc")]
    [SerializeField] CinemachineConfiner2D confiner; 
    [SerializeField] private PlayerController player;
    [SerializeField] private PolygonCollider2D worldBoundCollider;
    [SerializeField] private GameObject tileDrop;
    [SerializeField] private GameObject borderTile;
    [SerializeField] private GameObject borderParent;
    [SerializeField] private GameObject lightOverlay;

    [Header("Buffers")]
    private GameObject[] worldChunks;
    private TileClass[,] world_BackgroundTiles;
    private TileClass[,] world_ForegroundTiles;
    private GameObject[,] world_ForegroundObjects;
    private GameObject[,] world_BackgroundObjects;
    private BiomeClass currBiome;
    private Color[] biomeCol;

    [Header("Background")]
    [SerializeField] private GameObject background;
    [SerializeField] private Sprite grasslandBg;
    [SerializeField] private Sprite forestBg;
    [SerializeField] private Sprite desertBg;
    [SerializeField] private Sprite snowBg;
    [SerializeField] private Sprite caveBg;
    private BiomeClass currBiomeBg;

    [Header("Debugging")]
    public TextMeshProUGUI biomeText;
    public TextMeshProUGUI playerPosText;
    public TextMeshProUGUI mousePosText;

    private void OnValidate()
    {
        //DrawTextures();
    }
    private void Start()
    {
        Initialize();
        InitializeLighting();

        SetWorldBoundCollider();
        CreateBorder();
        DrawTextures();
        DrawCavesAndOres();
        GenerateChunks();
        GenerateTerrain();

        LightInitialBlocks(FilterMode.Point);
        OcclusionCulling();

        player.Spawn();
    }
    private void Update()
    {
        HandleResetTerrain();
        OcclusionCulling();
        SetDebugText();
    }

    #region DEBUG
    private void SetDebugText()
    {
        biomeText.text = "Biome: " + GetCurrentBiome(Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt(player.transform.position.y)).biomeName;
        playerPosText.text = $"Player pos: <{Mathf.RoundToInt(player.transform.position.x)}, {Mathf.RoundToInt(player.transform.position.y)}>";
        mousePosText.text = $"Mouse pos: <{Mathf.RoundToInt(player.mousePos.x)}, {Mathf.RoundToInt(player.mousePos.y)}>";
    }
    #endregion

    #region INITIALIZATION
    private void Initialize()
    {
        seed = Random.Range(-1000000f, 1000000f);
        biomeCol = new Color[biomes.Length];
        for (int i = 0; i < biomes.Length; i++)
        {
            biomeCol[i] = biomes[i].biomeColor;
        }

        lightShader.SetFloat("_WorldSize", worldSize);
        lightOverlay.transform.localScale = new Vector2(worldSize, worldSize);
        lightOverlay.transform.position = new Vector2(worldSize / 2, worldSize / 2);
        
        InitializeCollections();
    }
    private void InitializeLighting()
    {
        worldTilesMap = new Texture2D(worldSize, worldSize);

        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                worldTilesMap.SetPixel(x, y, Color.white);
            }
        }
        worldTilesMap.Apply();
    }
    private void LightInitialBlocks(FilterMode filter)
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                if (worldTilesMap.GetPixel(x, y) == Color.white)
                {
                    LightBlock(x, y, 1f, 0);
                }
            }
        }
        worldTilesMap.Apply();
        worldTilesMap.filterMode = filter;
        lightShader.SetTexture("_ShadowTex", worldTilesMap);
    }
    private void InitializeCollections()
    {
        world_ForegroundTiles = new TileClass[worldSize, worldSize];
        world_BackgroundTiles = new TileClass[worldSize, worldSize];
        world_ForegroundObjects = new GameObject[worldSize, worldSize];
        world_BackgroundObjects = new GameObject[worldSize, worldSize];
        worldTilesMap = new Texture2D(worldSize, worldSize);
        unlitTiles = new List<Vector2Int>();
    }
    private void InitializeBackground()
    {
        currBiomeBg = GetCurrentBiome(Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt(player.transform.position.y));
        SwitchBg(GetBackground(GetCurrentBiome(Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt(player.transform.position.y))), 0.2f);
    }
    private void HandleResetTerrain()
    {
        if (Input.GetKeyDown(KeyCode.End))
        {
            foreach (Transform child in this.transform)
            {
                Destroy(child.gameObject);
            }
            seed = Random.Range(-1000000f, 1000000f);

            Initialize();
            InitializeLighting();

            CreateBorder();
            DrawTextures();
            SetWorldBoundCollider();
            DrawCavesAndOres();
            GenerateChunks();
            GenerateTerrain();

            LightInitialBlocks(FilterMode.Point);
            OcclusionCulling();

            player.Spawn();
        }
    }
    #endregion

    #region UI
    private Sprite GetBackground(BiomeClass biome)
    {
        if (player.transform.position.y <= 30)
        {
            return caveBg;
        }
        else
        {
            switch (biome.biomeName.Replace(" ", "").ToUpper())
            {
                case "GRASSLAND":
                    return grasslandBg;
                case "FOREST":
                    return forestBg;
                case "DESERT":
                    return desertBg;
                case "SNOW":
                    return snowBg;
            }
        }
        return null;
    }
    IEnumerator FadeTransition(Sprite nextSprite, float fadeDuration)
    {
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            Color spriteColor = background.GetComponent<SpriteRenderer>().color;
            background.GetComponent<SpriteRenderer>().color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, alpha);
            yield return null;
        }

        background.GetComponent<SpriteRenderer>().sprite = nextSprite;

        timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            Color spriteColor = background.GetComponent<SpriteRenderer>().color;
            background.GetComponent<SpriteRenderer>().color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, alpha);
            yield return null;
        }
    }
    private void SwitchBg(Sprite bg, float fadeDuration)
    {
        StartCoroutine(FadeTransition(bg, fadeDuration));
    }
    private void UpdateBackground(float duration)
    {
        BiomeClass newBiome = GetCurrentBiome(Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt(player.transform.position.y));
        if (newBiome.biomeName != currBiomeBg.biomeName)
        {
            currBiomeBg = newBiome;
            SwitchBg(GetBackground(currBiomeBg), duration);
        }
    }
    #endregion

    #region OPTIMIZATION
    private void OcclusionCulling()
    {
        for (int i = 0; i < worldChunks.Length; i++)
        {
            if (Vector2.Distance(new Vector2((i * chunkSize) + (chunkSize / 2), 0), new Vector2(player.transform.position.x, 0)) > Camera.main.orthographicSize * renderDistance)
                worldChunks[i].SetActive(false);
            else
                worldChunks[i].SetActive(true);
        }
    }
    #endregion

    #region TEXTURES
    private void DrawCavesAndOres()
    {
        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        Color[] caveColors = new Color[caveNoiseTexture.width * caveNoiseTexture.height];
        for (int x = 0; x < caveNoiseTexture.width; x++)
        {
            for (int y = 0; y < caveNoiseTexture.height; y++)
            {
                currBiome = GetCurrentBiome(x, y);
                float v = Mathf.PerlinNoise((x + seed) * currBiome.caveFrequency, (y + seed) * currBiome.caveFrequency);
                caveColors[y * caveNoiseTexture.width + x] = (v > currBiome.surfaceValue) ? Color.white : Color.black;

                for (int i = 0; i < currBiome.ores.Length; i++)
                {
                    float o = Mathf.PerlinNoise((x + seed) * currBiome.ores[i].rarity, (y + seed) * currBiome.ores[i].rarity);
                    if (o > currBiome.ores[i].veinSize)
                        currBiome.ores[i].spreadTexture.SetPixel(x, y, Color.white);
                }
            }
        }
        caveNoiseTexture.SetPixels(caveColors);
        caveNoiseTexture.Apply();

        for (int i = 0; i < currBiome.ores.Length; i++)
        {
            currBiome.ores[i].spreadTexture.Apply();
        }
    }
    private void DrawTextures()
    {
        biomeMap = new Texture2D(worldSize, worldSize);

        for (int i = 0; i < biomes.Length; i++)
        {

            biomes[i].caveNoiseTexture = new Texture2D(worldSize, worldSize);
            for (int o = 0; o < biomes[i].ores.Length; o++)
            {
                biomes[i].ores[o].spreadTexture = new Texture2D(worldSize, worldSize);
                GenerateNoiseTextures(biomes[i].ores[o].rarity, biomes[i].ores[o].veinSize, biomes[i].ores[o].spreadTexture);
            }
        }
    }
    private void GenerateNoiseTextures(float frequency, float limit, Texture2D noiseTexture)
    {
        float v, b;
        Color col;
        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++) 
            { 
                v = Mathf.PerlinNoise((x + seed) * frequency, (y + seed) * frequency);
                b = Mathf.PerlinNoise((x + seed) * biomeFreq, (y + seed) * biomeFreq);
                col = biomeGradient.Evaluate(b);
                biomeMap.SetPixel(x, y, col);   

                if (v > limit)
                    noiseTexture.SetPixel(x, y, Color.white);
                else
                    noiseTexture.SetPixel(x, y, Color.black);
            }
        }
        noiseTexture.Apply();
        biomeMap.Apply();
    }
    #endregion

    #region TERRAIN GENERATION
    private void SetWorldBoundCollider()
    {
        confiner.InvalidateCache();
        worldBoundCollider.points = new[] { new Vector2(worldSize - 1, worldSize), new Vector2(0, worldSize), new Vector2(0, 0), new Vector2(worldSize - 1, 0) };
        confiner.m_BoundingShape2D = worldBoundCollider;
    }
    void CreateBorder()
    {
        int borderThickness = 3;
        // Top border
        for (int x = -borderThickness; x < worldSize + borderThickness; x++)
        {
            for (int y = worldSize; y < worldSize + borderThickness; y++)
            {
                GameObject t = Instantiate(borderTile, borderParent.transform);
                t.transform.position = new Vector2(x, y);
                t.AddComponent<BoxCollider2D>();
            }
        }

        // Bottom border
        for (int x = -borderThickness; x < worldSize + borderThickness; x++)
        {
            for (int y = -borderThickness - 1; y < 0; y++)
            {
                GameObject t = Instantiate(borderTile, borderParent.transform);
                t.transform.position = new Vector2(x, y);
                t.AddComponent<BoxCollider2D>();
            }
        }

        // Left border
        for (int x = -borderThickness - 1; x < 0; x++)
        {
            for (int y = -borderThickness; y < worldSize + borderThickness; y++)
            {
                GameObject t = Instantiate(borderTile, borderParent.transform);
                t.transform.position = new Vector2(x + 0.5f, y);
                t.AddComponent<BoxCollider2D>();
            }
        }

        // Right border
        for (int x = worldSize; x < worldSize + borderThickness; x++)
        {
            for (int y = -borderThickness; y < worldSize + borderThickness; y++)
            {
                GameObject t = Instantiate(borderTile, borderParent.transform);
                t.transform.position = new Vector2(x - 0.5f, y);
                t.AddComponent<BoxCollider2D>();
            }
        }
    }
    private void GenerateChunks()
    {
        int numChunks = worldSize / chunkSize;
        worldChunks = new GameObject[numChunks];

        for (int i = 0; i < numChunks; i++)
        {
            GameObject newChunk = new GameObject();
            newChunk.name = i.ToString();
            newChunk.transform.parent = this.transform;
            worldChunks[i] = newChunk;
        }
    }
    private void GenerateTerrain()
    {
        TileClass tileClass;
        for (int x = 0; x < worldSize - 1; x++) 
        {
            float height;

            for (int y = 0; y < worldSize; y++)
            {
                currBiome = GetCurrentBiome(x, y);
                height = Mathf.PerlinNoise((x + seed) * currBiome.terrainFrequency, seed * currBiome.terrainFrequency) * currBiome.heightMultiplier + heightAddition;

                if (x == worldSize / 2)
                    player.spawnPos = new Vector2(x, height + 3);

                if (y >= height)
                    break;

                if (y < height - currBiome.dirtLayerHeight)
                {
                    tileClass = currBiome.tileAtlas.stone;

                    if (currBiome.ores[0].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > currBiome.ores[0].maxSpawnHeight)
                        tileClass = currBiome.tileAtlas.aluminumOre;
                    if (currBiome.ores[1].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > currBiome.ores[1].maxSpawnHeight)
                        tileClass = currBiome.tileAtlas.ironOre;
                    if (currBiome.ores[2].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > currBiome.ores[2].maxSpawnHeight)
                        tileClass = currBiome.tileAtlas.goldOre;
                    if (currBiome.ores[3].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > currBiome.ores[3].maxSpawnHeight)
                        tileClass = currBiome.tileAtlas.titaniumOre;
                }
                else if (y < height - 1)
                {
                    tileClass = currBiome.tileAtlas.dirt;
                }
                else
                {
                    // Top most layer
                    tileClass = currBiome.tileAtlas.grass;
                }

                if (generateCaves)
                {
                    if (caveNoiseTexture.GetPixel(x, y).r > 0.5f)
                    {
                        PlaceTile(tileClass, x, y, true);
                    }
                    else if(tileClass.wall != null)
                    { 
                        PlaceTile(tileClass.wall, x, y, true);
                    }
                }
                else
                {
                    PlaceTile(tileClass, x, y, true);   
                }

                if (y >= height - 1)
                {
                    int t = Random.Range(0, currBiome.treeChance);
                    if (t == 1 && GetTileFromWorld(x, y) != null)
                    {
                        // Generate tree
                        GenerateTree(Random.Range(currBiome.minTreeHeight, currBiome.maxTreeHeight), x, y + 1);
                    }
                    else
                    {
                        int i = Random.Range(0, currBiome.foliageChance);
                        if (i == 1 && GetTileFromWorld(x, y) != null)
                        {
                            if (currBiome.tileAtlas.foliage != null) 
                                PlaceTile(currBiome.tileAtlas.foliage, x, y + 1, true);
                        }
                    }
                }
            }
        }
        worldTilesMap.Apply();
    }
    private BiomeClass GetCurrentBiome(int x,  int y)
    {
        if (System.Array.IndexOf(biomeCol, biomeMap.GetPixel(x, y)) >= 0)
            return biomes[System.Array.IndexOf(biomeCol, biomeMap.GetPixel(x, y))];

        return currBiome;
    }
    private void GenerateTree(int treeHeight, int x, int y)
    {
        PlaceTile(currBiome.tileAtlas.treeStump, x, y, true);
        for (int i = 1; i <= treeHeight; i++)
        {
            PlaceTile(currBiome.tileAtlas.log, x, y + i, true);
        }

        PlaceTile(currBiome.tileAtlas.treeTop, x, y + treeHeight + 1, true);

        PlaceTile(currBiome.tileAtlas.leaf, 3, x, y + treeHeight + 2, true, false);
        PlaceTile(currBiome.tileAtlas.leaf, 1, x, y + treeHeight + 3, true, false);

        PlaceTile(currBiome.tileAtlas.leaf, 5, x - 1, y + treeHeight + 1, true, false);
        PlaceTile(currBiome.tileAtlas.leaf, 4, x - 1, y + treeHeight + 2, true, true);
        PlaceTile(currBiome.tileAtlas.leaf, 0, x - 1, y + treeHeight + 3, true, false);

        PlaceTile(currBiome.tileAtlas.leaf, 6, x + 1, y + treeHeight + 1, true, false);
        PlaceTile(currBiome.tileAtlas.leaf, 4, x + 1, y + treeHeight + 2, true, false);
        PlaceTile(currBiome.tileAtlas.leaf, 2, x + 1, y + treeHeight + 3, true, false);
    }
    private bool CheckTileSurroundings(int x, int y)
    {
        return GetTileFromWorld(x, y - 1) != null || GetTileFromWorld(x, y + 1) != null || GetTileFromWorld(x - 1, y) != null || GetTileFromWorld(x + 1, y) != null;
    }
    private bool WithinWorldBounds(int x, int y)
    {
        return x >= 0 && x <= worldSize && y >= 0 && y <= worldSize;
    }
    private void AddTileToWorld(int x, int y, TileClass tile)
    {
        if (tile.isInBackground)
        {
            world_BackgroundTiles[x, y] = tile;
        }
        else
        {
            world_ForegroundTiles[x, y] = tile;
        }
    }
    private void RemoveTileFromWorld(int x, int y)
    {
        if (world_ForegroundTiles[x, y] != null)
        {
            world_ForegroundTiles[x, y] = null;
        }
        else if (world_BackgroundTiles[x, y] != null)
        {
            world_BackgroundTiles[x, y] = null;
        }
    }
    private TileClass GetTileFromWorld(int x, int y)
    {
        if (world_ForegroundTiles[x, y] != null)
        {
            return world_ForegroundTiles[x, y];
        }
        else if (world_BackgroundTiles[x, y] != null)
        {
            return world_BackgroundTiles[x, y];
        }
        return null;
    }
    private GameObject GetObjectFromWorld(int x, int y)
    {
        if (world_ForegroundObjects[x, y] != null)
        {
            return world_ForegroundObjects[x, y];
        }
        else if (world_BackgroundObjects[x, y] != null)
        {
            return world_BackgroundObjects[x, y];
        }
        return null;
    }
    private void RemoveObjectFromWorld(int x, int y)
    {
        if (world_ForegroundObjects[x, y] != null)
        {
            world_ForegroundObjects[x, y] = null;
        }
        else if (world_BackgroundObjects[x, y] != null)
        {
            world_BackgroundObjects[x, y] = null;
        }
    }
    private void AddObjectToWorld(int x, int y, GameObject tileObj, TileClass tile)
    { 
        if (tile.isInBackground)
        {
            world_BackgroundObjects[x, y] = tileObj;
        }
        else
        {
            world_ForegroundObjects[x, y] = tileObj;
        }
    }
    public void PlaceTile(TileClass tile, int x, int y, bool isNaturallyPlaced)
    {
        if (WithinWorldBounds(x, y))
        {
            GameObject newTile = new GameObject();

            int chunkCoordinate = Mathf.RoundToInt(Mathf.Round(x / chunkSize) * chunkSize);
            chunkCoordinate /= chunkSize;
            newTile.transform.parent = worldChunks[chunkCoordinate].transform;

            newTile.AddComponent<SpriteRenderer>();

            int spriteIndex = Random.Range(0, tile.sprites.Length);
            newTile.GetComponent<SpriteRenderer>().sprite = tile.sprites[spriteIndex];

            worldTilesMap.SetPixel(x, y, Color.black);
            if (tile.isInBackground)
            {
                newTile.GetComponent<SpriteRenderer>().sortingOrder = -10;
                if (tile.name.ToLower().Contains("wall"))
                {
                    newTile.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
                }
                else
                {
                    worldTilesMap.SetPixel(x, y, Color.white);
                }
            }
            else
            {
                newTile.GetComponent<SpriteRenderer>().sortingOrder = -5;
                newTile.AddComponent<BoxCollider2D>();
                newTile.GetComponent<BoxCollider2D>().size = Vector2.one;
                newTile.tag = "Ground";
            }
                

            newTile.name = tile.sprites[0].name;
            newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

            TileClass newTC = TileClass.CreateInstance(tile, isNaturallyPlaced);

            AddObjectToWorld(x, y, newTile, newTC);
            AddTileToWorld(x, y, newTC);
        }
    }
    // Place tile with a given index
    public void PlaceTile(TileClass tile, int spriteIndex, int x, int y, bool isNaturallyPlaced, bool flipped)
    {
        if (WithinWorldBounds(x, y))
        {
            GameObject newTile = new GameObject();

            int chunkCoordinate = Mathf.RoundToInt(Mathf.Round(x / chunkSize) * chunkSize);
            chunkCoordinate /= chunkSize;
            newTile.transform.parent = worldChunks[chunkCoordinate].transform;

            if (flipped)
                newTile.transform.localScale = new Vector2(-newTile.transform.localScale.x, newTile.transform.localScale.y);

            newTile.AddComponent<SpriteRenderer>();

            newTile.GetComponent<SpriteRenderer>().sprite = tile.sprites[spriteIndex];

            worldTilesMap.SetPixel(x, y, Color.black);
            if (tile.isInBackground)
            {
                newTile.GetComponent<SpriteRenderer>().sortingOrder = -10;
                if (tile.name.ToLower().Contains("wall"))
                {
                    newTile.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
                }
                else
                {
                    worldTilesMap.SetPixel(x, y, Color.white);
                }
            }
            else
            {
                newTile.GetComponent<SpriteRenderer>().sortingOrder = -5;
                newTile.AddComponent<BoxCollider2D>();
                newTile.GetComponent<BoxCollider2D>().size = Vector2.one;
                newTile.tag = "Ground";
            }


            newTile.name = tile.sprites[0].name;
            newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

            TileClass newTC = TileClass.CreateInstance(tile, isNaturallyPlaced);

            AddObjectToWorld(x, y, newTile, newTC);
            AddTileToWorld(x, y, newTC);
        }
    }
    // Place tile with fixed sprite
    public void PlaceTile(TileClass tile, int x, int y, bool isNaturallyPlaced, bool fixedSprite)
    {
        if (WithinWorldBounds(x, y))
        {
            GameObject newTile = new GameObject();

            int chunkCoordinate = Mathf.RoundToInt(Mathf.Round(x / chunkSize) * chunkSize);
            chunkCoordinate /= chunkSize;
            newTile.transform.parent = worldChunks[chunkCoordinate].transform;

            newTile.AddComponent<SpriteRenderer>();

            if (fixedSprite)
                newTile.GetComponent<SpriteRenderer>().sprite = tile.tileDrop.sprites[0];

            worldTilesMap.SetPixel(x, y, Color.black);
            if (tile.isInBackground)
            {
                newTile.GetComponent<SpriteRenderer>().sortingOrder = -10;
                if (tile.name.ToLower().Contains("wall"))
                {
                    newTile.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
                }
                else
                {
                    worldTilesMap.SetPixel(x, y, Color.white);
                }
            }
            else
            {
                newTile.GetComponent<SpriteRenderer>().sortingOrder = -5;
                newTile.AddComponent<BoxCollider2D>();
                newTile.GetComponent<BoxCollider2D>().size = Vector2.one;
                newTile.tag = "Ground";
            }


            newTile.name = tile.sprites[0].name;
            newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

            TileClass newTC = TileClass.CreateInstance(tile, isNaturallyPlaced);

            AddObjectToWorld(x, y, newTile, newTC);
            AddTileToWorld(x, y, newTC);
        }
    }
    #endregion

    #region TERRAIN MANIPULATION
    public void PlaceTile(TileClass tile, int x, int y, PlayerController player)
    {
        if (WithinWorldBounds(x, y))
        {
            // Alow player to place tile only if there is another tile next to target
            if (GetTileFromWorld(x, y) == null && CheckTileSurroundings(x, y))
            {
                // Place tile regardless
                RemoveLightSource(x, y);
                PlaceTile(tile, x, y, false, true);
            }
            else
            {
                // If tile is in background and not solid
                TileClass tileTemp = GetTileFromWorld(x, y);
                if (tileTemp != null && tileTemp.isInBackground && !tileTemp.isSolid)
                {
                    // Overwrite existing tile
                    RemoveTile(x, y);
                    RemoveLightSource(x, y);
                    PlaceTile(tile, x, y, false, true);
                }
            }
        }
    }
    public void RemoveTile(int x, int y)
    {
        if (GetTileFromWorld(x, y) != null && WithinWorldBounds(x, y))
        {
            TileClass tc = GetTileFromWorld(x, y);

            if (tc.wall != null)
            {
                if (tc.naturallyPlaced)
                    PlaceTile(tc.wall, x, y, true);
            }


            if (tc.doesDrop)
            {
                GameObject td = Instantiate(tileDrop, new Vector2(x, y + 0.5f), Quaternion.identity);
                td.GetComponent<SpriteRenderer>().sprite = tc.tileDrop.sprites[0];
                ItemClass tiledropItem = new ItemClass(tc);
                td.GetComponent<TileDropController>().item = tiledropItem;
            }
            
            RemoveTileFromWorld(x, y);
            if (!GetTileFromWorld(x, y)) 
            {
                worldTilesMap.SetPixel(x, y, Color.white);
                LightBlock(x, y, 1f, 0);
                worldTilesMap.Apply();
            }

            Destroy(GetObjectFromWorld(x, y));
            RemoveObjectFromWorld(x, y);
        }
    }
    #endregion

    #region LIGHTING
    private void LightBlock(int x, int y, float intensity, int iteration)
    {
        if (iteration < lightRadius)
        {
            worldTilesMap.SetPixel(x, y, Color.white * intensity);

            float thresh = groundLightThreshold;
            if (x >= 0 && x < worldSize && y >= 0 && y < worldSize)
            {
                if (world_ForegroundTiles[x, y])
                    thresh = groundLightThreshold;
                else
                    thresh = airLightThreshold;
            }

            for (int nx = x - 1; nx < x + 2; nx++)
            {
                for (int ny = y - 1; ny < y + 2; ny++)
                {
                    if (nx != x || ny != y)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(nx, ny));

                        float targetIntensity = Mathf.Pow(thresh, dist) * intensity;
                        if (worldTilesMap.GetPixel(nx, ny) != null)
                        {
                            if (worldTilesMap.GetPixel(nx, ny).r < targetIntensity)
                            {
                                LightBlock(nx, ny, targetIntensity, iteration + 1);
                            }
                        }
                    }
                }
            }
            worldTilesMap.Apply();
        }
    }
    private void RemoveLightSource(int x, int y)
    {
        unlitTiles.Clear();
        UnLightBlock(x, y, x, y);

        HashSet<Vector2Int> toRelight = new HashSet<Vector2Int>();

        foreach (Vector2Int block in unlitTiles)
        {
            Color blockColor = worldTilesMap.GetPixel(block.x, block.y);

            for (int nx = Mathf.Max(block.x - 1, 0); nx <= Mathf.Min(block.x + 1, worldTilesMap.width - 1); nx++)
            {
                for (int ny = Mathf.Max(block.y - 1, 0); ny <= Mathf.Min(block.y + 1, worldTilesMap.height - 1); ny++)
                {
                    if (nx != block.x || ny != block.y)
                    {
                        Color neighborColor = worldTilesMap.GetPixel(nx, ny);
                        if (neighborColor.r > blockColor.r)
                        {
                            toRelight.Add(new Vector2Int(nx, ny));
                        }
                    }
                }
            }
        }

        foreach (Vector2Int source in toRelight)
        {
            Color pixelColor = worldTilesMap.GetPixel(source.x, source.y);
            LightBlock(source.x, source.y, pixelColor.r, 0);
        }

        worldTilesMap.Apply();
    }
    private void UnLightBlock(int x, int y, int ix, int iy)
    {
        if (Mathf.Abs(x - ix) >= lightRadius || Mathf.Abs(y - iy) >= lightRadius || unlitTiles.Contains(new Vector2Int(x, y)))
            return;

        for (int nx = x - 1; nx < x + 2; nx++)
        {
            for (int ny = y - 1; ny < y + 2; ny++)
            {
                if (nx != x || ny != y)
                {
                    if (worldTilesMap.GetPixel(nx, ny) != null)
                    {
                        if (worldTilesMap.GetPixel(nx, ny).r < worldTilesMap.GetPixel(x, y).r)
                        {
                            UnLightBlock(nx, ny, ix, iy);
                        }
                    }
                }
            }
        }

        worldTilesMap.SetPixel(x, y, Color.black);
        unlitTiles.Add(new Vector2Int(x, y));
    }
    #endregion

    #region UNOPTIMIZED METHODS
    private void RemoveLightSourceDeprecated(int x, int y)
    {
        unlitTiles.Clear();
        UnLightBlock(x, y, x, y);

        List<Vector2Int> toRelight = new List<Vector2Int>();

        foreach (Vector2Int block in unlitTiles)
        {
            for (int nx = block.x - 1; nx < block.x + 2; nx++)
            {
                for (int ny = block.y - 1; ny < block.y + 2; ny++)
                {
                    if (worldTilesMap.GetPixel(nx, ny) != null)
                    {
                        if (worldTilesMap.GetPixel(nx, ny).r > worldTilesMap.GetPixel(block.x, block.y).r)
                        {
                            if (!toRelight.Contains(new Vector2Int(nx, ny)))
                            {
                                toRelight.Add(new Vector2Int(nx, ny));
                            }
                        }
                    }
                }
            }
        }

        foreach (Vector2Int source in toRelight)
        {
            LightBlock(source.x, source.y, worldTilesMap.GetPixel(source.x, source.y).r, 0);
        }

        worldTilesMap.Apply();
    }
    private void DrawCavesAndOresDeprecated()
    {
        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        float v, o;
        for (int x = 0; x < caveNoiseTexture.width; x++)
        {
            for (int y = 0; y < caveNoiseTexture.height; y++)
            {
                currBiome = GetCurrentBiome(x, y);
                v = Mathf.PerlinNoise((x + seed) * currBiome.caveFrequency, (y + seed) * currBiome.caveFrequency);
                if (v > currBiome.surfaceValue)
                    caveNoiseTexture.SetPixel(x, y, Color.white);
                else
                    caveNoiseTexture.SetPixel(x, y, Color.black);

                for (int i = 0; i < currBiome.ores.Length; i++)
                {
                    currBiome.ores[i].spreadTexture.SetPixel(x, y, Color.black);
                    if (currBiome.ores.Length >= i + 1)
                    {
                        o = Mathf.PerlinNoise((x + seed) * currBiome.ores[i].rarity, (y + seed) * currBiome.ores[i].rarity);
                        if (o > currBiome.ores[i].veinSize)
                            currBiome.ores[i].spreadTexture.SetPixel(x, y, Color.white);

                        currBiome.ores[i].spreadTexture.Apply();
                    }
                }
            }
        }
        caveNoiseTexture.Apply();
    }
    #endregion
}