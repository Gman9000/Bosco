using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSimulator : MonoBehaviour
{
    public const int SCANLINE_LIMIT         = 10;
    static readonly public bool flashOnLimit = true;


    public int tilevalue = 1;
    SpriteRenderer ren;

    public Vector2 SpritePos => ren.transform.position;


    public bool Flashed => ren.color.a > 0;

    bool _outOfView = false;
    [HideInInspector] public bool OutOfView => _outOfView;

    void Start()
    {
        Game.simulatedSprites.Add(this);
        ren = GetComponentInChildren<SpriteRenderer>();
        _outOfView = false;
    }

    public void Flash(bool visible)
    {
        if (OutOfView)  return;

        ren.enabled = visible;
    }

    public void SetOutOfView(bool hide)
    {
        _outOfView = hide;
        
        ren.enabled = !hide;
    }

    void OnDestroy()
    {
        Game.RemoveSimulatedSprite(this);
    }
}
