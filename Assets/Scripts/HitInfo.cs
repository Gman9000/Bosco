using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HitInfo
{
    public string layerName;
    public readonly Collider2D collider;
    public readonly Vector2 contact;
    public readonly Vector2 normal;
    public readonly Vector2 tangent;

    public HitInfo(Collider2D collider, Vector2 normal, Vector2 contact)
    {
        this.layerName = LayerMask.LayerToName(collider.gameObject.layer);
        this.collider = collider;
        this.normal = normal;
        
        this.normal.x = Mathf.Round(normal.x * 10.0F) / 10.0F;
        this.normal.y = Mathf.Round(normal.y * 10.0F) / 10.0F;


        this.tangent = new Vector2(normal.y, -normal.x);
        this.contact = contact;
    }

    public HitInfo(string layerName = null)
    {
        this.layerName = layerName;
        this.collider = new Collider2D();
        this.normal = new Vector2();
        this.tangent = new Vector2();
        this.contact = new Vector2();
    }

    public static bool operator ==(HitInfo a, HitInfo b)
    {
        return a.Equals(b);
    }

    public static implicit operator bool(HitInfo a)
    {
        return a.layerName != null;
    }
    
    public static bool operator !=(HitInfo a, HitInfo b)
    {
        return !a.Equals(b);
    }

    override public bool Equals(object o)
    {
        return base.Equals(o);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
