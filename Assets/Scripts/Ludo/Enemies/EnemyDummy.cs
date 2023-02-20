using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDummy : PawnEnemy
{
    override protected void OnHit()
    {
        anim.Play(AnimMode.Looped, "hurt");
    }
}
