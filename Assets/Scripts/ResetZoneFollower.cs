using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetZoneFollower : MonoBehaviour
{
    private Rigidbody2D theRB;

    // Start is called before the first frame update
    void Awake()
    {
        theRB = GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 currentVelocity = theRB.velocity;
        theRB.position = Vector2.SmoothDamp(theRB.position, new Vector2(Player.Instance.gameObject.transform.position.x, -10f), ref currentVelocity, 0.025f, Mathf.Infinity, Time.deltaTime);
    }
}
