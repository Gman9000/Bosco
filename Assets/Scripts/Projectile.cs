using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private GameObject targetLocation;
    public float speed;
    private Vector3 dirToPlayer;
    private Rigidbody2D theRB;

    private void Awake()
    {
        theRB = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!Game.IsPointOnScreen(transform.position, 2, true))
        {
            Destroy(gameObject);
        }
    }

    public void SetDirectionAndVelocity(Vector3 playerDirection)
    {
        dirToPlayer = playerDirection;
        //Debug.Log(dirToPlayer);
        theRB.velocity = dirToPlayer * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player.Instance.TakeDamage();
            Destroy(this.gameObject);
        }
    }
    
    public void AssignTarget(GameObject target)
    {

        targetLocation = target;

    }
}
