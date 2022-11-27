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
        Destroy(this.gameObject, 10);
    }

    public void SetDirectionAndVelocity(Vector3 playerDirection)
    {
        dirToPlayer = playerDirection;
        //Debug.Log(dirToPlayer);
        theRB.velocity = dirToPlayer * speed;
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Collider2D>().CompareTag("Player"))
        {
            Player.Instance.TakeDamage();
            Debug.Log("We have damaged the player");
            //collision.gameObject.GetComponent<PlayerMovement>().DamagePlayer();
        }

        Destroy(this.gameObject);
    }

    public void AssignTarget(GameObject target)
    {

        targetLocation = target;

    }
}
