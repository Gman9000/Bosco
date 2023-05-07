using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    /*=============*\
    |*  CONSTANTS  *|
    \*=============*/
    
    public const float PIXEL = 1.0F / 16.0F;    
    public const float WIDTH = 256.0F;
    public const float HEIGHT = 144.0F;
    public const float WIDTH_WORLD = WIDTH * PIXEL;
    public const float HEIGHT_WORLD = HEIGHT * PIXEL;

    public const float TICK_TIME = .0167F;
    public const float FRAME_TIME = .022F;

    private int fixedUpdateCounter = 0;


    /*=======================*\
    |*  INSPECTOR VARIABLES  *|
    \*=======================*/


    /*=================*\
    |*  STATIC FIELDS  *|
    \*=================*/
    public static Game Instance;
    
    public static float relativeTime => Time.deltaTime * 500;    // the time counter used for game logic and custom movement functions
    public static int litCandlesCount;
    public static int lives;
    public static bool isPaused;
    private static float _unpausedRealtime;
    public static float unpausedRealtime => _unpausedRealtime;
    public static bool gameStarted;
    public static List<SpriteSimulator> simulatedSprites;    
    private static List<SpriteSimulator>[] scanlines;
    private static int[] scanlineSimTotal;

    private static bool _isFreezeFraming;
    public static bool isFreezeFraming => _isFreezeFraming;

    public static object debugText;


    /*================*\
    |*  LOCAL FIELDS  *|
    \*================*/

    bool transitiontoGame;

    void Awake()
    {        
        Instance = this;
        simulatedSprites = new List<SpriteSimulator>();
        transitiontoGame = false;
        Pawn.Instances = new Dictionary<string, Pawn>();
        Timer.AllTimersInit();
        PlayerInput.Init();
    }

    void Start()
    {       
        Awake();
        litCandlesCount = 0;
        lives = 3;
        _unpausedRealtime = 0;
        Timer.AllTimersInit();
        Pawn.skitMode = false;

        gameStarted = true;

        isPaused = false;
        _isFreezeFraming = false;
        scanlines = new List<SpriteSimulator>[(int)Mathf.RoundToInt(HEIGHT / 16.0F)];
        scanlineSimTotal = new int[scanlines.Length];
        for (int i = 0; i < scanlines.Length; i++)
            scanlines[i] = new List<SpriteSimulator>();

        Time.timeScale = 1;
        SkitRunner.Init();
    }

    public static void Reset()
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
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 108;
        style.normal.textColor = Color.green;
        GUI.Label(new Rect(0, 0, 200, 100), "" + debugText, style);
    }

    public static void Pause()
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

    public static void Unpause()
    {
        if (!isPaused)   return;
        isPaused = false;
        foreach (SpriteSimulator sim in simulatedSprites)
            sim.Flash(true);

        Time.timeScale = 1.0F;
        HUD.Write(null);
        SoundSystem.Unpause();

        // temp
        SkitRunner.active = false;
    }

    public static void FreezeFrame(float secondsToWait, System.Action onResume)
    {
        if (isFreezeFraming)    return;
        float oldTimescale = Time.timeScale;
        _isFreezeFraming = true;
        Time.timeScale = 0;
        Timer.SetRealtime(secondsToWait, () => {
            Time.timeScale = oldTimescale;
            _isFreezeFraming = false;
            onResume();
        });
    }

    public static void FreezeFrame(float secondsToWait)
    {
        FreezeFrame(secondsToWait, () => {});
    }

    IEnumerator GameGo()
    {
        if (HUD.Instance)
        {
            for (int i = 0; i < 16; i++)
            {
                yield return new WaitForSeconds(.044F);
            }           
        }

        yield return new WaitForSeconds(1);

        gameStarted = true;
        SoundSystem.PlayBgm(SoundSystem.Instance.defaultSong, SoundSystem.Instance.defaultSongLoopPoint, true);
        yield break;
    }

    void FixedUpdate()
    {
        if (Player.Position.x > 100)
        {
            SoundSystem.DoFade(.01F);
        }

        // scanline logic
        for (int i = 0; i < scanlines.Length; i++)
            scanlines[i].Clear();
        for (int i = 0; i < scanlineSimTotal.Length; i++)
            scanlineSimTotal[i] = 0;        
        foreach (SpriteSimulator sim in simulatedSprites)
        {
            float x = sim.SpritePos.x - Camera.main.transform.position.x;
            float y = sim.SpritePos.y - Camera.main.transform.position.y + (HEIGHT * PIXEL) / 2.0F + 1;

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


        // simulate sprite flicker
        if (Time.timeScale > 0)
        {
            Time.timeScale = 1;
            for (int y = 0; y < scanlines.Length; y++)
            {
                if (scanlineSimTotal[y] <= SpriteSimulator.SCANLINE_LIMIT || !SpriteSimulator.flashOnLimit)
                {
                    foreach (SpriteSimulator sim in scanlines[y])
                        sim.Flash(true);
                }
                else
                {
                    for (int x = 0; x < scanlines[y].Count; x++)
                    {
                        scanlines[y][x].Flash(fixedUpdateCounter % 2 != x % 2);
                        Time.timeScale *= .86F;
                    }
                }
            }
        }


        fixedUpdateCounter++;
    }

    void Update()
    {
        if (!isPaused)
            _unpausedRealtime += Time.unscaledDeltaTime;
        Timer.Update();  // update all timer checks
        PlayerInput.Update();

        if (PlayerInput.Pressed(Button.Start))
        {
            if (Game.isPaused)
                Game.Unpause();
            else
                Game.Pause();
        }

        if (isPaused)
        {            
            if (PlayerInput.Pressed(Button.Select) )
            {
                Player.Instance.ResetToLastCheckPoint();
                Unpause();
            }
            return;
        }
    }

    public static void RemoveSimulatedSprite(SpriteSimulator sim)
    {
        simulatedSprites.Remove(sim);
        foreach (List<SpriteSimulator> scanline in scanlines)
            scanline.Remove(sim);
    }

    public static void HorShake(int pixels) => CameraController.Instance.HorShake(pixels);
    public static void VertShake(int pixels) => CameraController.Instance.VertShake(pixels);

    public static float PingPong(float time)
    {
        if (time % 1.0F >= .4F)
            return 0;
        if (time % 2.0F < 1.0F)
            return 1;
        else
            return -1;
    }

    public static bool IsPointOnScreen(Vector2 point, float widthMargin, bool ignoreUp = false)
    {
        float x = point.x - Camera.main.transform.position.x;        
        float y = point.y - Camera.main.transform.position.y + scanlines.Length / 2.0F;

        int scanlineIndex = Mathf.FloorToInt(y / (16.0F * PIXEL));
        
        return scanlineIndex >= -1 && (scanlineIndex <= scanlines.Length || ignoreUp) && Mathf.Abs(x) < WIDTH * PIXEL / 2.0 + widthMargin;
    }


    public static Vector2 RestrictDiagonals(Vector2 direction, float subdivision = 2)
    {
        Vector2 modifiedDirection = direction;
        modifiedDirection = (modifiedDirection).normalized * subdivision;
        modifiedDirection.x = Mathf.Round(modifiedDirection.x);
        modifiedDirection.y = Mathf.Round(modifiedDirection.y);
        modifiedDirection /= subdivision;
        return modifiedDirection;
    }
}