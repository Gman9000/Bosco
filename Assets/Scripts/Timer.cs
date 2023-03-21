using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    public static List<Timer> allTimers;
    public static List<Timer> allRealtimeTimers;

    public float timeStamp;                 // the game-time at which
    protected System.Action onComplete;     // the function to call when the timer is done
    private float duration;                 // length of time in seconds before Timer completes
    protected bool _done;
    public bool done => _done;
    public bool active => !done && progress != 0;
    public float progress => Mathf.Min(elapsed / duration, 1.0F);
    
    public float elapsed => Time.time - timeStamp;

    
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

    public static Timer Set(float duration, System.Action onComplete)
    {
        Timer timer = new Timer(duration, onComplete, false);
        allTimers.Add(timer);
        return timer;
    }

    public static Timer Set(float duration) => Set(duration, () => {});

    public static Timer SetRealtime(float duration, System.Action onComplete)
    {
        Timer timer = new Timer(duration, onComplete, true);
        allRealtimeTimers.Add(timer);
        return timer;

    }

    public static void AllTimersInit()
    {
        allTimers = new List<Timer>();
        allRealtimeTimers = new List<Timer>();
    }

    public static void Update()
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
                }
            }
    }

    public void Cancel()
    {
        _done = true;
        allTimers.Remove(this);
    }

    public static implicit operator bool(Timer a)
    {
        return a != null && a.active;
    }

    override public bool Equals(object o)
    {
        return base.Equals(o);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
