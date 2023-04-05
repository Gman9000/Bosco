using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum AtkBonusAbility {None, RenewAerialAttack}
public class PlayerAttack : MonoBehaviour
{
    public int damage = 1;
    public float stunFactor = 1;
    public Vector2 hitDirection = Vector2.zero;
    public bool relativeToFacingDir = false;
    public bool ignoreY = false;
    public Vector2 playerFeedbackDirection = Vector2.zero;
    public AtkBonusAbility bonusAbility = AtkBonusAbility.None;

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy" && gameObject.activeSelf)
        {
            HitEnemy(other);
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy" && gameObject.activeSelf)
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
                hitDirectionModified.x = Player.FacingDirection * hitDirection.x;
            else
                hitDirectionModified.x = Mathf.Sign(enemyCollider.transform.position.x - Player.Position.x) * hitDirection.x;
        }
        else if (hitDirectionModified == Vector2.zero)
            hitDirectionModified = (enemyCollider.transform.position - Player.Position).normalized;
            
        if (ignoreY)
            hitDirectionModified.y = 0;            

        hitDirectionModified = Game.RestrictDiagonals(hitDirectionModified) * hitDirection.magnitude;
        Player.Instance.hittingEnemyScript = enemyCollider.GetComponentInChildren<PawnEnemy>();

        bool attackSuccess = Player.Instance.hittingEnemyScript.InflictDamage(hitDirectionModified, damage, stunFactor);
        if (attackSuccess)
        {
            Game.VertShake(2);
            if (Player.Instance.isGrounded)
            Game.FreezeFrame(Game.FRAME_TIME * 4, () => {
                Player.Instance.hittingEnemyScript = null;          
            });
            Player.Instance.AttackFeedback(playerFeedbackDirection, hitDirectionModified, bonusAbility);
        }
    }
}
