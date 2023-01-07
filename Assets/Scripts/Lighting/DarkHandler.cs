using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

public class DarkHandler : MonoBehaviour
{
    static public DarkHandler Instance;
    [HideInInspector]public List<LightSource> lights;
    Tilemap darkTiles;

    public TileBase darknessTile;

    void Awake()
    {
        Instance = this;
        lights = new List<LightSource>();
        darkTiles = GetComponent<Tilemap>();
    }

    void FixedUpdate()
    {
        Vector3 topLeft, bottomRight;

        topLeft = bottomRight = Camera.main.transform.position;
        topLeft += Vector3.up * Screen.height / 2;
        topLeft += Vector3.left * Screen.width / 2;
        bottomRight += Vector3.down * Screen.height / 2;
        bottomRight += Vector3.right * Screen.width / 2;

        Vector3Int topLeftCell = darkTiles.WorldToCell(topLeft);
        Vector3Int bottomRightCell = darkTiles.WorldToCell(bottomRight);

        /*for (int x = topLeftCell.x; x <= bottomRightCell.x; x++)
            for (int y = topLeftCell.y; y <= bottomRightCell.y; y++)
                darkTiles.SetTile(new Vector3Int(x, y, 0), darknessTile);*/


                NOTE : // the solution is that it needs to be bottom left and top right
                // not bottom right and top left. down is negative, remember infinite loops abound

        foreach (LightSource light in lights)
        {
            Vector3 lightOrigin = light.transform.position;
            
            topLeft = bottomRight = lightOrigin;

            topLeft += Vector3.left * light.strength / 2;
            topLeft += Vector3.up * light.strength / 2;
            bottomRight += Vector3.right * light.strength / 2;
            bottomRight += Vector3.down * light.strength / 2;

            topLeftCell = darkTiles.WorldToCell(topLeft);
            bottomRightCell = darkTiles.WorldToCell(bottomRight);

            Debug.Log("x =" + topLeftCell.x + " < " +  bottomRightCell.x);
            Debug.Log("y =" + topLeftCell.y + " < " +  bottomRightCell.y);

            for (int x = topLeftCell.x; x <= bottomRightCell.x; x++)
                for (int y = topLeftCell.y; y <= bottomRightCell.y; y++)
                {
                    darkTiles.SetTile(new Vector3Int(x, y, 0), null);
                    Debug.Log(x + "  ,   " + y);
                }

            // TODO: iterate through everything between the two tile coordinates defined above
            // and make it a circle
        }
    }

    public void AddLightSource(LightSource light) => lights.Add(light);

}
