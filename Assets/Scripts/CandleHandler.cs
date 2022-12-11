using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleHandler : MonoBehaviour
{
    public Candle[] candles;

    SpriteAnimator animator;

    private bool canUseDoor = false;

    void Awake()
    {
        animator = GetComponentInChildren<SpriteAnimator>();
    }

    void FixedUpdate()
    {
        if (animator.CurrentAnim == "open" && !animator.AtEnd)
        {
            CameraController.Instance.HorShake(1);
            Player.Instance.DoPhysicsPause(.5F);
            Player.Instance.DoInputMovePause(.5F);
            Player.Instance.DoInputAttackPause(.5F);
            Player.Instance.animator.PlayDefault();
        }
    }

    private void CandleCheck()
    {
        if(Game.litCandlesCount == candles.Length)
        {
            canUseDoor = true;
            animator.Play(AnimMode.Hang, "open");
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CandleCheck();
            animator.Play(AnimMode.Hang, "open");
        }
    }


}
