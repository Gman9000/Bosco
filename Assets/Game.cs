using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public float publicVariable = 0;
    public static Game Instance;
    public const float PIXEL = 1.0F / 16.0F;    
    public const float WIDTH = 160.0F;
    public const float HEIGHT = 144.0F;
    public static float gameTime = 0.0F;    // the time counter used for game logic and custom movement functions
    public static int litCandlesCount = 0;
    public static int lives = 3;
    public static bool isPaused;


    static public List<SpriteSimulator> simulatedSprites = new List<SpriteSimulator>();

    
    static private List<SpriteSimulator>[] scanlines;

    static private int[] scanlineSimTotal;


    void Awake()
    {        
        Instance = this;
    }

    void Start()
    {
        isPaused = false;
        scanlines = new List<SpriteSimulator>[(int)Mathf.RoundToInt(HEIGHT / 16.0F)];
        scanlineSimTotal = new int[scanlines.Length];
        for (int i = 0; i < scanlines.Length; i++)
            scanlines[i] = new List<SpriteSimulator>();
    }

    #if !UNITY_EDITOR
    void OnApplicationFocus(bool focused)
    {
        if (!focused)
            Pause();
    }
    #endif

    static public void Pause()
    {
        if (isPaused)   return;
        isPaused = true;
        foreach (SpriteSimulator sim in simulatedSprites)
        {
            sim.Flash(false);
        }

        Time.timeScale = 0;        
        HUD.Write("\n      -paused-");
    }

    static public void Unpause()
    {
        if (!isPaused)   return;
        isPaused = false;
        foreach (SpriteSimulator sim in simulatedSprites)
        {
            sim.Flash(true);
        }

        Time.timeScale = 1.0F;
        HUD.Write(null);
    }

    void Update()
    {
        if (isPaused)   return;

        gameTime += Time.deltaTime;

        for (int i = 0; i < scanlines.Length; i++)
            scanlines[i].Clear();


        for (int i = 0; i < scanlineSimTotal.Length; i++)
            scanlineSimTotal[i] = 0;
        
        foreach (SpriteSimulator sim in simulatedSprites)
        {
            float x = sim.SpritePos.x - Camera.main.transform.position.x;
            float y = sim.SpritePos.y - Camera.main.transform.position.y + (HEIGHT * PIXEL) / 2.0F;

            int scanlineIndex = Mathf.FloorToInt(y / (16.0F * PIXEL));

            if (scanlineIndex > 0 && scanlineIndex < scanlines.Length && Mathf.Abs(x) < WIDTH * PIXEL / 2.0F)
            {
                scanlineSimTotal[scanlineIndex] += sim.tilevalue;
                scanlines[scanlineIndex].Add(sim);
                sim.SetOutOfView(false);
            }
            else
            {
                sim.SetOutOfView(true);
            }
        }

        Time.timeScale = 1.0F;

        for (int y = 0; y < scanlines.Length; y++)
        {
            if (scanlineSimTotal[y] <= 10)
            {
                foreach (SpriteSimulator sim in scanlines[y])
                    sim.Flash(true);
            }
            else
            {
                for (int x = 0; x < scanlines[y].Count; x++)
                {
                    scanlines[y][x].Flash((Time.frameCount) % 2 != x % 2);
                    Time.timeScale *= .95F;
                }
            }
        }
    }

    static public void RemoveSimulatedSprite(SpriteSimulator sim)
    {
        simulatedSprites.Remove(sim);
        foreach (List<SpriteSimulator> scanline in scanlines)
            scanline.Remove(sim);
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

    static public bool IsPointOnScreen(Vector2 point, float widthMargin)
    {
        float x = point.x - Camera.main.transform.position.x;        
        float y = point.y - Camera.main.transform.position.y + scanlines.Length / 2.0F;

        int scanlineIndex = Mathf.FloorToInt(y / (16.0F * PIXEL));
        return scanlineIndex >= -1 && scanlineIndex <= scanlines.Length && Mathf.Abs(x) < WIDTH * PIXEL / 2.0 + widthMargin;
    }
}
