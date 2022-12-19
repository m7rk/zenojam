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
    GameObject currentUnitToMove;
    List<Vector3Int> pendingUnitPath;
    List<Vector3Int> AITurnOrder;

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
        playerPosition = new Vector3Int(0, 0, 0);
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
                // order a move, player to position.
                currentUnitToMove = playerSprite;
                pendingUnitPath = gu.findPathTo(playerPosition, 3, targ, NPCOccupiedTiles());

                // update player position and clear old tiles
                playerPosition = targ;
                gu.clearSelectedTiles();

                // next state
                state = State.PLAYER_MOVE;
            }

        }
    }

    // move unit with current unit to move and pending path.
    // returns true when move is complete.
    public bool moveUnit()
    {
        // go to square
        currentUnitToMove.transform.position = Vector3.MoveTowards(currentUnitToMove.transform.position, globalPositionForTile(pendingUnitPath[0]), MOVE_ANIM_SPEED * Time.deltaTime);

        // if sqaure reached go to next. if no next go to next state.
        if (Vector3.Distance(currentUnitToMove.transform.position, globalPositionForTile(pendingUnitPath[0])) < 0.01f)
        {
            currentUnitToMove.transform.position = globalPositionForTile(pendingUnitPath[0]);
            pendingUnitPath.RemoveAt(0);
            if (pendingUnitPath.Count == 0)
            {
                currentUnitToMove = null;
                pendingUnitPath = null;
                return true;
            }
        }
        else
        {
            // set facing
            currentUnitToMove.GetComponent<Unit>().faceFront = globalPositionForTile(pendingUnitPath[0]).y - currentUnitToMove.transform.position.y < 0;
            currentUnitToMove.GetComponent<Unit>().faceRight = globalPositionForTile(pendingUnitPath[0]).x - currentUnitToMove.transform.position.x > 0;


            // the AIS are backwards so this is a filthy hack to fix thsat
            if(state == State.AI_MOVE)
            {
                currentUnitToMove.GetComponent<Unit>().faceRight = !currentUnitToMove.GetComponent<Unit>().faceRight;
            }
        }
        return false;

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
                if(moveUnit())
                {
                    state = State.PLAYER_DECIDE_ACTION;
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
                generateAITurnOrder();
                // skipped for now.
                state = State.AI_MOVE;
                break;
            case State.AI_MOVE:
                // animate all NPCs
                if(aiMove())
                {
                    state = State.PLAYER_DECIDE_MOVE;
                    showReachableTilesForPlayer();
                }
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

    void generateAITurnOrder()
    {
        AITurnOrder = new List<Vector3Int>();

        foreach(var v in NPCPositions.Keys)
        {
            AITurnOrder.Add(v);
        }

        AITurnOrder.Sort((a, b) => (Mathf.Abs(a.x - playerPosition.x) + Mathf.Abs(a.y - playerPosition.y)) - (Mathf.Abs(b.x - playerPosition.x) + Mathf.Abs(b.y - playerPosition.y)));
    }

    bool aiMove()
    {
        // check if there is a unit moving
        if(currentUnitToMove == null)
        {
            // any units left to move?
            if(AITurnOrder.Count == 0)
            {
                // done!
                return true;
            }

            var curTurn = AITurnOrder[0];
            AITurnOrder.RemoveAt(0);
            currentUnitToMove = NPCPositions[curTurn];
            pendingUnitPath = gu.findPathTo(curTurn, 1000, playerPosition, NPCOccupiedTiles());
            // cut off last move since that would collide with player
            pendingUnitPath.RemoveAt(pendingUnitPath.Count - 1);

            // update AI positon
            NPCPositions.Remove(curTurn);
            NPCPositions[pendingUnitPath[pendingUnitPath.Count - 1]] = currentUnitToMove;
            // all set! now just need to run the mover.
        }

        moveUnit();
        return false;

    }


}
