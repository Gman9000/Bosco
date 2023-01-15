using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

public class DarkHandler : MonoBehaviour
{
    static public DarkHandler Instance;
    [HideInInspector]public List<LightSource> lights;
    Tilemap darkTiles;
    public bool darkOn = false;
    public TileBase darknessTile;

    void Awake()
    {
        Instance = this;
        lights = new List<LightSource>();
        darkTiles = GetComponent<Tilemap>();
    }

    void FixedUpdate()
    {
        if (!darkOn)    return;
        
        // THIS CODE CRASHES THE BUILD DUE TO EXCESSIVELOOPING, I'M ASSUMING? PLEASE REVIEW
        Vector3 topRight, bottomLeft;

        bottomLeft = CameraController.viewRect.min;
        topRight = CameraController.viewRect.max;
        

        Vector3Int topRightCell = darkTiles.WorldToCell(topRight);
        Vector3Int bottomLeftCell = darkTiles.WorldToCell(bottomLeft);

        for (int x = bottomLeftCell.x; x <= topRightCell.x; x++)
            for (int y = bottomLeftCell.y; y <= topRightCell.y; y++)
                darkTiles.SetTile(new Vector3Int(x, y, 0), darknessTile);



        float time = Time.time * 1.5F;
        float lightRim = Game.PingPong(time) / 2.0F;
        

        foreach (LightSource light in lights)
        {            
            if (light.lit)
            {
                float lightDiameter = light.strength;
                if (light.doesShimmer)
                    lightDiameter += lightRim * light.strength / 2;
                Vector3 lightOrigin = light.transform.position;
                
                topRight = bottomLeft = lightOrigin;

                topRight += Vector3.up * lightDiameter / 2;
                topRight += Vector3.right * lightDiameter / 2;
                            
                bottomLeft += Vector3.down * lightDiameter / 2;
                bottomLeft += Vector3.left * lightDiameter / 2;

                topRightCell = darkTiles.WorldToCell(topRight);
                bottomLeftCell = darkTiles.WorldToCell(bottomLeft);

                Vector3Int lightOriginTile = darkTiles.WorldToCell(lightOrigin);

                for (int x = bottomLeftCell.x; x <= topRightCell.x; x++)
                    for (int y = bottomLeftCell.y; y <= topRightCell.y; y++)
                    {
                        int hash = 23;
                        hash = hash * 31 + x;
                        hash = hash * 31 + y;
                        hash = hash * 31 + Mathf.FloorToInt(time);
                        Random.InitState(hash);
                        
                        float a = x - lightOriginTile.x;
                        float b = y - lightOriginTile.y;

                        float distance = Mathf.Sqrt(a * a + b * b);
                        float hyp = (lightDiameter / 2) * Mathf.Sqrt(2);

                        if (distance <= hyp / 2)
                        {
                            if (distance < light.strength / 3 || Random.value < .15F)
                                darkTiles.SetTile(new Vector3Int(x, y, 0), null);
                        }
                    }
            }
        }
    }

    public void AddLightSource(LightSource light) => lights.Add(light);

}
