using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    public GridUtils gu;
    public GameState gs;

    public Tile[] tiles;
    public GameObject[] npcPrefabs;

    // tweak map gen with these
    public readonly int DIMS = 15;
    public readonly float FILL_THRESH = 0.7f;
    public readonly float PERLIN_MULT = 0.2f;
    public readonly float FALLOFF_FACTOR = 0.3f;

    public readonly int MIN_VIABLE_TILES = 40;

    // Start is called before the first frame update
    void Awake()
    {
        generateMapCandidate();

        // always place player at middle... removes all islands.
        var reachableToPlayer = gu.reachableTilesFrom(Vector3Int.zero, 100, new HashSet<Vector3Int>());

        while(gu.levelTileMap.GetTile(Vector3Int.zero) == null || reachableToPlayer.Count < MIN_VIABLE_TILES)
        {
            generateMapCandidate();
            reachableToPlayer = gu.reachableTilesFrom(Vector3Int.zero, 100, new HashSet<Vector3Int>());
        }

        reachableToPlayer.Add(Vector3Int.zero);
        // This map works
        cullNonReachable(reachableToPlayer);

        // drop npcs
        gs.NPCPositions = new Dictionary<Vector3Int, GameObject>();
        for (int i = 0; i != 3; ++i)
        {
            GameObject k = Instantiate(npcPrefabs[i]);
            k.transform.SetParent(this.transform);
            k.transform.position = gs.globalPositionForTile(new Vector3Int(-i, -1, 0));
            gs.NPCPositions[new Vector3Int(-i, -1, 0)] = k;
        }

        // place ladder
    }

    void generateMapCandidate()
    {
        var perlinStartX = Random.Range(0f, 100f);
        var perlinStartY = Random.Range(0f, 100f);

        // create map
        gu.levelTileMap.ClearAllTiles();
        for (int x = -DIMS; x != DIMS; ++x)
        {
            for (int y = -DIMS; y != DIMS; ++y)
            {
                var pComp = Mathf.PerlinNoise(perlinStartX + PERLIN_MULT * x, perlinStartY + PERLIN_MULT * y);
                var fallOff = (FALLOFF_FACTOR * ((Mathf.Abs(x) + Mathf.Abs(y)) / (float)DIMS));
                var tVal = pComp + fallOff;

                if (tVal < FILL_THRESH)
                {
                    var targTile = (int)((tiles.Length * tVal) / FILL_THRESH);
                    if (targTile >= tiles.Length)
                    {
                        targTile = tiles.Length - 1;
                    }
                    gu.levelTileMap.SetTile(new Vector3Int(x, y, 0), tiles[targTile]); // Or use SetTiles() for multiple tiles.
                }
            }
        }
    }

    void cullNonReachable(List<Vector3Int> reachable)
    {
        for (int x = -DIMS; x != DIMS; ++x)
        {
            for (int y = -DIMS; y != DIMS; ++y)
            {
                if (!reachable.Contains(new Vector3Int(x, y, 0)))
                {
                    gu.levelTileMap.SetTile(new Vector3Int(x, y, 0), null);
                }
            }
        }
    }
}
