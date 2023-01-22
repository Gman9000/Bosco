using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDummy : PawnEnemy
{
    override public void Start()
    {
        base.Start();
    }
    override protected void UpdateState0()
    {
        base.UpdateState0();
        if (anim.CurrentAnim != anim.defaultAnimation)
            anim.PlayDefault();
    }
    override protected void OnHit()
    {
        anim.Play(AnimMode.Looped, "hurt");
    }
}
