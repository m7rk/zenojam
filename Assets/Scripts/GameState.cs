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
    public Unit playerUnit;
    public static List<GameItem> playerItems;
    public List<GameItem> startingItems;
    public ItemListManager ilm;

    private readonly float MOVE_ANIM_SPEED = 4f;
    private readonly float ACTION_SPEED = 0.4f;

    // Grid Stuff
    public GridUtils gu;
    public Vector3Int ladderPosition;

    // Floor stuff
    public TMPro.TMP_Text floorText;
    static string[] numbers  = new string[] { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
    public static int floorID = 9;

    // State stuff
    private Unit currentUnitToMoveOrAction;
    private List<Vector3Int> pendingUnitPath;
    private List<Vector3Int> AITurnOrder;
    public float actionTimer;

    private Vector3Int currentUnitTarget;

    // NPC Stuff, pop'd by levelgnerator
    public Dictionary<Vector3Int, Unit> NPCPositions;

    // Item Stuff, pop'd by level generator
    public Dictionary<Vector3, GroundItem> groundItems;
    // AI Stuff
    private int AI_AGGRO_RANGE = 7;

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
        playerItems = startingItems;
        playerPosition = new Vector3Int(0, 0, 0);
        playerUnit.transform.position = globalPositionForTile(playerPosition);
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
            if (gu.reachableTilesFrom(playerPosition, playerUnit.speed, NPCOccupiedTiles()).Contains(targ))
            {
                // order a move, player to position.
                currentUnitToMoveOrAction = playerUnit;
                pendingUnitPath = gu.findPathTo(playerPosition, playerUnit.speed, targ, NPCOccupiedTiles());

                // update player position and clear old tiles
                playerPosition = targ;
                gu.clearSelectedTiles();

                // next state
                state = State.PLAYER_MOVE;
                return;
            }

            if(targ == playerPosition)
            {
                // skip move step
                state = State.PLAYER_DECIDE_ACTION;
                showActionableTiles();
                return;
            }

        }
    }

    void playerDecideAction()
    {
        if(actionableTiles().Count == 0)
        {
            // no action, end turn
            state = State.AI_MOVE;
            generateAITurnOrder();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            var targ = tileAtMousePosition();
            if (actionableTiles().Contains(targ))
            {
                // player to make move.
                currentUnitToMoveOrAction = playerUnit;
                currentUnitTarget = targ;
                gu.clearSelectedTiles();
                // next state
                state = State.PLAYER_ACTION;
            }
        }
    }

    // move unit with current unit to move and pending path.
    // returns true when move is complete.
    public bool moveUnit()
    {
        // go to square
        currentUnitToMoveOrAction.transform.position = Vector3.MoveTowards(currentUnitToMoveOrAction.transform.position, globalPositionForTile(pendingUnitPath[0]), MOVE_ANIM_SPEED * Time.deltaTime);

        // if sqaure reached go to next. if no next go to next state.
        if (Vector3.Distance(currentUnitToMoveOrAction.transform.position, globalPositionForTile(pendingUnitPath[0])) < 0.01f)
        {
            currentUnitToMoveOrAction.transform.position = globalPositionForTile(pendingUnitPath[0]);
            pendingUnitPath.RemoveAt(0);
            if (pendingUnitPath.Count == 0)
            {
                currentUnitToMoveOrAction = null;
                pendingUnitPath = null;
                return true;
            }
        }
        else
        {
            // set facing
            currentUnitToMoveOrAction.GetComponent<Unit>().faceFront = globalPositionForTile(pendingUnitPath[0]).y - currentUnitToMoveOrAction.transform.position.y < 0;
            currentUnitToMoveOrAction.GetComponent<Unit>().faceRight = globalPositionForTile(pendingUnitPath[0]).x - currentUnitToMoveOrAction.transform.position.x > 0;


            // the AIS are backwards so this is a filthy hack to fix thsat
            if(state == State.AI_MOVE)
            {
                currentUnitToMoveOrAction.GetComponent<Unit>().faceRight = !currentUnitToMoveOrAction.GetComponent<Unit>().faceRight;
            }
        }
        return false;

    }

    public bool executeAction()
    {
        currentUnitToMoveOrAction.GetComponent<Unit>().faceFront = globalPositionForTile(currentUnitTarget).y - currentUnitToMoveOrAction.transform.position.y < 0;
        currentUnitToMoveOrAction.GetComponent<Unit>().faceRight = globalPositionForTile(currentUnitTarget).x - currentUnitToMoveOrAction.transform.position.x > 0;

        actionTimer += Time.deltaTime;
        int frame = (int)(((actionTimer / ACTION_SPEED) * currentUnitToMoveOrAction.attackFront.Length));
        if (actionTimer >= ACTION_SPEED)
        {
            actionTimer = 0;
            execAttack();
            currentUnitToMoveOrAction = null;
            return true;
        }
        // run animation
        currentUnitToMoveOrAction.GetComponentInChildren<SpriteRenderer>().sprite = (currentUnitToMoveOrAction.GetComponent<Unit>().faceFront) ? currentUnitToMoveOrAction.attackFront[frame] : currentUnitToMoveOrAction.attackBack[frame];


        return false;

    }

    void execAttack()
    {
        var attackDmg = Random.Range(currentUnitToMoveOrAction.item.damageLow, currentUnitToMoveOrAction.item.damageHi);

        if (currentUnitTarget == playerPosition)
        {
            if (playerUnit.hurt(1))
            {
                SceneManager.LoadScene("Title");
            }
        }
        else
        {
            // trigger the attack animation
            if (NPCPositions[currentUnitTarget].hurt(attackDmg))
            {
                // otherwise carry on
                Destroy(NPCPositions[currentUnitTarget].gameObject);
                NPCPositions.Remove(currentUnitTarget);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        ilm.generate(playerItems);
        switch(state)
        {
            case State.PLAYER_DECIDE_MOVE:
                playerDecideMove();
                break;

            case State.PLAYER_MOVE:
                if(moveUnit())
                {
                    checkLeaveFloor();
                    state = State.PLAYER_DECIDE_ACTION;
                    showActionableTiles();
                }
                break;

            case State.PLAYER_DECIDE_ACTION:
                // wait for a valid action
                // break early if they should escape
                playerDecideAction();

                break;
            case State.PLAYER_ACTION:
                // animation + action
                if (executeAction())
                {
                    generateAITurnOrder();
                    state = State.AI_MOVE;
                }
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
        var reachable = gu.reachableTilesFrom(playerPosition, playerUnit.speed, NPCOccupiedTiles());
        reachable.Add(playerPosition);
        gu.showTilesAsSelected(reachable);
    }

    public List<Vector3Int> actionableTiles()
    {
        var inRange = gu.reachableTilesFrom(playerPosition, playerUnit.item.range, new HashSet<Vector3Int>());
        List<Vector3Int> hasNPC = new List<Vector3Int>();
        foreach (var v in inRange)
        {
            if (NPCPositions.ContainsKey(v))
            {
                hasNPC.Add(v);
            }
        }
        return hasNPC;
    }
    void showActionableTiles()
    {
        gu.showTilesAsSelected(actionableTiles());
    }

    void checkLeaveFloor()
    {
        if (playerPosition == ladderPosition)
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
        if(currentUnitToMoveOrAction == null)
        {
            // any units left to move?
            if(AITurnOrder.Count == 0)
            {
                // done!
                return true;
            }

            // pull the coordinate of the AI player to move.
            var curTurn = AITurnOrder[0];
            AITurnOrder.RemoveAt(0);
            currentUnitToMoveOrAction = NPCPositions[curTurn];

            // check if there exists a possible path to the player.
            var reachable = gu.reachableTilesFrom(curTurn, 1000, NPCOccupiedTiles());

            if(reachable.Contains(playerPosition))
            {
                pendingUnitPath = gu.findPathTo(curTurn, 1000, playerPosition, NPCOccupiedTiles());

                // cut off last move since that would collide with player
                pendingUnitPath.RemoveAt(pendingUnitPath.Count - 1);

                // aggro check
                if(pendingUnitPath.Count < AI_AGGRO_RANGE)
                {
                    currentUnitToMoveOrAction.aggro = true;
                }

                // only go as far as we can if length too big
                while (pendingUnitPath.Count > currentUnitToMoveOrAction.speed)
                {
                    pendingUnitPath.RemoveAt(pendingUnitPath.Count - 1);
                }

                if (currentUnitToMoveOrAction.aggro)
                {
                    // we were next to the player, so we don't need to update position after all.
                    if (pendingUnitPath.Count == 0)
                    {
                        currentUnitToMoveOrAction = null;
                        pendingUnitPath = null;
                        return false;
                    }
                    // update AI positon and go ahead with movement
                    else
                    {
                        NPCPositions.Remove(curTurn);
                        NPCPositions[pendingUnitPath[pendingUnitPath.Count - 1]] = currentUnitToMoveOrAction;
                    }
                }
                else
                {
                    // player is too far away, do nothing
                    currentUnitToMoveOrAction = null;
                    pendingUnitPath = null;
                    return false;
                }
            }
            else
            {
                // can't reach player, just do nothing.
                pendingUnitPath = null;
                currentUnitToMoveOrAction = null;
                return false;
            }


        }

        moveUnit();
        return false;

    }


}
