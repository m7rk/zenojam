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

    // NPC Stuff, pop'd by levelgnerator
    public Dictionary<Vector3Int, GameObject> NPCPositions;


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


    public Vector3 globalPositionForTile(Vector3Int pos)
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

    HashSet<Vector3Int> NPCOccupiedTiles()
    {
        var occd = new HashSet<Vector3Int>();
        foreach(var v in NPCPositions)
        {
            occd.Add(v.Key);
        }
        return occd;
    }

    void playerDecideMove()
    {
        // just wait for a destination tile to be clicked
        if (Input.GetMouseButtonDown(0))
        {
            var targ = tileAtMousePosition();
            if (gu.reachableTilesFrom(playerPosition, 3, NPCOccupiedTiles()).Contains(targ))
            {
                pendingPlayerPath = gu.findPathTo(playerPosition, 3, targ, NPCOccupiedTiles());
                gu.clearSelectedTiles();
                state = State.PLAYER_MOVE;
            }

        }
    }

    void playerMove()
    {
        // go to square
        playerSprite.transform.position = Vector3.MoveTowards(playerSprite.transform.position, globalPositionForTile(pendingPlayerPath[0]), MOVE_ANIM_SPEED * Time.deltaTime);

        // if sqaure reached go to next. if no next go to next state.
        if (Vector3.Distance(playerSprite.transform.position, globalPositionForTile(pendingPlayerPath[0])) < 0.01f)
        {
            playerSprite.transform.position = globalPositionForTile(pendingPlayerPath[0]);
            playerPosition = pendingPlayerPath[0];
            pendingPlayerPath.RemoveAt(0);
            if (pendingPlayerPath.Count == 0)
            {
                playerSprite.GetComponent<Unit>().faceFront = true;
                playerSprite.GetComponent<Unit>().faceRight = true;
                state = State.PLAYER_DECIDE_ACTION;
            }
        }
        else
        {
            // set facing
            playerSprite.GetComponent<Unit>().faceFront = globalPositionForTile(pendingPlayerPath[0]).y - playerSprite.transform.position.y < 0;
            playerSprite.GetComponent<Unit>().faceRight = globalPositionForTile(pendingPlayerPath[0]).x - playerSprite.transform.position.x > 0;
        }

    }

    // Update is called once per frame
    void Update()
    {
        switch(state)
        {
            case State.PLAYER_DECIDE_MOVE:
                playerDecideMove();
                break;

            case State.PLAYER_MOVE:
                playerMove();
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


    // draw all reachable tiles
    void showReachableTilesForPlayer()
    {
        gu.showTilesAsSelected(gu.reachableTilesFrom(playerPosition,3, NPCOccupiedTiles()));
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
