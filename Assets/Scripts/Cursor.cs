using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Cursor : MonoBehaviour
{
    public Tilemap tileGrid;
    public GridUtils gu;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var tilePos = tileGrid.WorldToCell(worldPos);
            processClick(tilePos);
        }
    }

    void processClick(Vector3Int click)
    {
        var reachable = gu.BFS(click, 2);
        Debug.Log("c" + reachable.Count);
    }
}
