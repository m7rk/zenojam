using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    public GridUtils gu;
    public GameState gs;
    public GameObject ladder;

    public Tile[] tiles;
    public GameObject[] npcPrefabs;

    public GameObject boss;

    // tweak map gen with these
    public readonly int DIMS = 9;
    public readonly float FILL_THRESH = 0.5f;
    public readonly float PERLIN_MULT = 0.25f;
    public readonly float FALLOFF_FACTOR = 0.1f;
    public readonly int MIN_VIABLE_TILES = 90;

    // 100 - 7 * 7 = 50 possible enemy spots
    // squad count does not exceed fifteen + MAX_

    public readonly int MAX_SQUAD_SIZE = 3;

    public readonly int ENEMY_QUOTA = 7;

    public readonly int PLAYER_SAFE_ZONE = 7;

    public readonly int ITEMS_TO_SPAWN = 10;

    public GameObject groundItemPrefab;


    // firewall is never spawned
    public List<string> itemSpawnList;


    public void spawnNPC(GameObject prefab, Vector3Int pos)
    {
        GameObject k = Instantiate(prefab);
        k.name = prefab.name;
        k.transform.SetParent(this.transform);
        k.transform.position = gs.globalPositionForTile(pos);
        gs.NPCPositions[pos] = k.GetComponent<Unit>();
    }

    // Start is called before the first frame update
    void Awake()
    {
        // tutorial level..
        if (GameState.floorID == 10)
        {
            // place the ladder - this will be as far from the player as possible every time because of how BFS works
            gs.ladderPosition = new Vector3Int(17, 10, 0);
            ladder.transform.position = gs.globalPositionForTile(gs.ladderPosition) + new Vector3(0, 0, (gs.globalPositionForTile(gs.ladderPosition).y * 0.001f));

            // make npcs
            gs.NPCPositions = new Dictionary<Vector3Int, Unit>();

            // ground items
            gs.groundItems = new Dictionary<Vector3, GroundItem>();
            gs.putItem("Seed", GameState.PRACTICE_SEED_POSITION);

            // practice kenku
            spawnNPC(npcPrefabs[4], GameState.PRACTICE_KENKU_POSITION);

            return;
        }
            
        generateMapCandidate();

        // always place player at middle... removes all islands.
        var reachableToPlayer = gu.reachableTilesFrom(Vector3Int.zero, 1000, new HashSet<Vector3Int>());

        while(gu.levelTileMap.GetTile(Vector3Int.zero) == null || reachableToPlayer.Count < MIN_VIABLE_TILES)
        {
            generateMapCandidate();
            reachableToPlayer = gu.reachableTilesFrom(Vector3Int.zero, 1000, new HashSet<Vector3Int>());
        }

        // this map works.
        Debug.Log("This map has " + reachableToPlayer.Count + " tiles. ");

        // place the ladder - this will be as far from the player as possible every time because of how BFS works
        gs.ladderPosition = reachableToPlayer[reachableToPlayer.Count - 1];
        ladder.transform.position = gs.globalPositionForTile(reachableToPlayer[reachableToPlayer.Count - 1]) + new Vector3(0, 0, (gs.globalPositionForTile(gs.ladderPosition).y * 0.001f)); ;

        // remove all the islands the perlin made
        reachableToPlayer.Add(Vector3Int.zero);
        cullNonReachable(reachableToPlayer);

        // make npcs
        gs.NPCPositions = new Dictionary<Vector3Int, Unit>();

        // No NPCs on final floor.
        if (GameState.floorID == 1)
        {
            spawnNPC(boss, gs.ladderPosition);
            gs.groundItems = new Dictionary<Vector3, GroundItem>();
            gs.ladderPosition = new Vector3Int(1000, 1000, 0);
            ladder.transform.position = gs.globalPositionForTile(gs.ladderPosition);
            return;
        }

        spawnNPCs();
        spawnItems();

    }

    public void spawnNPCs()
    {
        while(gs.NPCPositions.Count < ENEMY_QUOTA)
        {
            var squad_center = new Vector3Int(Random.Range(-DIMS, DIMS), Random.Range(-DIMS, DIMS), 0);
            // squads should be far from player
            if(Mathf.Abs(squad_center.x) < PLAYER_SAFE_ZONE && Mathf.Abs(squad_center.y) < PLAYER_SAFE_ZONE)
            {
                continue;
            }
            // this is a valid place to put a squad.
            if (gu.levelTileMap.HasTile(squad_center))
            {
                for (int j = 0; j != MAX_SQUAD_SIZE; ++j)
                {
                    // valid floor tile!
                    var targTile = squad_center + new Vector3Int(Random.Range(-1, 2), Random.Range(-1, 2), 0);

                    if (gu.levelTileMap.HasTile(targTile) && !gs.NPCPositions.ContainsKey(targTile))
                    {
                        spawnNPC(randomNPCForFloor(), targTile);
                    }
                }
            }
        }
    }

    public void spawnItems()
    {
        gs.groundItems = new Dictionary<Vector3, GroundItem>();
        for(int i = 0; i != ITEMS_TO_SPAWN; ++i)
        {
            var testTile = new Vector3Int(Random.Range(-DIMS, DIMS), Random.Range(-DIMS, DIMS), 0);
            if (gu.levelTileMap.HasTile(testTile) && !gs.groundItems.ContainsKey(testTile))
            {
                var spawnCand = randomItemForFloor();
                if (GameState.playerItems != null)
                {
                    foreach (var v in GameState.playerItems)
                    {
                        if (v.name == spawnCand && !v.distractable)
                        {
                            // we already have this item, reroll
                            spawnCand = randomItemForFloor();
                        }
                    }
                }
                gs.putItem(spawnCand, testTile);
            }
        }
    }

    public GameObject randomNPCForFloor()
    {
        // half the time just spawn native to that floor
        if (Random.Range(0f, 1f) > 0.5f)
        {
            return npcPrefabs[GameState.floorID-2];
        } 
        else
        {
            return npcPrefabs[Random.Range(GameState.floorID - 2,7+1)];
        }
    }

    // later, reroll if player has item already
    public string randomItemForFloor()
    {
        // if 9 : 2/9 of list spawnable.
        // if 2 : 9/9 of list spawnable.
        float bestPossble = Random.Range(0f, (11f - GameState.floorID) / 9f);
        float targ = 1f - bestPossble;
        Debug.Log(bestPossble);
        int toSpawn = (int)(targ * itemSpawnList.Count);
        string ret = itemSpawnList[toSpawn];
        itemSpawnList.RemoveAt(toSpawn);
        return itemSpawnList[toSpawn];
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

                    // clamp
                    if (targTile >= tiles.Length)
                    {
                        targTile = tiles.Length - 1;
                    }

                    if(targTile < 0)
                    {
                        targTile = 0;
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
