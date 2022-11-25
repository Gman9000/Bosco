using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    //[SerializeField] private Rigidbody2D rigidBody;      //enemy rigidBody
    private GameObject playerTarget;    //player character
    [SerializeField] private GameObject waypoint01;      //first waypoint
    [SerializeField] private GameObject waypoint02;      //second waypoint
    //[SerializeField] private GameObject firingPosition;  //position that projectiles are created.
    [SerializeField] private GameObject projectilePrefab;      //bullet prefab
    private GameObject currentWaypoint;
    public float speed;                 //enemy speed
    public float distance;              //required distance between player and enemy before enemy charges
    public float enemyWidth;            //width of enemy, needed for when enemy turns around
    public int enemyHealth;             //health of enemy unit
    private bool hitState = false;      //is the enemy's invincibility frames currently active
    private bool isShooting = false;    //is the enemy currently shooting.
    public float fireRate;              //the amount of time between each of the enemy's shots.
    public Animator flyingAnim;
    public float deathTimer;

    private void Awake()
    {
        playerTarget = GameObject.FindWithTag("Player");
    }

    void Start()
    {
        currentWaypoint = waypoint01;   //set the first waypoint.
    }

    // Update is called once per frame
    void FixedUpdate()
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
        if ((Vector2.Distance(transform.position, playerTarget.transform.position) < distance) && (!isShooting))
        {
            StartCoroutine(ShootProjectile());
        }
    }

    private IEnumerator ShootProjectile()
    {
        //acquire the player's current position and rotate towards it, then instantiate a bullet prefab with said rotation.
        Vector3 vectorToTarget = (playerTarget.transform.position - transform.position).normalized;
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
