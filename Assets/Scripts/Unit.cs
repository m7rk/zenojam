using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// this class doesn't really work for ai and player :(((((((((((((((((((
public class Unit : MonoBehaviour
{
    private readonly float JUMP_FACTOR = 0.3f;

    public Sprite f1;
    public Sprite f2;

    public Sprite b1;
    public Sprite b2;

    public bool faceFront = true;
    public bool faceRight = true;

    public int speed;
    public int health;
    public int AI_range;

    public GameObject healthPrefab;

    public bool thisIsPlayer;

    public bool aggro;
    public bool pacified;

    public Sprite[] attackFront;
    public Sprite[] attackBack;

    public Sprite[] altAttackFront;
    public Sprite[] altAttackBack;

    public Sprite hurtF;
    public Sprite hurtB;

    private static Material hit;
    private static Material outlineMat;
    private Material oldMaterial;

    private float MAX_HURT_FLASH_TIME = 0.2f;
    private float HURT_ANIM_TIME = 0.1f;
    private float flashTime = 0f;
    // Start is called before the first frame update

    public bool outline = false;

    public SpriteRenderer mainSpriteRenderer;

    public SpriteRenderer playerBook;

    public Sprite AIRangedProjectile;

    public GameObject[] playerMeleesF;
    public GameObject[] playerMeleesB;

    void Start()
    {
        if (hit == null)
        {
            hit = Resources.Load<Material>("Hit");
            outlineMat = Resources.Load<Material>("Outline");

        }

        mainSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        oldMaterial = GetComponentInChildren<SpriteRenderer>().material;
        if (!thisIsPlayer)
        {
            var v = Instantiate(healthPrefab);
            v.transform.SetParent(this.transform);
            v.transform.localPosition = Vector3.zero;
            v.GetComponent<HealthBar>().setMax(health);

            // offset above player properly - need a custom field but we do this later.
            v.transform.localPosition = new Vector3((-0.0625f / 2f) * health, 0.21f, 0);
        }

        aggro = false;
    }

    // Update is called once per frame
    void Update()
    {
        flashTime -= Time.deltaTime;
        if (outline)
        {
            mainSpriteRenderer.material = outlineMat;
        }
        else
        {
            if (flashTime > HURT_ANIM_TIME)
            {
                mainSpriteRenderer.material = hit;
            }
            else
            {
                mainSpriteRenderer.material = oldMaterial;
            }
        }

        // hop
        var xDif = Mathf.Abs(this.transform.position.x) % 0.5;
        // 0 - 0.25 up
        // 0.25 - 5 down
        var jumpPos = xDif > 0.25 ? (-xDif + 0.5) : xDif;
        mainSpriteRenderer.transform.localPosition = new Vector3(0, JUMP_FACTOR * (float)jumpPos, 0);


        // anim idle (80 if last floor! first floor is 60)
        var BPS = (60f / 60f);
        var halfNote = Time.time % (2 / BPS);
        mainSpriteRenderer.sprite = halfNote > (1f / BPS) ? (faceFront ? f1 : b1) : (faceFront ? f2 : b2);
        this.transform.GetChild(0).transform.localScale = !(faceRight ^ faceFront) ? Vector3.one : new Vector3(-1, 1, 1);

        // add a small Y component to Z to force sprite ordering
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 1 + (this.transform.position.y * 0.001f));

        if(flashTime > 0)
        {
            mainSpriteRenderer.sprite = faceFront ? hurtF : hurtB;

        } else
        {
            if (health <= 0 && !thisIsPlayer)
            {
                Destroy(this.gameObject);
            }
            // keep flashed until transitioner kicks in
            if(health <= 0 && thisIsPlayer)
            {
                flashTime = 0.1f;
            }
        }
    }

    // return true if ded
    public bool hurt(int count)
    {
        flashTime = MAX_HURT_FLASH_TIME;
        if (thisIsPlayer)
        {
            health -= 1;
            // route elsewhere
            return false;
        }

        for (int i = 0; i != count; ++i)
        {
            GetComponentInChildren<HealthBar>(true).hurt();
            health -= 1;
            if(health == 0)
            {
                return true;
            }
        }
        return false;
    }


    public void showBook(Sprite book)
    {
        playerBook.gameObject.SetActive(true);
        playerBook.GetComponent<SpriteRenderer>().sprite = book;
        playerBook.transform.localPosition = new Vector3(playerBook.transform.localPosition.x, playerBook.transform.localPosition.y, faceFront ? -0.01f : 0.01f);
    }

    public void showWeapon(string name, int frame)
    {
        foreach(var v in playerMeleesF)
        {
            v.SetActive(false);
        }

        var pm = playerMeleesF[frame];

        if (!faceFront)
        {
            pm = playerMeleesB[frame];
        }

        pm.SetActive(true);
        pm.transform.Find(name).gameObject.SetActive(true);
    }

    public void hideWeapons()
    {
        playerBook.gameObject.SetActive(false);
        foreach (var v in playerMeleesF)
        {
            for (int i = 0; i != v.transform.childCount; ++i)
            {
                v.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        foreach (var v in playerMeleesB)
        {
            for (int i = 0; i != v.transform.childCount; ++i)
            {
                v.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

    }
}
