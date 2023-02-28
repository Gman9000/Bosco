using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    static public List<Timer> allTimers;
    static public List<Timer> allRealtimeTimers;

    public float timeStamp;                 // the game-time at which
    protected System.Action onComplete;     // the function to call when the timer is done
    private float duration;                 // length of time in seconds before Timer completes
    protected bool _done;
    public bool done => _done;
    public bool active => !done && progress != 0;
    public float progress => Mathf.Min((Time.time - timeStamp) / duration, 1.0F);

    
    private Timer(float duration, System.Action onComplete, bool realtime)
    {
        this.duration = duration;
        this.onComplete = onComplete;
        if (realtime)
            timeStamp = Game.unpausedRealtime;
        else
            timeStamp = Time.time;
        _done = false;
    }

    static public Timer Set(float duration, System.Action onComplete)
    {
        Timer timer = new Timer(duration, onComplete, false);
        allTimers.Add(timer);
        return timer;
    }

    static public Timer Set(float duration) => Set(duration, () => {});

    static public Timer SetRealtime(float duration, System.Action onComplete)
    {
        Timer timer = new Timer(duration, onComplete, true);
        allRealtimeTimers.Add(timer);
        Game.debugText = "FREEZE FRAME!";
        return timer;

    }

    static public void AllTimersInit()
    {
        allTimers = new List<Timer>();
        allRealtimeTimers = new List<Timer>();
    }

    static public void Update()
    {
        for (int i = allTimers.Count - 1; i >= 0; i--)
        {
            Timer timer = allTimers[i];
            if (Time.time - timer.timeStamp + Time.deltaTime > timer.duration)
            {
                timer.onComplete();
                timer._done = true;
                allTimers.Remove(timer);
            }
        }

        if (!Game.isPaused)
            for (int i = allRealtimeTimers.Count - 1; i >= 0; i--)
            {
                Timer timer = allRealtimeTimers[i];
                if (Game.unpausedRealtime - timer.timeStamp + Time.unscaledDeltaTime > timer.duration)
                {
                    timer.onComplete();
                    timer._done = true;
                    allRealtimeTimers.Remove(timer);
                    Game.debugText = "";
                    Debug.Log("sound");
                }
            }
    }

    public void Cancel()
    {
        _done = true;
        allTimers.Remove(this);
    }
}
