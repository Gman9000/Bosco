using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RaycastChecks
{
    public static HitInfo IsGrounded(this BoxCollider2D collider, Vector2 leftRaycast, Vector2 rightRaycast, float rayCastMagnitude)
    {
        //scale of player is same on x and y
        int layerMask = LayerMask.GetMask("Ground", "Hidden", "TwoWayPlatform");

        Debug.DrawRay(new Vector2(leftRaycast.x, leftRaycast.y), rayCastMagnitude * (collider.size.x / 2) * Vector2.down, Color.green, Time.fixedDeltaTime);
        Debug.DrawRay(new Vector2(rightRaycast.x, rightRaycast.y), rayCastMagnitude * (collider.size.x / 2) * Vector2.down, Color.green, Time.fixedDeltaTime);
        RaycastHit2D leftRight = Physics2D.Raycast(new Vector2(leftRaycast.x, leftRaycast.y), Vector2.down, rayCastMagnitude * (collider.size.x / 2), layerMask);
        RaycastHit2D rightLeft = Physics2D.Raycast(new Vector2(rightRaycast.x, rightRaycast.y), Vector2.down, rayCastMagnitude * (collider.size.x / 2), layerMask);
        if (leftRight)
            return new HitInfo(LayerMask.LayerToName(leftRight.collider.gameObject.layer), leftRight);
        else if (rightLeft)
            return new HitInfo(LayerMask.LayerToName(rightLeft.collider.gameObject.layer), rightLeft);
        else
            return new HitInfo();
    }

    public static HitInfo IsGrounded(this BoxCollider2D collider, float raycastMagntiude)
    {
        Vector2 leftCast = collider.bounds.center;
        Vector2 rightCast = collider.bounds.center;
        leftCast.x -= collider.bounds.size.x / 2;
        leftCast.y -= collider.bounds.size.y / 4;
        
        rightCast.x += collider.bounds.size.x / 2;
        rightCast.y = leftCast.y;

        return IsGrounded(collider, leftCast, rightCast, raycastMagntiude);
    }

    public static bool IsHittingCeiling(this BoxCollider2D collider, Vector2 leftRaycast, Vector2 rightRaycast, float playerScale, float rayCastMagnitude)
    {
        //scale of player is same on x and y
        int layerMask = LayerMask.GetMask("Ground", "Hidden");
        Debug.DrawRay(new Vector2(leftRaycast.x, leftRaycast.y), rayCastMagnitude * (collider.size.x / 2 * playerScale) * Vector2.up, Color.green, Time.fixedDeltaTime);
        Debug.DrawRay(new Vector2(rightRaycast.x, rightRaycast.y), rayCastMagnitude * (collider.size.x / 2 * playerScale) * Vector2.up, Color.green, Time.fixedDeltaTime);
        return Physics2D.Raycast(new Vector2(leftRaycast.x, leftRaycast.y), Vector2.up, rayCastMagnitude * (collider.size.x / 2 * playerScale), layerMask) ||
               Physics2D.Raycast(new Vector2(rightRaycast.x, rightRaycast.y), Vector2.up, rayCastMagnitude * (collider.size.x / 2 * playerScale), layerMask);
    }
    public static bool IsHittingRightWall(this BoxCollider2D collider, Vector2 leftRaycast, Vector2 rightRaycast, float playerScale, float rayCastMagnitude)
    {
        //scale of player is same on x and y
        int layerMask = LayerMask.GetMask("Ground", "Hidden");

        Debug.DrawRay(new Vector2(leftRaycast.x, leftRaycast.y), rayCastMagnitude * (collider.size.x / 2 * playerScale) * Vector2.right, Color.blue, Time.fixedDeltaTime);
        Debug.DrawRay(new Vector2(rightRaycast.x, rightRaycast.y), rayCastMagnitude * (collider.size.x / 2 * playerScale) * Vector2.right, Color.blue, Time.fixedDeltaTime);
        return Physics2D.Raycast(new Vector2(leftRaycast.x, leftRaycast.y), Vector2.right, rayCastMagnitude * (collider.size.x / 2 * playerScale), layerMask) ||
               Physics2D.Raycast(new Vector2(rightRaycast.x, rightRaycast.y), Vector2.right, rayCastMagnitude * (collider.size.x / 2 * playerScale), layerMask);
    }

    public static bool IsHittingLeftWall(this BoxCollider2D collider, Vector2 leftRaycast, Vector2 rightRaycast, float playerScale, float rayCastMagnitude)
    {
        //scale of player is same on x and y
        int layerMask = LayerMask.GetMask("Ground", "Hidden");

        Debug.DrawRay(new Vector2(leftRaycast.x, leftRaycast.y), rayCastMagnitude * (collider.size.x / 2 * playerScale) * Vector2.left, Color.blue, Time.fixedDeltaTime);
        Debug.DrawRay(new Vector2(rightRaycast.x, rightRaycast.y), rayCastMagnitude * (collider.size.x / 2 * playerScale) * Vector2.left, Color.blue, Time.fixedDeltaTime);
        return Physics2D.Raycast(new Vector2(leftRaycast.x, leftRaycast.y), Vector2.left, rayCastMagnitude * (collider.size.x / 2 * playerScale), layerMask) ||
               Physics2D.Raycast(new Vector2(rightRaycast.x, rightRaycast.y), Vector2.left, rayCastMagnitude * (collider.size.x / 2 * playerScale), layerMask);
    }
}
