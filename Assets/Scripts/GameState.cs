using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameState : MonoBehaviour
{
    // player junk
    public Vector3Int playerPosition;
    public GameObject playerSprite;

    // Grid Stuff
    public GridUtils gu;

    public enum State
    {
        PLAYER_DECIDE_MOVE,
        PLAYER_MOVE,
        PLAYER_DECIDE_ACTION,
        PLAYER_ACTION,
        AI_MOVE
    }
    State state;

    void Start()
    {
        playerPosition = new Vector3Int(1, 1, 0);
        putGOAtPosition(playerSprite, playerPosition);
        showReachableTilesForPlayer();   
    }

    void putGOAtPosition(GameObject go, Vector3Int pos)
    {
        // 0.5 depends on sprite position and should not be trusted
        go.transform.position = gu.levelTileMap.CellToWorld(pos) + new Vector3(0, 0.5f, 1);
    }

    // get the tile currently under the mouse
    private Vector3Int tileAtMousePosition()
    {
        var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var tilePos = gu.levelTileMap.WorldToCell(worldPos);
        return tilePos;
    }

    // Update is called once per frame
    void Update()
    {
        switch(state)
        {
            case State.PLAYER_DECIDE_MOVE:
                // just wait for a destination tile to be clicked
                if (Input.GetMouseButtonDown(0))
                {
                    var targ = tileAtMousePosition();
                    if(reachableTilesFrom(playerPosition,3).Contains(targ))
                    {
                        playerPosition = targ;
                        putGOAtPosition(playerSprite, playerPosition);
                        gu.clearSelectedTiles();
                        state = State.PLAYER_MOVE;
                    }
                    
                }
                break;
            case State.PLAYER_MOVE:
                // animate moving to the tile

                // skipped for now.
                state = State.PLAYER_DECIDE_ACTION;

                break;
            case State.PLAYER_DECIDE_ACTION:
                // wait for a valid action

                // skipped for now.
                state = State.PLAYER_ACTION;
                break;
            case State.PLAYER_ACTION:
                // animation action

                // skipped for now.
                state = State.AI_MOVE;
                break;
            case State.AI_MOVE:
                // animate all NPCs

                // skipped for now.
                state = State.PLAYER_DECIDE_MOVE;
                showReachableTilesForPlayer();
                break;
        }


    }

    // convience method, get all reachable tiles from a start + depth.
    // does not return origin
    List<Vector3Int> reachableTilesFrom(Vector3Int start, int depth)
    {
        // returns only lists of moves so we need to trim
        var reachable = gu.BFS(playerPosition, 3);
        var destsOnly = new List<Vector3Int>();
        foreach (var v in reachable)
        {
            if (v.Count > 0)
            {
                destsOnly.Add(v[v.Count - 1]);
            }
        }
        return destsOnly;
    }

    // draw all reachable tiles
    void showReachableTilesForPlayer()
    {
        gu.showTilesAsSelected(reachableTilesFrom(playerPosition,3));
    }
}
