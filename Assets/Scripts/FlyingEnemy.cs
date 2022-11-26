using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    //[SerializeField] private Rigidbody2D rigidBody;      //enemy rigidBody
    private GameObject playerTarget;    //player character
    //[SerializeField] private GameObject waypoint01;      //first waypoint
    //[SerializeField] private GameObject waypoint02;      //second waypoint
    //[SerializeField] private GameObject firingPosition;  //position that projectiles are created.
    [SerializeField] private GameObject projectilePrefab;      //bullet prefab
    private GameObject currentWaypoint;
    public float speed;                 //enemy speed
    public float followDistance;              //required distance between player and enemy before enemy charges
    public float enemyWidth;            //width of enemy, needed for when enemy turns around
    public int enemyHealth;             //health of enemy unit
    private bool hitState = false;      //is the enemy's invincibility frames currently active
    private bool isShooting = false;    //is the enemy currently shooting.
    public float fireRate;              //the amount of time between each of the enemy's shots.
    public Animator flyingAnim;
    public float deathTimer;
    //bool isDetectingPlayer;
    //private CircleCollider2D CircleCollider2D;
    //private Rigidbody2D myRB;

    private void Awake()
    {
        playerTarget = GameObject.FindWithTag("Player");
        //myRB = GetComponent<Rigidbody2D>();
        //CircleCollider2D = GetComponent<CircleCollider2D>();
    }

    void Start()
    {
        //currentWaypoint = waypoint01;   //set the first waypoint.
    }

    void Update()
    {
        //isDetectingPlayer = CircleCollider2D.IsDetectingThePlayer(this.transform.position, this.transform.localScale.x, distance);
        if ((Vector2.Distance(transform.position, playerTarget.transform.position) <= followDistance))
        {
            Vector3 directionOfTravel = new Vector3 (playerTarget.transform.position.x,0f,0f) - new Vector3 (transform.position.x,0f,0f);
            directionOfTravel.Normalize();
            Vector3 enemyDirection = transform.localScale;

            if (transform.position.x < playerTarget.transform.position.x)
            {

                enemyDirection.x = enemyWidth;
            }

            else if (transform.position.x > playerTarget.transform.position.x)
            {

                enemyDirection.x = -enemyWidth;
            }
            transform.localScale = enemyDirection;

            if (this.transform.position.x <= (0.99 * playerTarget.transform.position.x) || this.transform.position.x >= (1.01 * playerTarget.transform.position.x))
            {
                this.transform.Translate(
                    directionOfTravel.x * speed * Time.deltaTime,
                    directionOfTravel.y * speed * Time.deltaTime,
                    directionOfTravel.z * speed * Time.deltaTime,
                    Space.World);
            }
        }
        /*if (Vector2.Distance(transform.position, currentWaypoint.transform.position) > 1f)
        {
            Vector3 directionOfTravel = currentWaypoint.transform.position - transform.position;
            directionOfTravel.Normalize();

            Vector3 enemyDirection = transform.localScale;

            if (transform.position.x < currentWaypoint.transform.position.x)
            {

                enemyDirection.x = enemyWidth;
            }

            else if (transform.position.x > currentWaypoint.transform.position.x)
            {

                enemyDirection.x = -enemyWidth;
            }

            transform.localScale = enemyDirection;

            this.transform.Translate(
                directionOfTravel.x * speed * Time.deltaTime,
                directionOfTravel.y * speed * Time.deltaTime,
                directionOfTravel.z * speed * Time.deltaTime,
                Space.World);
        }*/

        //otherwise, switch waypoints.
        /*else
        {
            if (currentWaypoint == waypoint01) currentWaypoint = waypoint02;
            else currentWaypoint = waypoint01;
        }*/

        //if the player is close enough, shoot a projectile.
        //if ((Vector2.Distance(transform.position, playerTarget.transform.position) < shootDistance) && (!isShooting))
        
        //if ( (playerTarget.transform.position.x == this.transform.position.x)
        if ( this.transform.position.x >= (0.95 * playerTarget.transform.position.x)  && this.transform.position.x <= (1.05 * playerTarget.transform.position.x)
            && (!isShooting)
            )
        {
            //Debug.Log("Player x: " + playerTarget.transform.position.x);
            //Debug.Log("Enemy x: " + transform.position.x);
            StartCoroutine(ShootProjectile());
        }
    }
    // Update is called once per frame
    /*void FixedUpdate()
    {
        //if the enemy is not at the intended waypoint, move towards it and change direction to face it if need be.
        if (Vector2.Distance(transform.position, currentWaypoint.transform.position) > 1f)
        {
            Vector3 directionOfTravel = currentWaypoint.transform.position - transform.position;
            directionOfTravel.Normalize();

            Vector3 enemyDirection = transform.localScale;

            if (transform.position.x < currentWaypoint.transform.position.x)
            {

                enemyDirection.x = enemyWidth;
            }

            else if (transform.position.x > currentWaypoint.transform.position.x)
            {

                enemyDirection.x = -enemyWidth;
            }

            transform.localScale = enemyDirection;

            this.transform.Translate(
                directionOfTravel.x * speed * Time.deltaTime,
                directionOfTravel.y * speed * Time.deltaTime,
                directionOfTravel.z * speed * Time.deltaTime,
                Space.World);
        }

        //otherwise, switch waypoints.
        else
        {
            if (currentWaypoint == waypoint01) currentWaypoint = waypoint02;
            else currentWaypoint = waypoint01;
        }

        //if the player is close enough, shoot a projectile.
        //if ((Vector2.Distance(transform.position, playerTarget.transform.position) < distance) && (!isShooting))
        if (isDetectingPlayer && !isShooting)
        {
            StartCoroutine(ShootProjectile());
        }
    }*/

    private IEnumerator ShootProjectile()
    {
        //acquire the player's current position and rotate towards it, then instantiate a bullet prefab with said rotation.
        //Vector3 vectorToTarget = (playerTarget.transform.position - transform.position).normalized;
        Vector3 vectorToTarget = Vector3.down;
        float angle = Mathf.Atan2(vectorToTarget.x, vectorToTarget.y) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.AngleAxis(-angle + 135.0f, Vector3.forward);
        GameObject projectileGO = Instantiate(projectilePrefab, transform.position, rot) as GameObject;
        projectileGO.GetComponent<Projectile>().SetDirectionAndVelocity(vectorToTarget);


        //set is shooting to true so that the enemy doesn't shoot again, until coroutine is finished.
        isShooting = true;
        //flyingAnim.SetTrigger("FlyingAttack");

        yield return new WaitForSeconds(fireRate);

        isShooting = false;
        //flyingAnim.ResetTrigger("FlyingAttack");

    }

    //enemy is hit with player attack and takes damage
    public void TakeDamage()
    {
        if (!hitState)
        {
            enemyHealth--;

            if (enemyHealth == 0) { StartCoroutine(DeathState()); } //subject to change.
            StartCoroutine(HitState());
        }

    }

    private IEnumerator DeathState()
    {
        //flyingAnim.SetTrigger("Death");
        yield return new WaitForSeconds(0.5f);
        //flyingAnim.ResetTrigger("Death");
        Destroy(this.gameObject);
    }


    //activate invincibilitie frames for enemy upon being hit
    private IEnumerator HitState()
    {
        hitState = true;

        yield return new WaitForSeconds(0.2f);

        hitState = false;
    }
}
