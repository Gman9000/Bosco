using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum FollowType {None, Linear, Lerp, Fixed}
public class CameraController : MonoBehaviour
{
    public static Rect viewRect;
    public static CameraController Instance;
    public Vector2 followRectSize = new Vector2(11, 10);

    public Vector2 offset = Vector2.up * 2;
    private Rect followRect;    // logic needs updating
    
    [HideInInspector]public FollowType followType;

    HUD hud;


    Coroutine shakeCoroutine = null;
    Vector2 moveTo;

    [HideInInspector] public Transform followObject;


    private Coroutine followCoroutine;

    void Awake()
    {
        Instance = this;
    }


    void Start()
    {
        moveTo = Vector2.zero;
        followRect = new Rect(Vector2.zero, followRectSize);
        hud = GetComponentInChildren<HUD>();
        shakeCoroutine = StartCoroutine(ShakeUp(5));
        SetFollowPlayer();
        followCoroutine = null;
    }

    void Update()
    {
        Game.debugText = "camera " + transform.position.z;
        Vector2 point = Camera.main.ViewportToWorldPoint(Vector3.zero);
        viewRect = new Rect(point.x, point.y, Game.WIDTH_WORLD * 1.15F, Game.HEIGHT_WORLD * 1.15F);

        if (!Game.gameStarted)  return;
        
        if (shakeCoroutine == null && followType == FollowType.Fixed)
        {
            CalcMovePos();
            if (transform.position.x != moveTo.x || transform.position.y != moveTo.y)
            {
                transform.position = (Vector3)moveTo;
            }
        }

        transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    void PreRender()
    {
        hud.transform.localPosition = Vector3.forward;
    }

    public void SetTransformFollow(Transform t, FollowType followType, float speedInterval)
    {
        followObject = t;

        if (followCoroutine != null)
            StopCoroutine(followCoroutine);
        switch (followType)
        {
            case FollowType.Linear:
                followCoroutine = StartCoroutine(CameraLinear(t, speedInterval, true));
                break;
        }
        this.followType = followType;        
    }

    static public void SetFollowPlayer() => Instance.SetTransformFollow(Player.Instance.transform, FollowType.Fixed, 1);

    private void CalcMovePos()  // todo: redo
    {
        if (followType != FollowType.Fixed) return;

        Vector2 followPos = (Vector2)followObject.transform.position + offset;

        followRect.center = transform.position;

        if (followPos.x > followRect.xMax)
        {
            moveTo += Vector2.right * (followPos.x - followRect.xMax);
        }
        else if (followPos.x < followRect.xMin)
        {
            moveTo += Vector2.right * (followPos.x - followRect.xMin);
        }

        if (followPos.y > followRect.yMax)
        {
            moveTo += Vector2.up * (followPos.y - followRect.yMax);
        }
        else if (followPos.y < followRect.yMin)
        {
            moveTo += Vector2.up * (followPos.y - followRect.yMin);
        }

        moveTo.x = Mathf.Round(moveTo.x / Game.PIXEL) * Game.PIXEL;
        moveTo.y = Mathf.Round(moveTo.y / Game.PIXEL) * Game.PIXEL;
    }

    public void HorShake(int pixels)
    {
        if (shakeCoroutine == null)
            shakeCoroutine = StartCoroutine(ShakeSide(pixels));
    }

    public void VertShake(int pixels)
    {
        if (shakeCoroutine == null)
            shakeCoroutine = StartCoroutine(ShakeUp(pixels));
    }

    IEnumerator ShakeSide(int pixels)
    {
        for (int i = pixels; i > 0; i--)
        {
            CalcMovePos();
            yield return new WaitForSecondsRealtime(Game.TICK_TIME);
            yield return new WaitUntil(() => !Game.isPaused);
            Vector3 rightMove = Vector3.right * i * (i % 2 == 0 ? 1 : -1) * Game.PIXEL;
            transform.position = (Vector3)moveTo + Vector3.back * 10;
            transform.position += rightMove;    
            yield return null;
        }
        yield return new WaitForEndOfFrame();

        CalcMovePos();
        transform.position = (Vector3)moveTo + Vector3.back * 10;

        shakeCoroutine = null;
        yield break;
    }
    
    IEnumerator ShakeUp(int pixels)
    {
        for (int i = pixels; i > 0; i--)
        {
            CalcMovePos();
            yield return new WaitForSecondsRealtime(Game.TICK_TIME);
            yield return new WaitUntil(() => !Game.isPaused);
            Vector3 upMove = Vector3.up * i * (i % 2 == 0 ? 1 : -1) * Game.PIXEL;
            transform.position = (Vector3)moveTo + Vector3.back * 10;
            transform.position += upMove;
        }
        yield return new WaitForEndOfFrame();

        CalcMovePos();
        transform.position = (Vector3)moveTo + Vector3.back * 10;

        shakeCoroutine = null;
        yield break;
    }

    IEnumerator CameraLinear(Transform t, float speedInterval, bool endOnComplete)
    {
        Vector3 startPosition = transform.position;
        Vector3 difference;
        Vector3 targetPosition = t.position;

        targetPosition.z = startPosition.z;
        float progress = 0;
        while (progress < 1)
        {
            targetPosition = t.position;
            targetPosition.z = startPosition.z;
            difference = targetPosition - startPosition;
            transform.position = startPosition + difference * progress;
            progress += speedInterval;

            if (progress >= 1 && !endOnComplete)
            {
                progress = 0;
                yield return new WaitUntil(() => (new Vector2(transform.position.x, transform.position.y) - new Vector2(t.position.x, t.position.y)).magnitude > Game.PIXEL * 8);
            }
            else
                yield return new WaitForSeconds(Game.TICK_TIME);
        }

        transform.position = targetPosition;
        followCoroutine = null;
        yield break;
    }
}
