using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Extensions
{
    static public float Degrees(this Vector2 v) => Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
}
