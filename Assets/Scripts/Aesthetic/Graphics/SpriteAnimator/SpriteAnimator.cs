using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimMode {None, Once, OnceDie, Looped, Hang}
public class SpriteAnimator : MonoBehaviour
{
    public bool autoAnim = false;
    public SpriteAnimation[] animations = new SpriteAnimation[0];
    public string defaultAnimation = "default";
    public Dictionary<string, SpriteAnimation> data = new Dictionary<string, SpriteAnimation>();
    
    [HideInInspector]public int indexOffset = 0;

    private Dictionary<string, Sprite[]> _frames = new Dictionary<string, Sprite[]>();
    private SpriteRenderer _renderer;
    public SpriteRenderer Renderer => _renderer;

    private Color startColor;
    public GetAnim getDefault;

    private GameObject _renObj;

    private List<string> _animQueue = new List<string>();

    private string _currentAnim;
    public string CurrentAnim => _currentAnim;

    private int _currentFrame;
    public int CurrentFrame => _currentFrame;

    public int CurrentFrameCount => _frames[CurrentAnim].Length;

    public bool AtEnd => CurrentFrame >= CurrentFrameCount - 1;

    public System.Action onEnd = () => {};

    private AnimMode _currentMode;

    private Coroutine _animatorCoroutine;

    void Awake()
    {
        _renderer = GetComponentInChildren<SpriteRenderer>();
        _renObj = _renderer.gameObject;

        Reset();


        foreach (SpriteAnimation data in animations)
        {
            Vector2Int cel = data.celSize;
            int count = data.sheet.width / cel.x;
            int start = 0;
            if (data.endFrame >= 0 && data.endFrame >= data.startFrame && data.endFrame < count)
            {
                start = data.startFrame;
                count = data.endFrame - start + 1;
            }
            Sprite[] frames = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                Vector2 pivot = Vector2.zero;
                pivot.x += _renderer.sprite.pivot.x / data.celSize.x;
                pivot.y += _renderer.sprite.pivot.y / data.celSize.y;
                
                frames[i] = Sprite.Create(data.sheet, new Rect((i + start) * cel.x, 0, cel.x, cel.y), pivot, 16, 0, SpriteMeshType.FullRect, Vector4.zero, false);
            }
            this._frames.Add(data.name, frames);
            this.data.Add(data.name, data);
        }

        if (autoAnim)
            Play(AnimMode.Looped, defaultAnimation);

        getDefault = () => defaultAnimation;
        startColor = Renderer.color;
    }

    public void Reset()
    {
        StopAllCoroutines();
        _animatorCoroutine = null;
        _currentAnim = "none";
        _animQueue = new List<string>();
        _currentFrame = 0;
        _currentMode = AnimMode.None;
    }


    IEnumerator Animate(string animName, AnimMode mode)
    {
        if (_renObj.transform != this.transform)
        {
            _renObj.transform.localPosition = new Vector2(data[animName].offset.x * Game.PIXEL, data[animName].offset.y * Game.PIXEL);
        }

        do
        {
            for (int i = 0; i < _frames[animName].Length; i++)
            {
                int length = _frames[animName].Length;
                _renderer.sprite = _frames[animName][(i + indexOffset) % length];
                _currentFrame = (i + indexOffset) % length;
                if (data[animName].delay > 0)
                    yield return new WaitForSeconds(data[animName].delay);
                else
                    yield return new WaitForSeconds(5.0F);
                yield return null;
            }
            yield return null;
        }
        while (mode == AnimMode.Looped && _frames.Count > 1);

        if (mode != AnimMode.Hang)
        {
            onEnd.Invoke();
            onEnd = () => {};
            Resume(mode);
        }
        
        yield break;
    }

    public void PlayDefault() => Play(AnimMode.Looped, getDefault());

    public void FlipX(int dir)
    {
        Renderer.flipX = dir < 0 ? true : false;
    }
    
    public void Rotate(float degrees)
    {
        _renObj.transform.localRotation = Quaternion.Euler(0, 0, degrees);
    }

    public bool HasAnim(string animName)
    {
        return _frames.ContainsKey(animName);
    }

    public void Resume(AnimMode mode)
    {
        _currentFrame = 0;
        if (_animQueue.Count > 0)
        {
            Play(mode, _animQueue[0]);
            _animQueue.Remove(_animQueue[0]);
        }
        else if (mode == AnimMode.OnceDie)
            Destroy(gameObject);
        else
            PlayDefault();
    }

    public void Multiplay(AnimMode mode, params string[] animNames)
    {
        _animQueue = new List<string>(animNames);
        Resume(mode);
    }

    public void Play(AnimMode mode, string animName, System.Action onEnd = null)
    {
        if (!gameObject.activeSelf || !Renderer.gameObject.activeSelf)
            return;
        if (_currentAnim.ToLower() == animName.ToLower())
            return;
        if (onEnd == null)
            this.onEnd = () => {};
        else
            this.onEnd = onEnd;
        
        _renderer.enabled = true;
        if (_currentMode == AnimMode.OnceDie && mode != AnimMode.OnceDie)
            return;
        if (_animatorCoroutine != null)
            StopCoroutine(_animatorCoroutine);
        _currentMode = mode;
        _currentAnim = animName;
        _animatorCoroutine = StartCoroutine(Animate(animName, mode));
    }

    public void SetVisible(bool value)
    {
        if (value)
            Renderer.color = startColor;
        else
            Renderer.color = new Color(0,0,0,0);
    }

    public void ToggleVisible()
    {
        if (Renderer.color.a == 0)
            SetVisible(true);
        else
            SetVisible(false);
    }
}