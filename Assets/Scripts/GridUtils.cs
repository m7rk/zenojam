using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridUtils : MonoBehaviour
{
    public Tilemap tileMap;

    // given a starting tile, return all tiles that can be reached.
    // returns a list which is the path to the tile. The last element in the list is the target tile.
    public List<List<Vector3Int>> BFS(Vector3Int start, int range)
    {
        Vector3Int[] dirs = new Vector3Int[4] { new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0) };

        var queue = new Queue<Vector3Int>();
        var pathTo = new Dictionary<Vector3Int, List<Vector3Int>>();

        // Distance from start is zero.
        pathTo[start] = new List<Vector3Int>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var v = queue.Dequeue();
            // this is an edge tile.
            if(pathTo[v].Count >= range)
            {
                continue;
            }

            foreach (var d in dirs)
            {
                var next = v + d;
                // explore this if it's unexplored and a floor tile
                if (!pathTo.ContainsKey(next) && tileMap.GetTile(next) != null && tileMap.GetTile(next).name == "FloorBase")
                {
                    // this tile was unexplored.
                    var pathToThis = new List<Vector3Int>(pathTo[v]);
                    pathToThis.Add(next);
                    pathTo[next] = pathToThis;
                    queue.Enqueue(next);
                }
            }
        }

        var results = new List<List<Vector3Int>>();
        // convert to list
        foreach (var n in pathTo)
        {
            results.Add(n.Value);
        }

        return results;
    }
}
