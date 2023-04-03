using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HitInfo
{
    public string layerName;
    public Collider2D collider;
    public Vector2 contact;

    public HitInfo(Collider2D collider, Vector2 contact)
    {
        this.layerName = LayerMask.LayerToName(collider.gameObject.layer);
        this.collider = collider;
        this.contact = contact;
    }

    public HitInfo(string layerName = null)
    {
        this.layerName = layerName;
        this.collider = new Collider2D();
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
