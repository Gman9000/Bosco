using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleHandler : MonoBehaviour
{
    public Candle[] candles;
    private int numberOfLitCandles;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void CandleCheck()
    {
        foreach (Candle candle in candles)
        {
            if (candle.HasBeenLighted())
            {
                numberOfLitCandles++;
            }
        }
        if(numberOfLitCandles == candles.Length)
        {
            this.gameObject.SetActive(false);
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
