using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSimulator : MonoBehaviour
{
    public int tilevalue = 1;
    SpriteRenderer ren;

    public Vector2 SpritePos => ren.transform.position;


    public bool Flashed => ren.color.a > 0;

    void Start()
    {
        Game.simulatedSprites.Add(this);
        ren = GetComponentInChildren<SpriteRenderer>();
    }

    public void Flash(bool visible)
    {
        if (visible)
            ren.color = Color.white;
        else
            ren.color = new Color(0,0,0,0);
    }

    void OnDestroy()
    {
        Game.RemoveSimulatedSprite(this);
    }
}
