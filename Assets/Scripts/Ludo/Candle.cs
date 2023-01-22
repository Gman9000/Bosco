using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Candle : MonoBehaviour
{
    private bool isLit;
    GameObject flameObj = null;

    LightSource lightSrc;

    void Awake()
    {
        lightSrc = GetComponent<LightSource>();
    }

    void Start()
    {
        isLit = false;
        flameObj = GetComponentInChildren<Animator>().gameObject;
        if (flameObj)
            flameObj.SetActive(false);
    }
    public bool HasBeenLit()
    {
        return isLit;
    }
    public void LightUpCandle()
    {
        if (isLit)  return;
        Game.litCandlesCount++;
        isLit = true;
        lightSrc.Light(true);
        if (flameObj)
            flameObj.SetActive(true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            LightUpCandle();
        }
    }
}