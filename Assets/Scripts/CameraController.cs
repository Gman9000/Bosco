using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Rigidbody2D theRB;

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
            theRB.position = Vector2.SmoothDamp(theRB.position, Player.Instance.gameObject.transform.position, ref currentVelocity, 0.015f, Mathf.Infinity, Time.deltaTime);
            //Debug.Log("POSITION: " + theRB.position);
        }
    }
}
