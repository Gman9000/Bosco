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
        Vector3 topRight, bottomLeft;

        topRight = bottomLeft = Camera.main.transform.position;
        topRight += Vector3.up * Screen.height / 2;
        topRight += Vector3.right * Screen.width / 2;
        bottomLeft += Vector3.down * Screen.height / 2;
        bottomLeft += Vector3.left * Screen.width / 2;

        Vector3Int topRightCell = darkTiles.WorldToCell(topRight);
        Vector3Int bottomLeftCell = darkTiles.WorldToCell(bottomLeft);

        for (int x = bottomLeftCell.x; x <= topRightCell.x; x++)
            for (int y = bottomLeftCell.y; y <= topRightCell.y; y++)
                darkTiles.SetTile(new Vector3Int(x, y, 0), darknessTile);


              

        foreach (LightSource light in lights)
        {
            Vector3 lightOrigin = light.transform.position;
            
            topRight = bottomLeft = lightOrigin;

            topRight += Vector3.up * light.strength / 2;
            topRight += Vector3.right * light.strength / 2;
                        
            bottomLeft += Vector3.down * light.strength / 2;
            bottomLeft += Vector3.left * light.strength / 2;

            topRightCell = darkTiles.WorldToCell(topRight);
            bottomLeftCell = darkTiles.WorldToCell(bottomLeft);

            Vector3Int lightOriginTile = darkTiles.WorldToCell(lightOrigin);

            // TODO: strength sometimes spits out squares and not circles

            for (int x = bottomLeftCell.x; x <= topRightCell.x; x++)
                for (int y = bottomLeftCell.y; y <= topRightCell.y; y++)
                {
                    float a = x - lightOriginTile.x;
                    float b = y - lightOriginTile.y;

                    float distance = Mathf.Sqrt(a * a + b * b);
                    float hyp = (light.strength / 2) * Mathf.Sqrt(2);

                    /*if (x == topRightCell.x && y == topRightCell.y)
                        Debug.Log(distance);*/

                    if (distance <= hyp / 2)
                        darkTiles.SetTile(new Vector3Int(x, y, 0), null);
                }

            // TODO: iterate through everything between the two tile coordinates defined above
            // and make it a circle
        }
    }

    public void AddLightSource(LightSource light) => lights.Add(light);

}
