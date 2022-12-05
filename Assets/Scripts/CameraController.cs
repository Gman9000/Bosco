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
        else
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = StartCoroutine(ShakeSide(-pixels));
        }
    }

    IEnumerator ShakeSide(int pixels)
    {
        CalcMovePos();
        Vector3 playerRootPos = Player.Instance.transform.position;
        Vector3 camRootPos = (Vector3)moveTo + Vector3.back * 10;;

        for (int i = pixels; i > 0; i--)
        {
            Vector3 rightMove = Vector3.right * i * (i % 2 == 0 ? 1 : -1) * Game.PIXEL;
            Player.Instance.transform.position = playerRootPos;
            Player.Instance.transform.position += rightMove;
            moveTo = camRootPos;
            moveTo += (Vector2) rightMove;
            transform.position = (Vector3)moveTo + Vector3.back * 10;;

            yield return new WaitForFixedUpdate();
        }

        CalcMovePos();
        Player.Instance.transform.position = playerRootPos;

        shakeCoroutine = null;
        yield break;
    }

    /*void Update()
    {
        if (Player.Instance != null)
        {
            //Debug.Log("PLAYER EXISTS!");

            Vector2 currentVelocity = theRB.velocity;

            //double pixelSize = 1.0 / 16.0;
            
            theRB.position = Vector2.SmoothDamp(theRB.position, Player.Instance.gameObject.transform.position + Vector3.up * 2, ref currentVelocity, smoothDampTime, Mathf.Infinity, Time.deltaTime);


            // this snaps the camera to be pixel perfect and not make a super janky camera
            Vector2 rbPos = theRB.position;
            rbPos.x = (float)(System.Math.Floor((double)rbPos.x * 16.0) / 16.0);
            rbPos.y = (float)(System.Math.Floor((double)rbPos.y * 16.0) / 16.0);
            theRB.position = rbPos;
            //Debug.Log("POSITION: " + theRB.position);
        }
    }*/
}
