using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Rigidbody2D theRB;
    public float smoothDampTime;
    // Start is called before the first frame update
    void Start()
    {
        theRB = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.Instance != null)
        {
            //Debug.Log("PLAYER EXISTS!");

            Vector2 currentVelocity = theRB.velocity;

            //double pixelSize = 1.0 / 16.0;
            
            theRB.position = Vector2.SmoothDamp(theRB.position, Player.Instance.gameObject.transform.position, ref currentVelocity, smoothDampTime, Mathf.Infinity, Time.deltaTime);


            // this snaps the camera to be pixel perfect and not make a super janky camera
            Vector2 rbPos = theRB.position;
            rbPos.x = (float)(System.Math.Floor((double)rbPos.x * 16.0) / 16.0);
            rbPos.y = (float)(System.Math.Floor((double)rbPos.y * 16.0) / 16.0);
            theRB.position = rbPos;
            //Debug.Log("POSITION: " + theRB.position);
        }
    }
}
