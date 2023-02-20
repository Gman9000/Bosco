using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySidewalker : PawnEnemy
{
    override public void Start()
    {
        statePrimary = new System.Func<IEnumerator>(() => Act_Inching());
        stateIdle = new System.Func<IEnumerator>(() => Act_Idle());
        base.Start();
    }    
}
