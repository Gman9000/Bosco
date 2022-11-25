using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RaycastChecks
{
    public static bool IsGrounded(this BoxCollider2D collider, Vector2 position, float playerScale)
    {
        //scale of player is same on x and y
        int layerMask = LayerMask.GetMask("Ground");

        Debug.DrawRay(new Vector2(position.x + 0.95f * (collider.size.x * playerScale) / 2, position.y), 1.05f * (collider.size.x * playerScale) * Vector2.down, Color.green, Time.fixedDeltaTime);
        Debug.DrawRay(new Vector2(position.x - 0.95f * (collider.size.x * playerScale) / 2, position.y), 1.05f * (collider.size.x * playerScale) * Vector2.down, Color.green, Time.fixedDeltaTime);
        return Physics2D.Raycast(new Vector2(position.x + 0.95f * (collider.size.x * playerScale) / 2, position.y), Vector2.down, 1.05f * (collider.size.x * playerScale), layerMask) ||
            Physics2D.Raycast(new Vector2(position.x - 0.95f * (collider.size.x * playerScale) / 2, position.y), Vector2.down, 1.05f * (collider.size.x * playerScale), layerMask);
    }
}
