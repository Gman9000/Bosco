using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Vector2 hitDirection = Vector2.zero;
    public bool ignoreY = false;
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            HitEnemy(other);
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            HitEnemy(other);
        }
    }

    private void HitEnemy(Collider2D enemyCollider)
    {
        Vector2 hitDirectionModified = hitDirection;

        if (hitDirectionModified.x != 0)
            hitDirectionModified.x = Player.FacingDirection;
        else if (hitDirectionModified == Vector2.zero)
            hitDirectionModified = (enemyCollider.transform.position - Player.Position).normalized;
            
        if (ignoreY)
            hitDirectionModified.y = 0;            

        hitDirectionModified = Game.RestrictDiagonals(hitDirectionModified);
        enemyCollider.GetComponentInChildren<PawnEnemy>().TakeDamage(hitDirectionModified);
        Debug.Log("Butt");
    }
}
