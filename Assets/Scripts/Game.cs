using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    /*=============*\
    |*--CONSTANTS--*|
    \*=============*/
    
    public const float PIXEL = 1.0F / 16.0F;    
    public const float WIDTH = 256.0F;
    public const float HEIGHT = 144.0F;
    public const float WIDTH_WORLD = WIDTH * PIXEL;
    public const float HEIGHT_WORLD = HEIGHT * PIXEL;


    /*=======================*\
    |*--INSPECTOR VARIABLES--*|
    \*=======================*/

    public bool doTitle = false;

    /*=================*\
    |*--STATIC FIELDS--*|
    \*=================*/
    public static Game Instance;
    public static float gameTime = 0.0F;    // the time counter used for game logic and custom movement functions
    public static int litCandlesCount = 0;
    public static int lives = 1;
    public static bool isPaused;
    public static bool gameStarted;
    public static List<SpriteSimulator> simulatedSprites;    
    private static List<SpriteSimulator>[] scanlines;
    private static int[] scanlineSimTotal;


    /*================*\
    |*--LOCAL FIELDS--*|
    \*================*/

    bool transitiontoGame;

    void Awake()
    {        
        Instance = this;
        simulatedSprites = new List<SpriteSimulator>();
        transitiontoGame = false;
    }

    void Start()
    {
        Awake();
        Timer.AllTimersInit();

        if (doTitle)
        {
            gameStarted = false;
            ShowTitle();
            transitiontoGame = false;
        }
        else
        {
            gameStarted = true;
            HUD.Instance.renderers["Bosco Sprite"].gameObject.SetActive(false);
            HideTitle();
        }

        isPaused = false;
        scanlines = new List<SpriteSimulator>[(int)Mathf.RoundToInt(HEIGHT / 16.0F)];
        scanlineSimTotal = new int[scanlines.Length];
        for (int i = 0; i < scanlines.Length; i++)
            scanlines[i] = new List<SpriteSimulator>();
    }

    static public void Reset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

#if !UNITY_EDITOR
    void OnApplicationFocus(bool focused)
    {
        if (!focused && gameStarted)
            Pause();
    }
#endif
    public void ShowTitle()
    {
        if (HUD.Instance)
        {
            HUD.Instance.renderers["Title"].enabled = true;
            HUD.Write("\n\n\n\n\n\n\n     PRESS START!");
        }
    }

    public void HideTitle()
    {
        if (HUD.Instance)
        {
            HUD.Instance.renderers["Title"].enabled = false;
            HUD.Instance.texts["Credits"].gameObject.SetActive(false);
            HUD.Instance.renderers["Bosco Sprite"].enabled = false;
            HUD.Write(null);
        }
    }

    static public void Pause()
    {
        if (isPaused)   return;
        isPaused = true;
        foreach (SpriteSimulator sim in simulatedSprites)
        {
            sim.Flash(false);
        }

        Time.timeScale = 0;        
        HUD.Write("\n\n\n\n\n      -paused-\n\n (Press R to reset)");
        SoundSystem.Pause();
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
        SoundSystem.Unpause();
    }

    IEnumerator GameGo()
    {
        if (HUD.Instance)
        {
            HUD.Instance.texts["Credits"].gameObject.SetActive(false);
            HUD.Instance.texts["Main Text Layer"].text = "\n\n\n\n\n\n\n      LET'S GO!!";
        

            for (int i = 0; i < 16; i++)
            {
                HUD.Instance.texts["Main Text Layer"].enabled = !HUD.Instance.texts["Main Text Layer"].enabled;
                yield return new WaitForSeconds(.044F);
            }

            HUD.Instance.texts["Main Text Layer"].enabled = false;
        }

        yield return new WaitForSeconds(.15F);


        if (HUD.Instance)
        {
            HUD.Instance.renderers["Bosco Sprite"].GetComponentInChildren<SpriteAnimator>().Play(AnimMode.Looped, "run");
            while (Mathf.Abs(HUD.Instance.renderers["Bosco Sprite"].transform.localPosition.x) < WIDTH * PIXEL * .75F)
            {
                HUD.Instance.renderers["Bosco Sprite"].transform.position += Vector3.right * PIXEL * 4;
                yield return new WaitForFixedUpdate();
            }

            if (HUD.Instance)
                HUD.Instance.renderers["Bosco Sprite"].gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(1);

        gameStarted = true;
        SoundSystem.PlayBgm(SoundSystem.Instance.defaultSong, SoundSystem.Instance.defaultSongLoopPoint, true);
        HideTitle();
        yield break;
    }

    void FixedUpdate()
    {
        if (Player.Position.x > 100)
        {
            SoundSystem.DoFade(.01F);
        }
    }

    void Update()
    {
        Timer.UpdateAll();  // update all timer checks


        if (!gameStarted && !transitiontoGame)
        {
            if (PlayerInput.HasPressedStart())
            {
                StartCoroutine(GameGo());   
                transitiontoGame = true;             
            }
            if (HUD.Instance)
                HUD.Instance.texts["Main Text Layer"].enabled = Time.time % 1F > .5F;

            string[] credit = new[]{
                "2022 (c) Idea Guy Interactive",
                "Audio & Design by Scotty Rich",
                " Coded by Granville Jones Jr.",
                "  Background Art by Emily Yi",
                " Sprites by Daniel Hernandez",
            };

            if (HUD.Instance)
                HUD.Instance.texts["Credits"].text = credit[(int)(Time.time / 3) % 5];

            return;
        }

        if (isPaused)
        {            
            if (PlayerInput.HasPressedSelect() )
            {
                Player.Instance.ResetToLastCheckPoint();
                Unpause();
            }
            return;
        }

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

    static public bool IsPointOnScreen(Vector2 point, float widthMargin, bool ignoreUp = false)
    {
        float x = point.x - Camera.main.transform.position.x;        
        float y = point.y - Camera.main.transform.position.y + scanlines.Length / 2.0F;

        int scanlineIndex = Mathf.FloorToInt(y / (16.0F * PIXEL));
        
        return scanlineIndex >= -1 && (scanlineIndex <= scanlines.Length || ignoreUp) && Mathf.Abs(x) < WIDTH * PIXEL / 2.0 + widthMargin;
    }


    static public Vector2 RestrictDiagonals(Vector2 direction, float subdivision = 2)
    {
        Vector2 modifiedDirection = direction;
        modifiedDirection = (modifiedDirection).normalized * subdivision;
        modifiedDirection.x = Mathf.Round(modifiedDirection.x);
        modifiedDirection.y = Mathf.Round(modifiedDirection.y);
        modifiedDirection /= subdivision;
        return modifiedDirection;
    }
}