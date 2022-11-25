using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Candle : MonoBehaviour
{
    private bool isLit;
    // Start is called before the first frame update
    void Start()
    {
        isLit = false;
    }
    public bool HasBeenLighted()
    {
        return isLit;
    }
    public void LightUpCandle()
    {
        isLit = true;
    }
}
