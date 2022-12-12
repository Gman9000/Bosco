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
            CameraController.Instance.VertShake(2);
            Player.Instance.DoInputMovePause(.45F);
            Player.Instance.DoInputAttackPause(.45F);
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
        }
    }


}
