using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDummy : PawnEnemy
{
    override protected void UpdateState0()
    {
        anim.PlayDefault();
        base.UpdateState0();
    }
    override protected void OnHit()
    {
        anim.Play(AnimMode.Looped, "hurt");
    }

    protected override Vector2 UpdateKnockback(Vector2 moveToPoint)
    {
        Vector2 upness = Vector2.up * .25F;
        if (moveToPoint.y < positionWhenHit.y)
            upness = Vector2.zero;
        return positionWhenHit + (moveToPoint + upness - positionWhenHit) * InvProgress;
    }


}
