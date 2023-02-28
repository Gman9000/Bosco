using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    static public Rect viewRect;
    static public CameraController Instance;
    private Rigidbody2D theRB;

    public float smoothDampTime;

    public Vector2 followRectSize = new Vector2(11, 10);

    public Vector2 offset = Vector2.up * 2;
    private Rect followRect;
    // Start is called before the first frame update

    HUD hud;


    Coroutine shakeCoroutine = null;
    Vector2 moveTo;

    void Awake()
    {
        Instance = this;
    }


    void Start()
    {
        theRB = GetComponent<Rigidbody2D>();
        moveTo = Vector2.zero;
        followRect = new Rect(Vector2.zero, followRectSize);
        hud = GetComponentInChildren<HUD>();
    }

    void Update()
    {
        Vector2 point = Camera.main.ViewportToWorldPoint(Vector3.zero);
        viewRect = new Rect(point.x, point.y, Game.WIDTH_WORLD * 1.15F, Game.HEIGHT_WORLD * 1.15F);

        if (!Game.gameStarted)  return;
        
        if (shakeCoroutine == null)
        {
            CalcMovePos();
            if (transform.position.x != moveTo.x || transform.position.y != moveTo.y)
            {
                transform.position = (Vector3)moveTo + Vector3.back * 10;
            }
        }
    }

    void PreRender()
    {
        hud.transform.localPosition = Vector3.forward;
    }


    private void CalcMovePos()
    {
        Vector2 playerPos = (Vector2)Player.Position + offset;

        followRect.center = transform.position;

        if (playerPos.x > followRect.xMax)
        {
            moveTo += Vector2.right * (playerPos.x - followRect.xMax);
        }
        else if (playerPos.x < followRect.xMin)
        {
            moveTo += Vector2.right * (playerPos.x - followRect.xMin);
        }

        if (playerPos.y > followRect.yMax)
        {
            moveTo += Vector2.up * (playerPos.y - followRect.yMax);
        }
        else if (playerPos.y < followRect.yMin)
        {
            moveTo += Vector2.up * (playerPos.y - followRect.yMin);
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

        Transform playerSprite = Player.Instance.animator.Renderer.transform;
        
        Vector3 playerRootPos = playerSprite.localPosition;

        for (int i = pixels; i > 0; i--)
        {
            CalcMovePos();
            yield return new WaitForSecondsRealtime(Game.TICK_TIME);
            yield return new WaitUntil(() => !Game.isPaused);
            Vector3 rightMove = Vector3.right * i * (i % 2 == 0 ? 1 : -1) * Game.PIXEL;
            playerSprite.localPosition = playerRootPos;
            playerSprite.localPosition += rightMove;
            transform.position = (Vector3)moveTo + Vector3.back * 10;
            transform.position += rightMove;            
        }
        yield return new WaitForEndOfFrame();

        CalcMovePos();
        playerSprite.localPosition = playerRootPos;
        transform.position = (Vector3)moveTo + Vector3.back * 10;

        shakeCoroutine = null;
        yield break;
    }
    IEnumerator ShakeUp(int pixels)
    {

        Transform playerSprite = Player.Instance.animator.Renderer.transform;
        
        Vector3 playerRootPos = playerSprite.localPosition;

        for (int i = pixels; i > 0; i--)
        {
            CalcMovePos();
            yield return new WaitForSecondsRealtime(Game.TICK_TIME);
            yield return new WaitUntil(() => !Game.isPaused);
            Vector3 upMove = Vector3.up * i * (i % 2 == 0 ? 1 : -1) * Game.PIXEL;
            playerSprite.localPosition = playerRootPos;
            playerSprite.localPosition += upMove;
            transform.position = (Vector3)moveTo + Vector3.back * 10;
            transform.position += upMove;

            
        }
        yield return new WaitForEndOfFrame();

        CalcMovePos();
        playerSprite.localPosition = playerRootPos;
        transform.position = (Vector3)moveTo + Vector3.back * 10;

        shakeCoroutine = null;
        yield break;
    }
}
