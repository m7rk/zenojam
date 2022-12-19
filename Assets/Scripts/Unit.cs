using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public GameItem item;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // hop
        var xDif = Mathf.Abs(this.transform.position.x) % 0.5;
        // 0 - 0.25 up
        // 0.25 - 5 down
        var jumpPos = xDif > 0.25 ? (-xDif + 0.5) : xDif;
        GetComponentInChildren<SpriteRenderer>().transform.localPosition = new Vector3(0, JUMP_FACTOR * (float)jumpPos, 0);


        // anim idle (80 if last floor! first floor is 60)
        var BPS = (60f / 60f);
        var halfNote = Time.time % (2 / BPS);
        GetComponentInChildren<SpriteRenderer>().sprite = halfNote > (1f / BPS) ? (faceFront ? f1 : b1) : (faceFront ? f2 : b2);
        this.transform.localScale = !(faceRight ^ faceFront) ? Vector3.one : new Vector3(-1, 1, 1);

        // add a small Y component to Z to force sprite ordering
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 1 + (this.transform.position.y * 0.001f));
    }
}
