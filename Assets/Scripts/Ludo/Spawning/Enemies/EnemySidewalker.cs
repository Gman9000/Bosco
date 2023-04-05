using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySidewalker : PawnEnemy
{
    public float xMove = -1F;
    public float moveDuration = .5F;
    public float moveWait = .2F;
    override public void Start()
    {
        DefState(EState.Idle, () => StateIdle());
        base.Start();
    }

    override protected void OnHurt()
    {
        anim.Play(AnimMode.Looped, "hurt");
    }

    override protected IEnumerator DeathSequence()
    {
        anim.Play(AnimMode.Looped, "hurt");
        yield return base.DeathSequence();
    }

    IEnumerator StateIdle()
    {
        while (true)
        {
            yield return Act_Inching(EState.Idle, xMove, moveDuration, moveWait, 
            () => anim.Play(AnimMode.Looped, "move"),
            () => anim.PlayDefault());
            yield return null;
        }
    }
}
