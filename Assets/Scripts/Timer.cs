using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    static public List<Timer> allTimers;

    public float timeStamp;                 // the game-time at which
    protected System.Action onComplete;     // the function to call when the timer is done
    private float duration;                 // length of time in seconds before Timer completes
    
    protected bool _done;
    public bool Done => _done;
    public float Progress => Mathf.Min((Time.time - timeStamp) / duration, 1.0F);

    
    public Timer(float duration, System.Action onComplete)
    {
        this.duration = duration;
        this.onComplete = onComplete;
        timeStamp = Time.time;
        _done = false;
    }

    static public Timer Set(float duration, System.Action onComplete)
    {
        Timer ret = new Timer(duration, onComplete);
        allTimers.Add(ret);
        return ret;
    }

    static public void AllTimersInit()
    {
        allTimers = new List<Timer>();
    }

    static public void UpdateAll()
    {
        for (int i = allTimers.Count - 1; i >= 0; i--)
        {
            Timer timer = allTimers[i];
            if (Time.time - timer.timeStamp >= timer.duration)
            {
                timer.onComplete();
                timer._done = true;
                allTimers.Remove(timer);
            }
        }
    }

    public void Cancel()
    {
        _done = true;
        allTimers.Remove(this);
    }
}
