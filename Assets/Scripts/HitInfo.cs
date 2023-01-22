using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HitInfo
{
    public string layerName;
    public RaycastHit2D hit;

    public HitInfo(string layerName, RaycastHit2D hit)
    {
        this.layerName = layerName;
        this.hit = hit;
    }

    public HitInfo(string layerName = null)
    {
        this.layerName = layerName;
        this.hit = new RaycastHit2D();
    }

    public static bool operator ==(HitInfo a, HitInfo b)
    {
        return a.Equals(b);
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
