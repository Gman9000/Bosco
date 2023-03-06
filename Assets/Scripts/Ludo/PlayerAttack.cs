using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum AtkBonusAbility {None, RenewAerialAttack}
public class PlayerAttack : MonoBehaviour
{
    public Vector2 hitDirection = Vector2.zero;
    public bool relativeToFacingDir = false;
    public bool ignoreY = false;
    public Vector2 playerFeedbackDirection = Vector2.zero;
    public AtkBonusAbility bonusAbility = AtkBonusAbility.None;

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
        {
            if (relativeToFacingDir)
                hitDirectionModified.x = Player.FacingDirection * Mathf.Sign(hitDirection.x);
            else
                hitDirectionModified.x = Mathf.Sign(enemyCollider.transform.position.x - Player.Position.x) * Mathf.Sign(hitDirection.x);
        }
        else if (hitDirectionModified == Vector2.zero)
            hitDirectionModified = (enemyCollider.transform.position - Player.Position).normalized;
            
        if (ignoreY)
            hitDirectionModified.y = 0;            

        hitDirectionModified = Game.RestrictDiagonals(hitDirectionModified);
        bool attackSuccess = enemyCollider.GetComponentInChildren<PawnEnemy>().TakeDamage(hitDirectionModified);
        if (attackSuccess)
        {
            Player.Instance.AttackFeedback(playerFeedbackDirection, hitDirectionModified, bonusAbility);
        }
    }
}
