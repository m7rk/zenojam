using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameState : MonoBehaviour
{
    // player junk
    public Vector3Int playerPosition;
    public GameObject playerSprite;

    private readonly float MOVE_ANIM_SPEED = 3f;

    // Grid Stuff
    public GridUtils gu;

    // Floor stuff
    public TMPro.TMP_Text floorText;
    static string[] numbers  = new string[] { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
    public static int floorID = 9;

    // State stuff
    List<Vector3Int> pendingPlayerPath;

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
        playerSprite.transform.position = globalPositionForTile(playerPosition);
        showReachableTilesForPlayer();
        floorText.text = "FLOOR " + numbers[floorID].ToUpper();
    }

    Vector3 globalPositionForTile(Vector3Int pos)
    {
        // 0.5 depends on sprite position and should not be trusted
        return gu.levelTileMap.CellToWorld(pos) + new Vector3(0, 0.5f, 1);
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
                        pendingPlayerPath = findPathTo(playerPosition, 3, targ);
                        gu.clearSelectedTiles();
                        state = State.PLAYER_MOVE;
                    }
                    
                }
                break;

            case State.PLAYER_MOVE:

                // go to square
                playerSprite.transform.position = Vector3.MoveTowards(playerSprite.transform.position, globalPositionForTile(pendingPlayerPath[0]), MOVE_ANIM_SPEED * Time.deltaTime);

                // if sqaure reached go to next. if no next go to next state.
                if (Vector3.Distance(playerSprite.transform.position, globalPositionForTile(pendingPlayerPath[0])) < 0.01f)
                {
                    playerSprite.transform.position = globalPositionForTile(pendingPlayerPath[0]);
                    playerPosition = pendingPlayerPath[0];
                    pendingPlayerPath.RemoveAt(0);
                    if(pendingPlayerPath.Count == 0)
                    {
                        playerSprite.GetComponent<PlayerSprite>().faceFront = true;
                        playerSprite.GetComponent<PlayerSprite>().faceRight = true;
                        state = State.PLAYER_DECIDE_ACTION;
                    }
                }
                else
                {
                    // set facing
                    playerSprite.GetComponent<PlayerSprite>().faceFront = globalPositionForTile(pendingPlayerPath[0]).y - playerSprite.transform.position.y < 0;
                    playerSprite.GetComponent<PlayerSprite>().faceRight = globalPositionForTile(pendingPlayerPath[0]).x - playerSprite.transform.position.x > 0;
                }

                break;
            case State.PLAYER_DECIDE_ACTION:
                // wait for a valid action

                // break early if they should escape
                checkLeaveFloor();

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

    List<Vector3Int> findPathTo(Vector3Int start, int depth, Vector3Int end)
    {
        var reachable = gu.BFS(playerPosition, 3);
        foreach (var v in reachable)
        {
            if (v.Count > 0)
            {
                if((v[v.Count - 1]) == end)
                {
                    return v;
                }
            }
        }
        Debug.Log("path error in findPathTo");
        return null;
    }

    // draw all reachable tiles
    void showReachableTilesForPlayer()
    {
        gu.showTilesAsSelected(reachableTilesFrom(playerPosition,3));
    }

    void checkLeaveFloor()
    {
        if (gu.levelTileMap.GetTile(playerPosition).name == "FloorEscape")
        {
            floorID -= 1;
            if (floorID == 0)
            {
                SceneManager.LoadScene("Title");

            }
            else
            {
                SceneManager.LoadScene("Dungeon");
            }
        }
    }
}
