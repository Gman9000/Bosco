using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDummy : PawnEnemy
{
    override protected void UpdateState0()
    {
        base.UpdateState0();
        anim.PlayDefault();
    }
    override protected void OnHit()
    {
        anim.Play(AnimMode.Looped, "hurt");
    }

    protected override void UpdateKnockback(Vector2 moveToPoint)
    {
        Vector2 upness = Vector2.up * .25F;
        if (moveToPoint.y < positionWhenHit.y)
            upness = Vector2.zero;
        body.MovePosition(positionWhenHit + (moveToPoint + upness - positionWhenHit) * InvProgress);
    }


}
