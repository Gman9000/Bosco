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
            return new HitInfo(hitAB);
        else if (hitBA)
            return new HitInfo(hitBA);
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

    public static HitInfo IsHittingCeiling(this BoxCollider2D collider, Vector2 raycastA, Vector2 raycastB, float rayCastMagnitude)
    {
        //scale of player is same on x and y
        int layerMask = LayerMask.GetMask("Ground", "Hidden");
        Debug.DrawRay(new Vector2(raycastA.x, raycastA.y), rayCastMagnitude * (collider.size.x / 2) * Vector2.up, Color.green);
        Debug.DrawRay(new Vector2(raycastB.x, raycastB.y), rayCastMagnitude * (collider.size.x / 2) * Vector2.up, Color.green);
        RaycastHit2D hitAB = Physics2D.Raycast(new Vector2(raycastA.x, raycastA.y), Vector2.up, rayCastMagnitude * (collider.size.x / 2), layerMask);
        RaycastHit2D hitBA = Physics2D.Raycast(new Vector2(raycastB.x, raycastB.y), Vector2.up, rayCastMagnitude * (collider.size.x / 2), layerMask);

        if (hitAB)  return new HitInfo(hitAB);
        else if (hitBA)  return new HitInfo(hitBA);
        else return new HitInfo();
    }

    public static HitInfo IsHittingCeiling(this BoxCollider2D collider, float raycastMagntiude)
    {
        Vector2 leftCast = collider.bounds.center;
        Vector2 rightCast = collider.bounds.center;
        leftCast.x -= collider.bounds.size.x / 2;
        leftCast.y += collider.bounds.size.y / 4;
        
        rightCast.x += collider.bounds.size.x / 2;
        rightCast.y = leftCast.y;

        return IsHittingCeiling(collider, leftCast, rightCast, raycastMagntiude);
    }

    public static HitInfo IsHittingRight(this BoxCollider2D collider, Vector2 raycastA, Vector2 raycastB, float rayCastMagnitude)
    {
        //scale of player is same on x and y
        int layerMask = LayerMask.GetMask("Ground", "Hidden");

        Debug.DrawRay(new Vector2(raycastA.x, raycastA.y), rayCastMagnitude * (collider.size.x / 2) * Vector2.right, Color.blue);
        Debug.DrawRay(new Vector2(raycastB.x, raycastB.y), rayCastMagnitude * (collider.size.x / 2) * Vector2.right, Color.blue);
        RaycastHit2D hitAB = Physics2D.Raycast(new Vector2(raycastA.x, raycastA.y), Vector2.right, rayCastMagnitude * (collider.size.x / 2), layerMask);
        RaycastHit2D hitBA = Physics2D.Raycast(new Vector2(raycastB.x, raycastB.y), Vector2.right, rayCastMagnitude * (collider.size.x / 2), layerMask);

        if (hitAB)   return new HitInfo(hitAB);
        else if (hitBA)   return new HitInfo(hitBA);
        else return new HitInfo();
    }

    public static HitInfo IsHittingRight(this BoxCollider2D collider, float raycastMagntiude)
    {
        Vector2 topCast = collider.bounds.center;
        Vector2 bottomCast = collider.bounds.center;
        topCast.x += collider.bounds.size.x / 2;
        topCast.y += collider.bounds.size.y / 4;
        
        bottomCast.x = topCast.x;
        bottomCast.y -= collider.bounds.size.x / 2;

        return IsHittingRight(collider, topCast, bottomCast, raycastMagntiude);
    }

    public static HitInfo IsHittingLeft(this BoxCollider2D collider, Vector2 raycastA, Vector2 raycastB, float rayCastMagnitude)
    {
        //scale of player is same on x and y
        int layerMask = LayerMask.GetMask("Ground", "Hidden");

        Debug.DrawRay(new Vector2(raycastA.x, raycastA.y), rayCastMagnitude * (collider.size.x / 2) * Vector2.left, Color.blue);
        Debug.DrawRay(new Vector2(raycastB.x, raycastB.y), rayCastMagnitude * (collider.size.x / 2) * Vector2.left, Color.blue);
        RaycastHit2D hitAB = Physics2D.Raycast(new Vector2(raycastA.x, raycastA.y), Vector2.left, rayCastMagnitude * (collider.size.x / 2), layerMask);
        RaycastHit2D hitBA = Physics2D.Raycast(new Vector2(raycastB.x, raycastB.y), Vector2.left, rayCastMagnitude * (collider.size.x / 2), layerMask);

        if (hitAB)  return new HitInfo(hitAB);
        else if (hitBA)  return new HitInfo(hitBA);
        else return new HitInfo();
    }

    public static HitInfo IsHittingLeft(this BoxCollider2D collider, float raycastMagntiude)
    {
        Vector2 topCast = collider.bounds.center;
        Vector2 bottomCast = collider.bounds.center;
        topCast.x -= collider.bounds.size.x / 2;
        topCast.y += collider.bounds.size.y / 4;
        
        bottomCast.x = topCast.x;
        bottomCast.y -= collider.bounds.size.x / 2;

        return IsHittingLeft(collider, topCast, bottomCast, raycastMagntiude);
    }
}
