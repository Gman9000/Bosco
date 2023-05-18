using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    /*=========*\
    |  Vector2  |
    \*=========*/
    public static float Degrees(this Vector2 v) => Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;

    /*========*\
    |  Bounds  |
    \*========*/

    public static float Top(this Bounds bounds) => bounds.center.y + bounds.size.y / 2;
    public static float Bottom(this Bounds bounds) => bounds.center.y - bounds.size.y / 2;
    public static float Left(this Bounds bounds) => bounds.center.x - bounds.size.x / 2;
    public static float Right(this Bounds bounds) => bounds.center.x + bounds.size.x / 2;

    /*===============*\
    |  BoxCollider2D  |
    \*===============*/

    //
    //  Bounds Shortcuts
    //
    public static float Top(this BoxCollider2D collider) => collider.bounds.Top();
    public static float Bottom(this BoxCollider2D collider) => collider.bounds.Bottom();
    public static float Left(this BoxCollider2D collider) => collider.bounds.Left();
    public static float Right(this BoxCollider2D collider) => collider.bounds.Right();
    public static Vector2 Center(this BoxCollider2D collider) => collider.bounds.center;


    //
    //  Raycasting
    //
    public static HitInfo IsGrounded(this BoxCollider2D collider, Vector2 raycastA, Vector2 raycastB, float rayCastMagnitude, string[] layers)
    {
        //scale of player is same on x and y
        int layerMask = LayerMask.GetMask(layers);

        Debug.DrawRay(new Vector2(raycastA.x, raycastA.y), rayCastMagnitude * (collider.size.x / 2) * Vector2.down, Color.green);
        Debug.DrawRay(new Vector2(raycastB.x, raycastB.y), rayCastMagnitude * (collider.size.x / 2) * Vector2.down, Color.green);
        Debug.DrawRay(new Vector2(raycastB.x, raycastB.y), rayCastMagnitude * (collider.size.x / 2) * Vector2.down, Color.green);
        RaycastHit2D hitAB = Physics2D.Raycast(new Vector2(raycastA.x, raycastA.y), Vector2.down, rayCastMagnitude * (collider.size.x / 2), layerMask);
        RaycastHit2D hitBA = Physics2D.Raycast(new Vector2(raycastB.x, raycastB.y), Vector2.down, rayCastMagnitude * (collider.size.x / 2), layerMask);
        if (hitAB)
            return new HitInfo(hitAB.collider, hitAB.normal, hitAB.point);
        else if (hitBA)
            return new HitInfo(hitBA.collider, hitBA.normal, hitBA.point);
        else
            return new HitInfo();
    }

    public static HitInfo IsGrounded(this BoxCollider2D collider, float raycastMagntiude, string[] layers)
    {
        Vector2 leftCast = collider.bounds.center;
        Vector2 rightCast = collider.bounds.center;
        leftCast.x -= collider.bounds.size.x / 2;
        leftCast.y -= collider.bounds.size.y / 4;
        
        rightCast.x += collider.bounds.size.x / 2;
        rightCast.y = leftCast.y;

        return IsGrounded(collider, leftCast, rightCast, raycastMagntiude, layers);
    }

    public static bool CheckHitDown(this BoxCollider2D collider, Vector2 point)
    {
        return Mathf.Abs(point.y - collider.Bottom()) < Game.PIXEL * 2 && 
        point.y <= collider.Top();
    }
    public static bool CheckHitSlope(this BoxCollider2D collider, Vector2 normal, Vector2 point)
    {
        return Mathf.Abs(normal.x) < 1 &&
        normal.y < 1 && normal.y > 0 &&
        Mathf.Abs(point.x - collider.bounds.center.x) <= .7F &&
        point.y - collider.bounds.center.y < 0 &&
        point.y - collider.bounds.center.y >= -.5F;
    }

    public static bool CheckHitUp(this BoxCollider2D collider, Vector2 point)
    {
        return Mathf.Abs(point.y - collider.Top()) < Game.PIXEL * 2 && 
        point.y >= collider.Bottom();
    }
    public static bool CheckHitLeft(this BoxCollider2D collider, Vector2 point)
    {
        return Mathf.Abs(point.x - collider.Left()) < Game.PIXEL * 2 && 
        point.x <= collider.Right();
    }
    public static bool CheckHitRight(this BoxCollider2D collider, Vector2 point)
    {
        return Mathf.Abs(point.x - collider.Right()) < Game.PIXEL * 2 && 
        point.x >= collider.Left();
    }



}
