using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTosser : PawnEnemy
{
    public float fireRate;                                      //the amount of time between each of the enemy's shots.
    public float deathTimer;
    [SerializeField] private GameObject projectilePrefab;       //bullet prefab
    public float speed;                                         //enemy speed
    public float followDistance;                                //required distance between player and enemy before enemy charges
    private bool isShooting = false;                            //is the enemy currently shooting.
    

    Vector3 bounceDirection;

    //for bouncing back and forth
    private bool bounceRight;
    public Transform projectileSpawnLocation;

    private Vector2 bounceMultiplier = Vector2.one;

    bool primeToBounce = true;
    override public void Start()
    {
        base.Start();        
        bounceRight = false;
        isShooting = false;
        currentHealth = maxHealth;
        bounceMultiplier = Vector2.one;     
        bounceDirection = Vector2.zero;
        body.velocity = Vector2.zero;
        primeToBounce = true;
        StopAllCoroutines();
    }

    /*override protected void UpdateState0()
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

        if (transform.position.x <= spawnerBounds.xMin)
            bounceDirection = Vector3.right * 2;
        else if (transform.position.x >= spawnerBounds.xMax)
            bounceDirection = Vector3.left * 2;

        if (Invincible)
            Motion(bounceDirection);

        if (!isShooting)
        {
            StartCoroutine(ShootProjectile());
        }

    }*/

    public void Motion(Vector3 directionOfTravel, float speed = 1)
    {
        this.transform.Translate(
            directionOfTravel.x * speed * Time.deltaTime,
            directionOfTravel.y * speed * Time.deltaTime,
            directionOfTravel.z * speed * Time.deltaTime,
            Space.World);
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
        body.velocity = Vector3.up * 3 * bounceMultiplier.y;
    }

    private IEnumerator ShootProjectile()
    {
        Vector3 vectorToTarget;

        //acquire the player's current position and rotate towards it, then instantiate a bullet prefab with said rotation.
        vectorToTarget = (3 * Vector3.up) + Vector3.right / 2;
        if (playerTarget.transform.position.x < transform.position.x)
            vectorToTarget.x = -vectorToTarget.x;

        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnLocation.position, Quaternion.identity) as GameObject;
        projectile.GetComponent<Projectile>().SetDirectionAndVelocity(vectorToTarget * 1.5F);


        //set is shooting to true so that the enemy doesn't shoot again, until coroutine is finished.
        isShooting = true;

        yield return new WaitForSeconds(fireRate);

        isShooting = false;

    }
}
