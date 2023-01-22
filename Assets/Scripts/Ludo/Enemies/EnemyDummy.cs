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
}
