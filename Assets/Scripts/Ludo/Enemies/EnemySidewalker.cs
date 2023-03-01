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
        stateIdle = new System.Func<IEnumerator>(() => Act_Inching(xMove, moveDuration, moveWait));
        statePrimary = new System.Func<IEnumerator>(() => Act_Idle());
        base.Start();
    }    
}
