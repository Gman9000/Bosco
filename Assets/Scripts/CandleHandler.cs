using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleHandler : MonoBehaviour
{
    public Candle[] candles;
    private int numberOfLitCandles;
    // Start is called before the first frame update

    Animator animator;

    private bool canUseDoor = false;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        animator.speed = 0;
    }

    void Update()
    {
        if (animator.speed > 0 && animator.GetCurrentAnimatorStateInfo(0).IsName("DoorOpen"))
        {
            CameraController.Instance.HorShake(1);
        }
    }

    private void CandleCheck()
    {
        foreach (Candle candle in candles)
        {
            if (candle.HasBeenLit())
            {
                numberOfLitCandles++;
            }
        }
        if(numberOfLitCandles == candles.Length)
        {
            //this.gameObject.SetActive(false);

            animator.speed = .25F;
            canUseDoor = true;
        }
        else
        {
            numberOfLitCandles = 0;
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
