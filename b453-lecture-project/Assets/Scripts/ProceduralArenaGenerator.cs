using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralArenaGenerator : MonoBehaviour
{
    // reference variables
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private GameObject billionaireBasePrefab;
    [SerializeField] private GameObject flagControllerPrefab;
    [SerializeField] private GameObject flagPrefab;

    // arena size
    [SerializeField] private int width = 120;
    [SerializeField] private int height = 80;

    // generation constraints
    [SerializeField] private int randomFillPercent = 45;
    [SerializeField] private int smoothingIterations = 5;
    [SerializeField] private int generationAttempts = 20;
    [SerializeField] private float minimumWalkableCoverage = 0.45f;

    // base placement
    [SerializeField] private float baseRadius = 1.1f;
    [SerializeField] private float billionRadius = 0.25f;
    [SerializeField] private float wallClearanceBuffer = 0.6f;
    [SerializeField] private float minBaseSpacing = 9.0f;

    // spawned assets
    private bool[,] walls;
    private readonly List<Vector2> spawnedBasePositions = new List<Vector2>();
    private readonly List<BillionaireBase> spawnedBases = new List<BillionaireBase>();
    private readonly List<Flag> spawnedFlags = new List<Flag>();
    private readonly List<TeamFlagController> spawnedFlagControllers = new List<TeamFlagController>();
    private System.Random seed;

    private void Start()
    {
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        if (wallTilemap == null || wallTile == null || billionaireBasePrefab == null)
        {
            Debug.LogError("Arena generator is missing references.");
            return;
        }

        seed = new System.Random(UnityEngine.Random.Range(int.MinValue, int.MaxValue));

        // loops through until valid generation
        for (int attempt = 0; attempt < generationAttempts; attempt++)
        {
            ClearExisting();

            // generate arena walls + obstacles
            walls = GenerateArena();
            if (walls == null)
                continue;
            RenderWalls();

            // spawn bases
            if (!SpawnBases())
                continue;

            return;
        }

        Debug.LogError("Failed to generate a valid arena after multiple attempts.");
    }

    private void ClearExisting()
    {
        wallTilemap.ClearAllTiles();

        for (int i = 0; i < spawnedBases.Count; i++)
        {
            if (spawnedBases[i] != null)
                Destroy(spawnedBases[i].gameObject);
        }

        for (int i = 0;i < spawnedFlags.Count;i++)
        {
            if (spawnedFlags[i] != null && spawnedFlagControllers[i] != null)
            {
                FlagManager.Instance.RemoveFlagFromList(spawnedFlags[i], spawnedFlags[i].team);
                Destroy(spawnedFlags[i].gameObject);
                Destroy(spawnedFlagControllers[i].gameObject);
            }
        }

        spawnedBases.Clear();
        spawnedBasePositions.Clear();
        spawnedFlags.Clear();
        spawnedFlagControllers.Clear();
    }

    private bool[,] GenerateArena()
    {
        // 2D array of bools - true for walls
        bool[,] map = new bool[width, height];

        // horizontal borders
        for (int x = 0; x < width; x++)
        {
            map[x, 0] = true;
            map[x, height - 1] = true;
        }

        // vertical borders
        for (int y = 0; y < height; y++)
        {
            map[0, y] = true;
            map[width - 1, y] = true;
        }

        // loops through inner box, uses seed to determine fill
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                map[x, y] = seed.Next(0, 100) < randomFillPercent;
            }
        }

        // smooths out map
        for (int i = 0; i < smoothingIterations; i++)
        {
            map = Smooth(map);
        }

        // checks if map region is walkable
        map = KeepLargestWalkableRegion(map);
        float walkableCoverage = CountWalkable(map) / (float)(width * height);
        if (walkableCoverage < minimumWalkableCoverage)
            return null;

        return map;
    }


    private bool[,] Smooth(bool[,] input)
    {
        bool[,] output = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // sets order bounds
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    output[x, y] = true;
                    continue;
                }

                // cleans generation by removing especially isolated cells
                int wallCount = GetWallNeighbourCount(input, x, y);
                output[x, y] = wallCount > 4;
            }
        }

        return output;
    }

    private int GetWallNeighbourCount(bool[,] map, int x, int y)
    {
        int count = 0;

        // loops through the 8 cells surrounding inputed cell
        for (int nx = x - 1; nx <= x + 1; nx++)
        {
            for (int ny = y - 1; ny <= y + 1; ny++)
            {
                if (nx == x && ny == y)
                    continue;

                // adds count for cells outside the border radius ensuring borders remain intact
                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                {
                    count++;
                    continue;
                }

                if (map[nx, ny])
                    count++;
            }
        }

        return count;
    }

    private bool[,] KeepLargestWalkableRegion(bool[,] map)
    {
        bool[,] visited = new bool[width, height];
        List<Vector2Int> bestRegion = null;

        // checks each unvisited blank cell in map, marks the biggest walkable region
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (map[x, y] || visited[x, y])
                    continue;

                // walks through neigboring cells of starting point and maps the walkable region
                List<Vector2Int> region = FloodFillWalkable(map, visited, new Vector2Int(x, y));
                if (bestRegion == null || region.Count > bestRegion.Count)
                    bestRegion = region;
            }
        }

        if (bestRegion == null || bestRegion.Count == 0)
            return null;

        // generates map with all cells true
        bool[,] result = new bool[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                result[x, y] = true;

        // uses coordinates of blank cells in the best region to rebuild map with only the walkable region
        foreach (Vector2Int cell in bestRegion)
            result[cell.x, cell.y] = false;

        return result;
    }

    private List<Vector2Int> FloodFillWalkable(bool[,] map, bool[,] visited, Vector2Int start)
    {
        // region is the list of coordinates of each blank cell in the map
        List<Vector2Int> region = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(start);
        visited[start.x, start.y] = true;

        while (queue.Count > 0)
        {
            Vector2Int cell = queue.Dequeue();
            region.Add(cell);

            // list of top, right, bottom, and left neighbor of the cell
            Vector2Int[] neighbours = new Vector2Int[]
            {
                new Vector2Int(cell.x + 1, cell.y),
                new Vector2Int(cell.x - 1, cell.y),
                new Vector2Int(cell.x, cell.y + 1),
                new Vector2Int(cell.x, cell.y - 1)
            };

            // checks each neighboring cell for unvisited blank tiles and adds them to the the queue
            for (int i = 0; i < neighbours.Length; i++)
            {
                Vector2Int n = neighbours[i];
                if (n.x < 0 || n.y < 0 || n.x >= width || n.y >= height)
                    continue;

                if (visited[n.x, n.y] || map[n.x, n.y])
                    continue;

                visited[n.x, n.y] = true;
                queue.Enqueue(n);
            }
        }

        return region;
    }

    private int CountWalkable(bool[,] map)
    {
        int count = 0;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (!map[x, y])
                    count++;

        return count;
    }

    private void RenderWalls()
    {
        wallTilemap.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!walls[x, y])
                    continue;

                wallTilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
            }
        }
    }

    private bool SpawnBases()
    {
        spawnedBasePositions.Clear();
        spawnedBases.Clear();
        spawnedFlags.Clear();
        spawnedFlagControllers.Clear();

        RectInt[] regions = GetQuadrantRegions();

        int i = 0;
        foreach (Team team in (Team[])Enum.GetValues(typeof(Team)))
        {
            Vector2 worldPos;
            bool found = TryFindBasePositionInRegion(regions[i], out worldPos);
            if (!found)
                return false;

            GameObject billionaireBaseObj = Instantiate(billionaireBasePrefab, worldPos, Quaternion.identity);
            BillionaireBase billionaireBase = billionaireBaseObj.GetComponent<BillionaireBase>();
            billionaireBase.Initialize(team, 1);
            spawnedBases.Add(billionaireBase);
            spawnedBasePositions.Add(worldPos);

            if (team.Equals(FlagManager.Instance.playerTeam))
                continue;

            GameObject flagObj = Instantiate(flagPrefab, worldPos, Quaternion.identity);
            Flag flag = flagObj.GetComponent<Flag>();
            flag.Initialize(team);
            FlagManager.Instance.AddFlagToList(flag, team);
            spawnedFlags.Add(flag);

            GameObject flagControllerObj = Instantiate(flagControllerPrefab, new Vector2(0, 0), Quaternion.identity);
            TeamFlagController flagController = flagControllerObj.GetComponent<TeamFlagController>();
            flagController.Initialize(flag, team, wallTilemap);
            spawnedFlagControllers.Add(flagController);

            i++;
        }

        return true;
    }

    private RectInt[] GetQuadrantRegions()
    {
        int halfW = width / 2;
        int halfH = height / 2;

        // buffer to keep bases from placing too close to walls
        int inset = Mathf.CeilToInt(baseRadius + billionRadius + wallClearanceBuffer) + 2;

        RectInt bottomLeft = new RectInt(inset, inset, Mathf.Max(1, halfW - inset * 2), Mathf.Max(1, halfH - inset * 2));
        RectInt bottomRight = new RectInt(halfW + inset, inset, Mathf.Max(1, width - (halfW + inset) - inset), Mathf.Max(1, halfH - inset * 2));
        RectInt topLeft = new RectInt(inset, halfH + inset, Mathf.Max(1, halfW - inset * 2), Mathf.Max(1, height - (halfH + inset) - inset));
        RectInt topRight = new RectInt(halfW + inset, halfH + inset, Mathf.Max(1, width - (halfW + inset) - inset), Mathf.Max(1, height - (halfH + inset) - inset));

        return new RectInt[] { topLeft, topRight, bottomLeft, bottomRight };
    }

    private bool TryFindBasePositionInRegion(RectInt region, out Vector2 worldPos)
    {
        int attempts = 250;
        for (int i = 0; i < attempts; i++)
        {
            // random coordinate
            int x = seed.Next(region.xMin, region.xMax);
            int y = seed.Next(region.yMin, region.yMax);

            // checks if random coordinate on tile or border continue
            if (x < 1 || y < 1 || x >= width - 1 || y >= height - 1 || walls[x, y])
                continue;

            // checks if random coordinate is far enough from walls and other bases
            Vector2 candidate = CellCenterWorld(new Vector2Int(x, y));
            if (!HasClearRing(candidate) || !FarEnoughFromOtherBases(candidate))
                continue;

            worldPos = candidate;
            return true;
        }

        worldPos = default;
        return false;
    }

    private Vector2 CellCenterWorld(Vector2Int cell)
    {
        return wallTilemap.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
    }

    private bool HasClearRing(Vector2 center)
    {
        float requiredClearance = baseRadius + billionRadius + wallClearanceBuffer;

        // checks angles around the center to see if they are walkable (blank cells)
        for (int i = 0; i < 24; i++)
        {
            float angle = (i / (float)24) * Mathf.PI * 2f;
            Vector2 sample = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * requiredClearance;

            if (!IsWalkableWorld(sample))
                return false;
        }

        return true;
    }

    private bool FarEnoughFromOtherBases(Vector2 candidate)
    {
        float minSpacingSqr = minBaseSpacing * minBaseSpacing;

        // checks if each base is farther than the minimum spacing
        for (int i = 0; i < spawnedBasePositions.Count; i++)
        {
            if ((spawnedBasePositions[i] - candidate).sqrMagnitude < minSpacingSqr)
                return false;
        }

        return true;
    }

    private bool IsWalkableWorld(Vector2 worldPos)
    {
        Vector3Int cell = wallTilemap.WorldToCell(worldPos);

        // border check
        if (cell.x < 0 || cell.y < 0 || cell.x >= width || cell.y >= height)
            return false;

        return !walls[cell.x, cell.y];
    }
}