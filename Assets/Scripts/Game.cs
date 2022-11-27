using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public const float PIXEL = 1.0F / 16.0F;
    public static float gameTime = 0.0F;    // the time counter used for game logic and custom movement functions
    void Update()
    {
        gameTime += Time.deltaTime * 10.0F;
        Debug.Log(gameTime);
    }


    static public float PingPong(float time)
    {
        if (time % 1.0F >= .4F)
            return 0;
        if (time % 2.0F < 1.0F)
            return 1;
        else
            return -1;
    }
}
