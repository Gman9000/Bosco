using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingEnemy : MonoBehaviour , IEnemy
{
    private Player playerTarget;    //player character
    [SerializeField] private GameObject waypoint01;      //first waypoint
    [SerializeField] private GameObject waypoint02;      //second waypoint
    //[SerializeField] private GameObject firingPosition;  //position that projectiles are created.
    [SerializeField] private GameObject projectilePrefab;      //bullet prefab
    private GameObject currentWaypoint;
    public float speed;                 //enemy speed
    public float followDistance;              //required distance between player and enemy before enemy charges
    public int maxEnemyHealth;
    private int currentEnemyHealth;             //health of enemy unit
    private bool hitState = false;      //is the enemy's invincibility frames currently active
    private bool isShooting = false;    //is the enemy currently shooting.
    public float fireRate;              //the amount of time between each of the enemy's shots.
    public Animator flyingAnim;
    public float deathTimer;

    private float lastPlayerPosTime = 0;

    Vector3 bounceDirection;

    //for bouncing back and forth
    private bool bounceRight;

    SpriteRenderer ren;
    SpriteSimulator sim;

    public Transform projectileSpawnLocation;

    public EnemyRespawner mySpawner;

    private Vector2 bounceMultiplier = Vector2.one;

    private Rigidbody2D myRB;

    bool primeToBounce = true;

    private void Awake()
    {
        bounceRight = false;
        currentEnemyHealth = maxEnemyHealth;
        playerTarget = Player.Instance;
        ren = GetComponentInChildren<SpriteRenderer>();
        sim = GetComponentInChildren<SpriteSimulator>();
        myRB = GetComponent<Rigidbody2D>();
    }

    public void Start()
    {
        currentWaypoint = waypoint01;   //set the first waypoint.
        hitState = false;
        currentEnemyHealth = maxEnemyHealth;
        bounceMultiplier = Vector2.one;     
        bounceDirection = Vector2.zero;
        myRB.velocity = Vector2.zero;
        primeToBounce = true;
        StopAllCoroutines();
    }

    void Update()
    {
        Vector3 enemyDirection = transform.localScale;

        if (transform.position.x < playerTarget.transform.position.x)
        {
            enemyDirection.x = -1.0F;
        }

        else if (transform.position.x > playerTarget.transform.position.x)
        {
            enemyDirection.x = 1.0F;
        }
        transform.localScale = enemyDirection;

        if (!hitState)
            Motion(bounceDirection);

        if (!isShooting)
        {
            StartCoroutine(ShootProjectile());
        }

    }

    public void Motion(Vector3 directionOfTravel, float speed = 1)
    {
        this.transform.Translate(
            directionOfTravel.x * speed * Time.deltaTime,
            directionOfTravel.y * speed * Time.deltaTime,
            directionOfTravel.z * speed * Time.deltaTime,
            Space.World);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            TakeDamage();
        }

        if (other.CompareTag("PlayerTarget") && !hitState)
        {
            Player.Instance.TakeDamage();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            TakeDamage();
        }

        if (other.CompareTag("PlayerTarget") && !hitState)
        {
            Player.Instance.TakeDamage();
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Ground") && other.GetContact(0).normal.y > 0)
        {
            Bounce();            
            primeToBounce = false;
        }
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (primeToBounce && other.collider.CompareTag("Ground") && other.GetContact(0).normal.y > 0)
        {
            Bounce();
            primeToBounce = false;
        }
    }

    private void Bounce()
    {                    
        bounceRight = !bounceRight;
        if (Random.value > .75F)
            bounceMultiplier.x = 2.0F;
        else
            bounceMultiplier.x = 1.0F;

        bounceMultiplier.y = 1.0F;

        bounceDirection = (bounceRight ? Vector3.right : Vector3.left) * 2 * bounceMultiplier.x;
        myRB.velocity = Vector3.up * 3 * bounceMultiplier.y;
    }

    private IEnumerator ShootProjectile()
    {
        Vector3 vectorToTarget;
        //acquire the player's current position and rotate towards it, then instantiate a bullet prefab with said rotation.
        if (playerTarget.transform.position.x > transform.position.x)
        {
            vectorToTarget = (2 * Vector3.up) + Vector3.right;
        }
        else
        {
            vectorToTarget = (2 * Vector3.up) + Vector3.left;

        }
        GameObject projectileGO = Instantiate(projectilePrefab, projectileSpawnLocation.position, Quaternion.identity) as GameObject;
        projectileGO.GetComponent<Projectile>().SetDirectionAndVelocity(vectorToTarget * 1.5F);


        //set is shooting to true so that the enemy doesn't shoot again, until coroutine is finished.
        isShooting = true;

        yield return new WaitForSeconds(fireRate);

        isShooting = false;

    }

    void FixedUpdate()
    {

        if (hitState)   ren.color = ren.color.a == 0 ? Color.white : new Color(0,0,0,0);
        else            ren.color = Color.white;
    }

    //enemy is hit with player attack and takes damage
    public void TakeDamage()
    {
        if (!hitState)
        {
            currentEnemyHealth--;

            if (currentEnemyHealth == 0) 
                StartCoroutine(DeathState()); //subject to change.
            else
                StartCoroutine(HitState());
        }
    }

    private IEnumerator DeathState()
    {
        yield return new WaitForSeconds(0.5f);
        ResetEnemyHealth();
        gameObject.SetActive(false);
    }

    public void ResetEnemyHealth()
    {
        currentEnemyHealth = maxEnemyHealth;
    }
    //activate invincibilitie frames for enemy upon being hit
    private IEnumerator HitState()
    {
        hitState = true;
        float xDir = playerTarget.FacingRight ? 1 : -1;
        myRB.velocity = xDir * Vector2.right * 5 + Vector2.up * 2;
        bounceDirection.x = xDir;

        yield return new WaitForSeconds(0.5f);
        primeToBounce = true;
        myRB.velocity = Vector2.zero;

        hitState = false;
    }
}
