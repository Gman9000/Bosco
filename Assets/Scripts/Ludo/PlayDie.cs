using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayDie : MonoBehaviour
{   void Start()
    {
        GetComponent<SpriteAnimator>().Play(AnimMode.OnceDie, "default");
    }
}
