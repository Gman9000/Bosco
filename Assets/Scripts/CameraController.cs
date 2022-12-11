using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
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
        //halfRect = new Rect(0, 0, followRect.width / 2.0F, followRect.height / 2.0F);
        theRB = GetComponent<Rigidbody2D>();
        moveTo = Vector2.zero;
        followRect = new Rect(Vector2.zero, followRectSize);
        hud = GetComponentInChildren<HUD>();
    }

    void Update()
    {
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
        Vector2 playerPos = (Vector2)Player.Instance.transform.position + offset;

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
    }

    public void HorShake(int pixels)
    {
        if (shakeCoroutine == null)
            shakeCoroutine = StartCoroutine(ShakeSide(pixels));
    }

    IEnumerator ShakeSide(int pixels)
    {
        CalcMovePos();
        
        Vector3 playerRootPos = Player.Instance.transform.position;
        Vector3 camRootPos = (Vector3)moveTo + Vector3.back * 10;;

        for (int i = pixels; i > 0; i--)
        {
            yield return new WaitForFixedUpdate();
            Vector3 rightMove = Vector3.right * i * (i % 2 == 0 ? 1 : -1) * Game.PIXEL;
            Player.Instance.transform.position = playerRootPos;
            Player.Instance.transform.position += rightMove;
            moveTo = camRootPos;
            moveTo += (Vector2) rightMove;
            transform.position = (Vector3)moveTo + Vector3.back * 10;

            
        }
        yield return new WaitForFixedUpdate();

        CalcMovePos();
        Player.Instance.transform.position = playerRootPos;

        shakeCoroutine = null;
        yield break;
    }
}
