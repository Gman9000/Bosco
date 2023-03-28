using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpammy : PawnEnemy
{
    public float moveSpeed = 3;
    public float atkMoveSpeed = 4;
    public GameObject atkbox;

    override public void Start()
    {
        DefState(EState.Idle, () => StateIdle());
        DefState(EState.Primary, () => StateAttack());
        base.Start();
    }

    override protected void OnHurt()
    {
        anim.Play(AnimMode.Looped, "hurt");
        atkbox.SetActive(false);
    }

    override protected IEnumerator DeathSequence()
    {
        anim.Play(AnimMode.Looped, "hurt");
        yield return base.DeathSequence();
    }

    IEnumerator StateAttack()
    {
        atkbox.SetActive(true);
        while (true)
        {
            anim.Play(AnimMode.Looped, "attack");
            yield return Act_WalkTowardPlayer(EState.Primary, atkMoveSpeed, .4F);
            yield return null;
        }
    }

    IEnumerator StateIdle()
    {
        atkbox.SetActive(false);
        while (true)
        {
            anim.Play(AnimMode.Looped, "walk");
            yield return Act_Walk(EState.Idle, moveSpeed);
            yield return null;
        }
    }
}
