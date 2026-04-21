using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class ArenaPathfinder
{
    private struct Node
    {
        
        public Vector2Int cell;

        // g = cost from start to this node, h = heuristic cost from this node to goal, f = g + h
        public int g;
        public int h;
        public int f => g + h;
        public Vector2Int parent;

        public Node(Vector2Int cell, int g, int h, Vector2Int parent)
        {
            this.cell = cell;
            this.g = g;
            this.h = h;
            this.parent = parent;
        }
    }

    public static List<Vector2Int> GetPath(Tilemap wallTilemap, Vector3 startWorld, Vector3 goalWorld)
    {
        // Convert world positions to cell positions
        Vector3Int startCell3 = wallTilemap.WorldToCell(startWorld);
        Vector3Int goalCell3 = wallTilemap.WorldToCell(goalWorld);

        // Use only x and y for pathfinding
        Vector2Int start = new Vector2Int(startCell3.x, startCell3.y);
        Vector2Int goal = new Vector2Int(goalCell3.x, goalCell3.y);

        // converts start and goal to the nearest walkable cell
        start = FindNearestWalkableCell(wallTilemap, start);
        goal = FindNearestWalkableCell(wallTilemap, goal);

        if (start.x < 0 || goal.x < 0)
            return null;

        BoundsInt bounds = wallTilemap.cellBounds;

        List<Node> open = new List<Node>();
        HashSet<Vector2Int> closed = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, int> gScore = new Dictionary<Vector2Int, int>();

        open.Add(new Node(start, 0, Heuristic(start, goal), start));
        gScore[start] = 0;

        // walkable directions
        Vector2Int[] dirs = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        // A* algorithm
        while (open.Count > 0)
        {
            // find node in open with lowest cost
            int bestIndex = 0;
            for (int i = 1; i < open.Count; i++)
            {
                if (open[i].f < open[bestIndex].f || (open[i].f == open[bestIndex].f && open[i].h < open[bestIndex].h))
                    bestIndex = i;
            }

            Node current = open[bestIndex];
            open.RemoveAt(bestIndex);

            if (current.cell == goal)
                return ReconstructPath(cameFrom, current.cell);

            closed.Add(current.cell);

            // check neighbors and calculate costs 
            for (int i = 0; i < dirs.Length; i++)
            {
                Vector2Int neighbor = current.cell + dirs[i];

                if (!bounds.Contains(new Vector3Int(neighbor.x, neighbor.y, 0)))
                    continue;

                if (wallTilemap.HasTile(new Vector3Int(neighbor.x, neighbor.y, 0)))
                    continue;

                if (closed.Contains(neighbor))
                    continue;

                int tentativeG = current.g + 1;

                // if this path to neighbor is better than any previous one, record it
                if (!gScore.TryGetValue(neighbor, out int existingG) || tentativeG < existingG)
                {
                    cameFrom[neighbor] = current.cell;
                    gScore[neighbor] = tentativeG;

                    int h = Heuristic(neighbor, goal);
                    open.Add(new Node(neighbor, tentativeG, h, current.cell));
                }
            }
        }

        Debug.LogWarning("No path found from " + start + " to " + goal);
        return null;
    }

    private static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new List<Vector2Int> { current };

        while (cameFrom.TryGetValue(current, out Vector2Int parent))
        {
            current = parent;
            path.Add(current);
        }

        path.Reverse();
        return path;
    }

    private static int Heuristic(Vector2Int a, Vector2Int b)
    {
        // Manhattan distance heuristic for grid-based pathfinding
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private static Vector2Int FindNearestWalkableCell(Tilemap wallTilemap, Vector2Int start)
    {
        if (!wallTilemap.HasTile(new Vector3Int(start.x, start.y, 0)))
            return start;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        // walkable directions
        Vector2Int[] dirs = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        BoundsInt bounds = wallTilemap.cellBounds;
        
        // breadth-first search to find nearest walkable cell
        while (queue.Count > 0)
        {
            Vector2Int cell = queue.Dequeue();

            if (!wallTilemap.HasTile(new Vector3Int(cell.x, cell.y, 0)))
                return cell;

            for (int i = 0; i < dirs.Length; i++)
            {
                Vector2Int next = cell + dirs[i];
                if (!bounds.Contains(new Vector3Int(next.x, next.y, 0)))
                    continue;

                if (visited.Add(next))
                    queue.Enqueue(next);
            }
        }

        return new Vector2Int(-1, -1);
    }
}