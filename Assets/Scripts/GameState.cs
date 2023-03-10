using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameState : MonoBehaviour
{

    // player junk
    public Vector3Int playerPosition;
    public Unit playerUnit;
    public DungeonUI dungeonUI;

    // preserve between floors
    public static List<GameItem> playerItems;
    public static int playerItemIndex = 0;
    public static int PLAYER_MAXSPEED = 3;
    public static bool pacifist = true;
    public static bool knowsAboutAutoSkip = false;

    public static float timeJumpGame;
    public static float timeJumpBoss;

    public static int floorID = 9;
    public static int PLAYER_MAXHEALTH = 6;
    public static int healthLastFloor = PLAYER_MAXHEALTH;

    public List<GameItem> startingItems;
    public ItemListManager ilm;
    public HPBar hpBarT;

    private readonly float MOVE_ANIM_SPEED = 4f;
    public static readonly float ACTION_SPEED = 0.4f;

    // Grid Stuff
    public GridUtils gu;
    public Vector3Int ladderPosition;

    // Floor stuff
    public TMPro.TMP_Text floorText;
    static string[] numbers  = new string[] { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };


    // State stuff
    private Unit currentUnitToMoveOrAction;
    private List<Vector3Int> pendingUnitPath;
    private List<Vector3Int> AITurnOrder;
    public float actionTimer;
    private bool levelEndFlag = false;

    private Vector3Int AI_MEMO_MOVING; 

    private Vector3Int currentUnitTarget;

    // NPC Stuff, pop'd by levelgnerator
    public Dictionary<Vector3Int, Unit> NPCPositions;

    // Item Stuff, pop'd by level generator
    public Dictionary<Vector3, GroundItem> groundItems;
    // AI Stuff
    private int AI_AGGRO_RANGE = 7;

    public static readonly Vector3Int PRACTICE_SEED_POSITION = new Vector3Int(4, 4, 0);
    public static readonly Vector3Int PRACTICE_KENKU_POSITION = new Vector3Int(8, 8, 0);

    public GameObject projectileBase;

    public GameObject contextCursor;
    public Canvas canvas;

    public GameObject groundItemPrefab;

    public MagicAnimsets ma;
    public enum State
    {
        PLAYER_DECIDE_MOVE,
        PLAYER_MOVE,
        PLAYER_DECIDE_ACTION,
        PLAYER_ACTION,
        AI_MOVE
    }
    public State state;

    public AudioPlayer ap;

    public AudioSource gameMus;
    public AudioSource bossMus;


    public AudioSource pickUpSound;
    public AudioSource eatSound;

    public GameObject autoSkipHint;

    void Start()
    {
        if(GameState.floorID == 10)
        {
            PARAMS_RESET();
        }
        Debug.Log(GameState.pacifist + " pacifist? ");
        gameMus.time = GameState.timeJumpGame;
        bossMus.time = GameState.timeJumpBoss;
        playerUnit.health = GameState.healthLastFloor;
        if (playerItems == null)
        {
            playerItems = startingItems;
        }
        playerPosition = new Vector3Int(0, 0, 0);
        playerUnit.transform.position = globalPositionForTile(playerPosition);
        showReachableTilesForPlayer();
        if (floorID == 10)
        {
            floorText.text = "TUTORIAL";
        }
        else
        {
            floorText.text = "FLOOR " + numbers[floorID].ToUpper();
        }

        if (floorID <= 3)
        {
            Destroy(gameMus.gameObject);
            gameMus = null;
            floorText.color = Color.red;
        }
        else
        {
            Destroy(bossMus.gameObject);
            bossMus = null;
            floorText.color = Color.white;
        }
        ilm.generate();
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
                return;
            }

        }
    }

    public void putItem(string name, Vector3Int tposition)
    {
        var v = Instantiate(groundItemPrefab);
        v.transform.SetParent(this.transform);
        v.transform.position = globalPositionForTile(tposition) + new Vector3(0, -0.22f, 1f);
        v.GetComponent<GroundItem>().setItemType(name);
        groundItems[tposition] = v.GetComponent<GroundItem>();
    }

    void playerDecideAction()
    {
        showActionableTiles();

        if(Input.GetKey(KeyCode.Space))
        {
            knowsAboutAutoSkip = true;
            state = State.AI_MOVE;
            gu.clearSelectedTiles();
            generateAITurnOrder();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            var targ = tileAtMousePosition();
            if (actionableTiles().Contains(targ))
            {
                if(targ.Equals(playerPosition))
                {
                    // eat?
                    if(playerItems[playerItemIndex].edible)
                    {
                        eatSound.Play();
                        playerUnit.health += 1;
                        playerUnit.health = Mathf.Min(playerUnit.health, PLAYER_MAXHEALTH);
                        playerItems.RemoveAt(playerItemIndex);
                        playerItemIndex--;
                        ilm.generate();
                    }    

                    state = State.AI_MOVE;
                    gu.clearSelectedTiles();
                    generateAITurnOrder();
                    return;
                }
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
            if (state == State.PLAYER_MOVE)
            {
                ap.playByName("STEP");
            }
            currentUnitToMoveOrAction.transform.position = globalPositionForTile(pendingUnitPath[0]);
            pendingUnitPath.RemoveAt(0);
            if (pendingUnitPath.Count == 0)
            {
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

    public bool attackingPlayerIsUsingRanged()
    {
        if (currentUnitToMoveOrAction.GetComponent<Unit>().thisIsPlayer)
        {
            return playerItems[playerItemIndex].range > 1;
        }
        else
        {
            return currentUnitToMoveOrAction.AI_range > 1;
        }
    }

    public bool executeAction()
    {
        if(actionTimer == 0)
        {
            // check if we need to launch a projectile
            if (attackingPlayerIsUsingRanged())
            {
                var v = Instantiate(projectileBase);
                // if we're the player load the sprite, otherwise, we're an AI.
                v.GetComponent<RangedDecal>().anims = new Sprite[] { (state == State.PLAYER_ACTION) ? playerItems[playerItemIndex].image : currentUnitToMoveOrAction.AIRangedProjectile };

                if(state != State.PLAYER_ACTION)
                {
                    currentUnitToMoveOrAction.GetComponent<Unit>().playAIAttackSound();
                    if(GameState.floorID == 1)
                    {
                        v.GetComponent<RangedDecal>().anims = ma.animsetForSpell("Combust");
                    }
                }

                // fireballs get special animation.
                if ((state == State.PLAYER_ACTION) && !playerItems[playerItemIndex].distractable)
                {
                    // it's a fireball
                    v.GetComponent<RangedDecal>().anims = ma.animsetForSpell(playerItems[playerItemIndex].name);
                    ap.playByName("PLAYER_RANGED");
                }

                v.transform.SetParent(this.transform);
                v.transform.position = currentUnitToMoveOrAction.transform.position;
                // offset for items
                v.GetComponent<RangedDecal>().setGoal(globalPositionForTile(currentUnitTarget) + new Vector3(0, -0.22f, 1f));
            } else
            {
                if ((state == State.PLAYER_ACTION))
                {
                    ap.playByName("PLAYER_MELEE");
                }
                else
                {
                    currentUnitToMoveOrAction.GetComponent<Unit>().playAIAttackSound();
                }
            }
        }

        var attackAnimAry = (currentUnitToMoveOrAction.GetComponent<Unit>().faceFront) ? currentUnitToMoveOrAction.attackFront : currentUnitToMoveOrAction.attackBack;

        if(state == State.PLAYER_ACTION && playerItems[playerItemIndex].range == 1 && playerItems[playerItemIndex].name != "Fisticuffs")
        {
            attackAnimAry = (currentUnitToMoveOrAction.GetComponent<Unit>().faceFront) ? currentUnitToMoveOrAction.altAttackFront : currentUnitToMoveOrAction.altAttackBack;
        }

        actionTimer += Time.deltaTime;
        int frame = (int)(((actionTimer / ACTION_SPEED) * attackAnimAry.Length));

        if (actionTimer >= ACTION_SPEED)
        {
            actionTimer = 0;
            finishAction();
            return true;
        }

       
        currentUnitToMoveOrAction.GetComponent<Unit>().faceFront = globalPositionForTile(currentUnitTarget).y - currentUnitToMoveOrAction.transform.position.y < 0;
        currentUnitToMoveOrAction.GetComponent<Unit>().faceRight = globalPositionForTile(currentUnitTarget).x - currentUnitToMoveOrAction.transform.position.x > 0;

        // the AIS are backwards so this is a filthy hack to fix thsat
        if (state == State.AI_MOVE)
        {
            currentUnitToMoveOrAction.GetComponent<Unit>().faceRight = !currentUnitToMoveOrAction.GetComponent<Unit>().faceRight;
            // the boss is forward rekt
            if(currentUnitToMoveOrAction.name == "Adventurer" && currentUnitToMoveOrAction.GetComponent<Unit>().faceFront)
            {
                currentUnitToMoveOrAction.GetComponent<Unit>().faceRight = !currentUnitToMoveOrAction.GetComponent<Unit>().faceRight;
            }
        } else
        {
            // this is a weapon
            if (!playerItems[playerItemIndex].distractable)
            {
                if (playerItems[playerItemIndex].range > 1)
                {
                    currentUnitToMoveOrAction.GetComponent<Unit>().showBook(playerItems[playerItemIndex].image);
                } 
                else
                {
                    if (playerItems[playerItemIndex].name != "Fisticuffs")
                    {
                        currentUnitToMoveOrAction.GetComponent<Unit>().showWeapon(playerItems[playerItemIndex].name, frame);
                    }
                }
            }
        }

        // run animation
        currentUnitToMoveOrAction.mainSpriteRenderer.sprite = attackAnimAry[frame];

        return false;

    }

    void finishAction()
    {
        if (currentUnitTarget.Equals(playerPosition))
        {
            if (playerUnit.hurt(1))
            {

            }
        }
        else
        {
            if(!NPCPositions.ContainsKey(currentUnitTarget))
            {
                // item was tossed, just place it!
                putItem(playerItems[playerItemIndex].name,currentUnitTarget);
                playerItems.RemoveAt(playerItemIndex);
                playerItemIndex--;
                ilm.generate();
                return;
               
            }

            var attackDmg = UnityEngine.Random.Range(playerItems[playerItemIndex].damageLow, 1 + playerItems[playerItemIndex].damageHi);


            // special AOE for that one spell
            if (playerItems[playerItemIndex].name == "Inferno")
            {
                for (int x = -1; x != 2; ++x)
                {
                    for (int y = -1; y != 2; ++y)
                    {
                        if(NPCPositions.ContainsKey(currentUnitTarget + new Vector3Int(x,y,0)))
                        {
                            // trigger the attack animation
                            if (NPCPositions[currentUnitTarget + new Vector3Int(x, y, 0)].hurt(attackDmg))
                            {
                                GameState.pacifist = false;
                                NPCPositions.Remove(currentUnitTarget + new Vector3Int(x, y, 0));
                            }
                        }
                    }
                }
            }
            else
            {
                // trigger the attack animation
                if (NPCPositions[currentUnitTarget].hurt(attackDmg))
                {
                    GameState.pacifist = false;
                    NPCPositions.Remove(currentUnitTarget);
                }
            }
            // blaze gets consumed.
            if(playerItems[playerItemIndex].name == "Blaze")
            {
                playerItems.RemoveAt(playerItemIndex);
                playerItemIndex--;
                ilm.generate();
            }

            if (playerItems[playerItemIndex].distractable)
            {
                // threw away a distractable, destroy...
                playerItems.RemoveAt(playerItemIndex);
                playerItemIndex--;
                ilm.generate();
            }


            currentUnitToMoveOrAction.GetComponent<Unit>().hideWeapons();
        }
    }

    public void tryPickUpItem()
    {
        if(groundItems.ContainsKey(playerPosition))
        {
            pickUpSound.Play();
            // sus
            var it = Resources.Load<GameObject>("Items/" + groundItems[playerPosition].thisItem);
            playerItems.Add(it.GetComponent<GameItem>());
            Destroy(groundItems[playerPosition].gameObject);
            groundItems.Remove(playerPosition);
            ilm.generate();

        }
    }

    void playerDied()
    {
        FindObjectOfType<Transitioner>().endScene(toNextLevel);
    }

    // Update is called once per frame
    // use lateupdate because we want sprites to be overriden..
    void LateUpdate()
    {
        if(knowsAboutAutoSkip)
        {
            autoSkipHint.SetActive(false);
        }
        getCursorContext();
        hpBarT.setHP(playerUnit.health, PLAYER_MAXHEALTH);

        if (levelEndFlag)
        {
            return;
        }

        // end game no hp
        if (playerUnit.health == 0)
        {
            Invoke("playerDied", 1f);
            levelEndFlag = true;
            return;
        }

        // end game killed boss
        if (GameState.floorID == 1 && NPCOccupiedTiles().Count == 0)
        {
            FindObjectOfType<Transitioner>().endScene(toNextLevel);
            levelEndFlag = true;
        }


        // tutorial stuff
        if (GameState.floorID == 10)
        {
            if(playerItemIndex == 1)
            {
                dungeonUI.progressEquip();
            }

            if(playerPosition.x > PRACTICE_KENKU_POSITION.x && playerPosition.y > PRACTICE_KENKU_POSITION.y)
            {
                dungeonUI.progressUse();
            }
        }
        switch(state)
        {
            case State.PLAYER_DECIDE_MOVE:
                playerDecideMove();
                break;

            case State.PLAYER_MOVE:
                if(moveUnit())
                {
                    tryPickUpItem();
                    checkLeaveFloor();
                    state = State.PLAYER_DECIDE_ACTION;
                }
                break;

            case State.PLAYER_DECIDE_ACTION:
                if(GameState.floorID == 10)
                {
                    dungeonUI.progressMove();
                    if(playerPosition.Equals(GameState.PRACTICE_SEED_POSITION))
                    {
                        dungeonUI.progressPickup();
                    }
                }
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
                if (GameState.floorID == 10)
                {
                    dungeonUI.progressAction();
                }
                    
                // animate all NPCs
                if (aiMove())
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
        gu.showTilesAsSelected(reachable, false);
    }

    

    public List<Vector3Int> actionableTiles()
    {
        var tiles = gu.manhattan(playerPosition, playerItems[playerItemIndex].range);

        var actionables = new List<Vector3Int>();
        foreach (var v in tiles)
        {
            // this NPC attackable.
            if (playerItems[playerItemIndex].damageHi > 0)
            {
                if (NPCPositions.ContainsKey(v))
                {
                    actionables.Add(v);
                }
            }

            // this item can be thrown as a distraction
            if(playerItems[playerItemIndex].distractable && gu.levelTileMap.HasTile(v) && !NPCPositions.ContainsKey(v) && !groundItems.ContainsKey(v))
            {
                actionables.Add(v);
            }
        }
        // always add eat or pass
        actionables.Add(playerPosition);
        return actionables;
    }
    void showActionableTiles()
    {
        gu.showTilesAsSelected(actionableTiles(), true);
    }

    void checkLeaveFloor()
    {
        if (playerPosition == ladderPosition)
        {
            FindObjectOfType<Transitioner>().endScene(toNextLevel);
            levelEndFlag = true;
        }
    }


    public static void PARAMS_RESET()
    {
        GameState.playerItems = null;
        GameState.playerItemIndex = 0;
        GameState.pacifist = true;
        GameState.floorID = 9;
        GameState.healthLastFloor = PLAYER_MAXHEALTH;
        GameState.timeJumpGame = 0;
        GameState.timeJumpBoss = 0;
    }

    void toNextLevel()
    {
        floorID -= 1;

        //ded
        if(playerUnit.health == 0)
        {
            PARAMS_RESET();
            SceneManager.LoadScene("Dungeon");
        }
        // bad end
        else if (floorID == 0)
        {
            SceneManager.LoadScene("BadEnd");
        }
        // good end
        else if (floorID == 1 && GameState.pacifist)
        {
            SceneManager.LoadScene("GoodEnd");
        }
        else
        {
            GameState.healthLastFloor = Mathf.Min(PLAYER_MAXHEALTH, playerUnit.health + 2);
            if (gameMus != null)
            {
                GameState.timeJumpGame = gameMus.time;
            }
            if (bossMus != null)
            {
                GameState.timeJumpBoss = bossMus.time;
            }
            SceneManager.LoadScene("Dungeon");
        }
    }

    void generateAITurnOrder()
    {
        currentUnitToMoveOrAction = null;
        AITurnOrder = new List<Vector3Int>();

        foreach(var v in NPCPositions.Keys)
        {
            AITurnOrder.Add(v);
        }
        AITurnOrder.Sort((a, b) => (Mathf.Abs(a.x - playerPosition.x) + Mathf.Abs(a.y - playerPosition.y)) - (Mathf.Abs(b.x - playerPosition.x) + Mathf.Abs(b.y - playerPosition.y)));
    }


    // does two things
    // set the currentUnitToMove
    // set the currentUnitTarget (if possible)
    // returns TRUE if no more players to move
    // returns FALSE if we need to process move and action.
    public bool generateAIMovePlan()
    {
        // any units left to move?
        if (AITurnOrder.Count == 0)
        {
            // done!
            return true;
        }

        // pull the coordinate of the AI player to move.
        var curTurn = AITurnOrder[0];
        AITurnOrder.RemoveAt(0);
        currentUnitToMoveOrAction = NPCPositions[curTurn];
        AI_MEMO_MOVING = curTurn;


        // ====================================

        // check if there exists a possible path to a distraction.
        var occd = NPCOccupiedTiles();
        occd.Add(playerPosition);
        var ireachable = gu.reachableTilesFrom(curTurn, currentUnitToMoveOrAction.speed, occd);

        currentUnitToMoveOrAction.pacified = false;
        foreach (var v in ireachable)
        {
            if(groundItems.ContainsKey(v) && groundItems[v].GetComponent<GroundItem>().canDistract(currentUnitToMoveOrAction.name))
            {
                pendingUnitPath = gu.findPathTo(curTurn, 1000, v, NPCOccupiedTiles());

                // make seeds always work in tutorial level
                if (GameState.floorID == 10)
                {
                    var weakSeedToss = ((v == (new Vector3Int(1, 0, 0) + GameState.PRACTICE_KENKU_POSITION)) || (v == (new Vector3Int(0, 1, 0) + GameState.PRACTICE_KENKU_POSITION)));
                    // if, the seed was not thrown far enough, move somewhere safe
                    if (weakSeedToss && (v + new Vector3Int(1, 2, 0)) != playerPosition)
                    {
                        // forced move
                        pendingUnitPath = gu.findPathTo(curTurn, 1000, GameState.PRACTICE_KENKU_POSITION + new Vector3Int(1, 2, 0), NPCOccupiedTiles());
                    }
                }

                // last move cutoff
                pendingUnitPath.RemoveAt(pendingUnitPath.Count - 1);


                // we know we're in range so just do it and return

                // we were next to the item so we don't need to update position after all.
                if (pendingUnitPath.Count == 0)
                {
                    currentUnitToMoveOrAction.pacified = true;
                    pendingUnitPath = null;
                    return false;
                }
                // update AI positon and go ahead with movement
                else
                {
                    currentUnitToMoveOrAction.pacified = true;
                    NPCPositions.Remove(curTurn);
                    NPCPositions[pendingUnitPath[pendingUnitPath.Count - 1]] = currentUnitToMoveOrAction;
                    AI_MEMO_MOVING = pendingUnitPath[pendingUnitPath.Count - 1];
                    return false;
                }
            }
        }


        // ====================================

        // check if there exists a possible path to the player.
        var reachable = gu.reachableTilesFrom(curTurn, 1000, NPCOccupiedTiles());
        if (reachable.Contains(playerPosition))
        {
            pendingUnitPath = gu.findPathTo(curTurn, 1000, playerPosition, NPCOccupiedTiles());
            // cut off last move since that would collide with player
            pendingUnitPath.RemoveAt(pendingUnitPath.Count - 1);

            // aggro check - disable on tutorial floor.
            if (pendingUnitPath.Count < AI_AGGRO_RANGE && GameState.floorID != 10)
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
                    pendingUnitPath = null;
                    return false;
                }
                // update AI positon and go ahead with movement
                else
                {
                    NPCPositions.Remove(curTurn);
                    NPCPositions[pendingUnitPath[pendingUnitPath.Count - 1]] = currentUnitToMoveOrAction;
                    AI_MEMO_MOVING = pendingUnitPath[pendingUnitPath.Count - 1];
                    return false;
                }
            }
            else
            {
                // we aren't aggro yet
                pendingUnitPath = null;
                return false;
            }
        }
        else
        {
            // can't reach player, just do nothing.
            pendingUnitPath = null;
            return false;
        }
    }


    public void generateAIAttackPlan()
    {
        int dist = Math.Abs(AI_MEMO_MOVING.x - playerPosition.x) + Math.Abs(AI_MEMO_MOVING.y - playerPosition.y);

        if (currentUnitToMoveOrAction.pacified)
        {
            // look at item
            var ireachable = gu.reachableTilesFrom(AI_MEMO_MOVING, currentUnitToMoveOrAction.speed, NPCOccupiedTiles());
            foreach (var v in ireachable)
            {
                if (groundItems.ContainsKey(v) && groundItems[v].GetComponent<GroundItem>().canDistract(currentUnitToMoveOrAction.name))
                {
                    currentUnitToMoveOrAction.GetComponent<Unit>().faceFront = globalPositionForTile(v).y - currentUnitToMoveOrAction.transform.position.y < 0;
                    currentUnitToMoveOrAction.GetComponent<Unit>().faceRight = globalPositionForTile(v).x - currentUnitToMoveOrAction.transform.position.x > 0;
                    currentUnitToMoveOrAction.GetComponent<Unit>().faceRight = !currentUnitToMoveOrAction.GetComponent<Unit>().faceRight;
                }
            }
            // don't attack
            currentUnitToMoveOrAction = null;
            return;
        }

        // the unit is in range!
        if (dist <= currentUnitToMoveOrAction.AI_range && GameState.floorID != 10 && !currentUnitToMoveOrAction.pacified)
        {
            currentUnitTarget = playerPosition;
        }
        else
        {
            currentUnitToMoveOrAction = null;
        }

    }

    bool aiMove()
    {
        // check if there is a unit moving
        if(currentUnitToMoveOrAction == null)
        {
            // if this is true we're done.
            if (generateAIMovePlan())
            {
                return true;
            }
            // jump to attack logic if move was null
            if(pendingUnitPath == null)
            {
                generateAIAttackPlan();
                return false;
            }
        }

        // always do move first if we have to.
        if (pendingUnitPath != null)
        {
            // run unit move animation.
            // when this finishes, check for attack.
            if(moveUnit())
            {
                generateAIAttackPlan();
                return false;
            }
            
        }
        else
        {
            if(executeAction())
            {
                currentUnitToMoveOrAction = null;
            }
        }

        return false;
    }

    // later, show enemy name and stat
    public void getCursorContext()
    {
        Vector2 movePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition, canvas.worldCamera,
            out movePos);

        contextCursor.transform.position = canvas.transform.TransformPoint(movePos);

        contextCursor.GetComponentInChildren<TMP_Text>().text = "";

        var targ = tileAtMousePosition();

        if(state == State.PLAYER_DECIDE_MOVE)
        {
            if (targ == playerPosition)
            {
                contextCursor.GetComponentInChildren<TMP_Text>().text = "Stay Put";
            } 
            else if(gu.reachableTilesFrom(playerPosition, playerUnit.speed, NPCOccupiedTiles()).Contains(targ))
            {
                if (groundItems.ContainsKey(targ))
                {
                    contextCursor.GetComponentInChildren<TMP_Text>().text = "Pick Up " + groundItems[targ].thisItem;
                }
                else
                {
                    contextCursor.GetComponentInChildren<TMP_Text>().text = "Move Here";
                }
            }
        }

        if (state == State.PLAYER_DECIDE_ACTION)
        {
            if (targ == playerPosition)
            {
                if(playerItems[playerItemIndex].edible)
                {
                    contextCursor.GetComponentInChildren<TMP_Text>().text = "Eat " + playerItems[playerItemIndex].name;
                } 
                else
                {
                    contextCursor.GetComponentInChildren<TMP_Text>().text = "Skip Action";
                }
            }
            else if (actionableTiles().Contains(targ))
            {
                if (NPCPositions.ContainsKey(targ))
                {
                    contextCursor.GetComponentInChildren<TMP_Text>().text = "Attack " + NPCPositions[targ].name + " with " + playerItems[playerItemIndex].name;
                } 
                else
                {
                    contextCursor.GetComponentInChildren<TMP_Text>().text = "Toss " + playerItems[playerItemIndex].name + " here";
                }
            }
        }
    }
}
