using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public AudioClip[] sfx_swordSide;
    private int consecutiveHits = 0;

    public static Player Instance { get; protected set; }
    private Rigidbody2D myRB;
    public float moveSpeed;
    //private bool canPlay;
    private BoxCollider2D boxCollider2D;

    //for jumping
    //public float originalJumpSpeed;
    public float jumpSpeed;
    public float jumpTime;
    private float jumpTimeCountdown;
    private bool isJumping;
    public float originalGravityScale;
    public float DownThrustGravityModifier;

    //for attacking
    //[SerializeField] private GameObject leftAttackHitbox;
    [SerializeField] private GameObject rightAttackHitbox;
    [SerializeField] private GameObject SpinAttackHitbox;
    [SerializeField] private GameObject upAttackHitbox;
    [SerializeField] private GameObject downAttackHitbox;
    [SerializeField] private GameObject radialAttackHitbox;
    private bool isHitting = false;
    public float hitTime;
    public float hitCooldown;   // should be less than hit time
    public float hitComboWindow;   // should be less than hit time
    public float comboCooldown;
    private float hitTimeStamp = 0; // the time when the player attacked

    //health stuff
    public float maxhealth;
    private float currentHealth;

    Coroutine pausePhysicsCoroutine = null;
    public float deathTime;


    private SpriteAnimator animator;


    //[SerializeField] private float bouncyJumpTimeModifier;
    //[SerializeField] private float bouncyJumpSpeedModifier;

    //for firing the Essence Absorb bullet
    //public GameObject bullet;
    //public Transform firePosition;
    //public float firingSpeed;
    //private Vector2 lookDirection;
    //private float lookAngle;
    //private float shootCooldownCounter;
    //public float shootCooldown;
    //for escaping a squit
    //private Vector3 escapeOffset = new Vector3(-0.5f,0.5f,0f);
    //for the essence that is absorbed;
    //public GameObject essence;

    private Vector3 checkpoint;

    //sfx
    //[SerializeField] private AudioSource shotSFX;
    //[SerializeField] private AudioSource jumpSFX;

    //animator
    //[SerializeField] private Animator animatorController;

    //for groundChecks
    bool isGrounded;
    bool isHittingCeiling;
    bool IsHittingRightWall;
    bool IsHittingLeftWall;
    // Start is called before the first frame update


    bool isGroundPounding = false;
    void Awake()
    {
        currentHealth = maxhealth;
        //shootCooldownCounter = 0f;
        //essence = null;
        //jumpTimeCountdown = jumpTime;
        myRB = GetComponent<Rigidbody2D>();
        myRB.gravityScale = originalGravityScale;
        boxCollider2D = GetComponent<BoxCollider2D>();
        //canPlay = true;
        checkpoint = this.gameObject.transform.position;
        Instance = this;

        animator = GetComponent<SpriteAnimator>();
    }

    void Start()
    {
        isGroundPounding = false;
    }

    // Update is called once per frame
    void Update()
    {
        //animatorController.SetFloat("horizontalVelocity", myRB.velocity.x);
        //animatorController.SetFloat("verticalVelocity", myRB.velocity.y);
        //animatorController.ResetTrigger("shoot");
        /*if (shootCooldownCounter >= 0f)
        {
            shootCooldownCounter -= Time.deltaTime;
        }*/
        isGrounded = boxCollider2D.IsGrounded(myRB.position, this.transform.localScale.x);
        isHittingCeiling = boxCollider2D.IsHittingCeiling(myRB.position, this.transform.localScale.x);
        IsHittingRightWall = boxCollider2D.IsHittingRightWall(myRB.position, this.transform.localScale.x);
        IsHittingLeftWall = boxCollider2D.IsHittingLeftWall(myRB.position, this.transform.localScale.x);
        if (isGrounded)
        {
            if (isGroundPounding)   // if is falling fast
            {
                CameraController.Instance.HorShake(3);          // shake screen
                DoPhysicsPause(.2F);
                isGroundPounding = false;
            }

            myRB.gravityScale = originalGravityScale;
        }
        //animatorController.SetBool("isGrounded", isGrounded);
        //myRB.velocity = new Vector2(0f, 0f);
        myRB.velocity = new Vector2(0f, myRB.velocity.y);
        //animatorController.SetFloat("horizontalVelocity", Mathf.Abs(myRB.velocity.x));
        //animatorController.SetBool("crouching", false);
        //if (LevelManager.Instance.GetGameStartStatus() && !PauseMenu.Instance.Paused())
        //{
        //animatorController.SetFloat("horizontalVelocity", -1f);
        if (PlayerInput.IsPressingLeft())
            {
                myRB.velocity = new Vector2(-moveSpeed, myRB.velocity.y);
                this.transform.rotation = new Quaternion(0f, 180f, this.transform.rotation.z, this.transform.rotation.w);
            }

            if (PlayerInput.IsPressingRight())
            {
                myRB.velocity = new Vector2(moveSpeed, myRB.velocity.y);
                this.transform.rotation = new Quaternion(0f, 0f, this.transform.rotation.z, this.transform.rotation.w);
            }
        
            if (isGrounded && PlayerInput.HasPressedJumpKey())
            {
                isJumping = true;
                jumpTimeCountdown = jumpTime;
                myRB.velocity = new Vector2(myRB.velocity.x, jumpSpeed);
            }

            if (isJumping && PlayerInput.HasHeldJumpKey())
            {
                if (jumpTimeCountdown > 0)
                {
                    myRB.velocity = new Vector2(myRB.velocity.x, jumpSpeed);
                    jumpTimeCountdown -= Time.deltaTime;
                    if (isHittingCeiling)
                    {
                        jumpTimeCountdown = 0;
                    }
                }
                else
                {
                    isJumping = false;
                }
            }
            if (PlayerInput.HasReleasedJumpKey())
            {
               isJumping = false;
            }
            if(PlayerInput.IsPressingDown() && isGrounded)
            {
                //animatorController.SetBool("crouching", true);
            }




        /*if (PlayerInput.HasPressedJumpKey())
        {
            //Debug.Log("We pressed jump");
            //Debug.Log("Grounded: " + isGrounded);
            //Debug.Log("Bouncy Grounded: " + isOnBouncyGround);
            if (isGrounded)
            {
            //jumpSFX.Play();
            //myRB.AddForce(Vector2.up * jumpSpeed,ForceMode2D.Impulse);
            //myRB.velocity = new Vector2(myRB.velocity.x, jumpSpeed);
            //myRB.velocity = new Vector2(myRB.velocity.x, jumpSpeed);
                jumpSpeed = originalJumpSpeed;
                jumpTimeCountdown = jumpTime;
            }
        }*/
        /*if (PlayerInput.HasPressedAttackKey())
        {
            shotSFX.Play();
            lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            lookAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            firePosition.rotation = Quaternion.Euler(0f, 0f, lookAngle - 90f);
            GameObject firedBullet = Instantiate(bullet, firePosition.position, firePosition.rotation);
            firedBullet.GetComponent<Rigidbody2D>().velocity = firePosition.up * firingSpeed;

            //do something
        }*/

        /*if (PlayerInput.HasPressedAbsorbPlacementKey() && essence == null && shootCooldownCounter <= 0f)
        {
            shotSFX.Play();
            animatorController.SetTrigger("shoot");
            lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            lookAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            firePosition.rotation = Quaternion.Euler(0f, 0f, lookAngle - 90f);
            GameObject firedBullet = Instantiate(bullet, firePosition.position, firePosition.rotation);
            firedBullet.GetComponent<Rigidbody2D>().velocity = firePosition.up * firingSpeed;
            shootCooldownCounter = shootCooldown;
            //do something
        }

        if (PlayerInput.HasPressedAbsorbPlacementKey() && essence != null && shootCooldownCounter <= 0f)
        {
            shotSFX.Play();
            animatorController.SetTrigger("shoot");
            lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            lookAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            firePosition.rotation = Quaternion.Euler(0f, 0f, lookAngle - 90f);
            GameObject firedBullet = Instantiate(bullet, firePosition.position, firePosition.rotation);
            firedBullet.GetComponent<Rigidbody2D>().velocity = firePosition.up * firingSpeed;
            shootCooldownCounter = shootCooldown;
            //do something
        }*/

        AttackInputs();


        if (PlayerInput.HasPressedResetKey())
        {
            LevelManager.Instance.ResetToLastCheckPoint();
        }

        if (PlayerInput.HasPressedEscapeKey())
        {
            PauseMenu.Instance.ActivateMenu();
        }

            /*if (jumpTimeCountdown > 0f)
            {
                jumpTimeCountdown -= Time.deltaTime;
                //myRB.gravityScale = 0;
                myRB.velocity = new Vector2(myRB.velocity.x, jumpSpeed);
                animatorController.SetFloat("verticalVelocity", Mathf.Abs(myRB.velocity.y));
            }*/
        //}


        // this snaps the player to be pixel perfect and not make a super janky camera
        /*Vector2 playerPos = transform.position;
        playerPos.x = (float)(System.Math.Round((double)playerPos.x * 16.0) / 16.0);
        playerPos.y = (float)(System.Math.Round((double)playerPos.y * 16.0) / 16.0);
        transform.position = playerPos;*/


        //CameraController.CameraUpdate();


        Animate();
    }

    void Animate()
    {
        if (isGrounded)
        {
            if ((PlayerInput.IsPressingLeft() || PlayerInput.IsPressingRight()))
            {
                animator.Play(AnimMode.Looped, "run");
            }
            else
            {
                animator.Play(AnimMode.Looped, "idle");
            }
        }
        else
        {
            if (myRB.velocity.y > 0)
            {
                animator.Play(AnimMode.Hang, "jump");
            }
            else
            {
                animator.Play(AnimMode.Hang, "fall");
            }
        }
    }

    public void DoPhysicsPause(float time)
    {
        if (pausePhysicsCoroutine != null)
            StopCoroutine(pausePhysicsCoroutine);
        pausePhysicsCoroutine = StartCoroutine(PausePhysics(time));
    }

    public void TakeDamage()
    {
        currentHealth--;
        //animatorController.SetTrigger("getHurt");
        if(currentHealth <= 0)
        {
            //for now this is death
            StartCoroutine(Die());

            //ResetToLastCheckPoint();
        }
        //animatorController.ResetTrigger("getHurt");
    }

    private void AttackInputs()
    {
        if (consecutiveHits >= 3 && Time.time - hitTimeStamp >= comboCooldown)
            {
                consecutiveHits = 0;
            }

            
            if (PlayerInput.HasPressedAttackKey() && Time.time - hitTimeStamp > hitCooldown)
            {
                if (isGrounded) // if grounded
                {
                    if (PlayerInput.IsPressingUp() )
                    {
                        StartCoroutine(Hit(1));

                        DoPhysicsPause(.1F);
                        SoundSystem.PlaySfx(sfx_swordSide[0], 4);   //play attack sfx
                    }
                    else
                    {                        
                        if (consecutiveHits < 3)
                        {
                            if (Time.time - hitTimeStamp < hitComboWindow)
                                consecutiveHits++;
                            else
                                consecutiveHits = 1;

                            DoPhysicsPause(.15F);                        
                            StartCoroutine(Hit(4));
                            SoundSystem.PlaySfx(sfx_swordSide[consecutiveHits - 1], 4);   //play attack sfx
                        }
                        else
                        {
                            //do spin attack here and reset the other trigger here
                            StartCoroutine(Hit(3));
                            return;
                        }
                }
                }
                else
                {
                    if (PlayerInput.IsPressingDown())
                    {
                        StartCoroutine(Hit(2));
                        SoundSystem.PlaySfx(sfx_swordSide[0], 4);   //play attack sfx
                        
                        myRB.velocity = Vector3.down * 38;
                        isGroundPounding = true;
                        myRB.gravityScale = - 0; //originalGravityScale * DownThrustGravityModifier;
                        DoPhysicsPause(.1F);
                    }
                    else if (PlayerInput.IsPressingUp())
                    {
                        StartCoroutine(Hit(1));

                        SoundSystem.PlaySfx(sfx_swordSide[0], 4);   //play attack sfx
                        myRB.velocity = Vector3.up * 15;
                        myRB.gravityScale = originalGravityScale * .97F;
                        DoPhysicsPause(.1F);
                    }
                    else
                    {
                        StartCoroutine(Hit(5));
                        SoundSystem.PlaySfx(sfx_swordSide[0], 4);   //play attack sfx
                    }
                }

                
                hitTimeStamp = Time.time;

                /*if (PlayerInput.IsPressingLeft() && PlayerInput.HasPressedAttackKey() && isGrounded)
                {
                    StartCoroutine(Hit(3));
                }*/
                /*else if ((PlayerInput.IsPressingRight() || PlayerInput.IsPressingLeft()) && PlayerInput.HasPressedAttackKey() && isGrounded)
                {
                    StartCoroutine(Hit(4));
                }*/
            }
    }

    private IEnumerator Hit(int hitBoxIndex)
    {


        /*if (isGrounded) //you are slashing on the ground and are not in the air and are not gliding
        {
            //set some animation for an attack on the ground
        }
        else if (!grounded && !gliding) //you are slashing while in the air and not gliding
        {
            DKanim.SetBool("slash", false);
            DKanim.SetBool("glidingSlash", false);
            DKanim.SetBool("jumpingSlash", true);
        }
        else if (!grounded && gliding) //you are slashing while in the air and gliding
        {
            DKanim.SetBool("slash", false);
            DKanim.SetBool("glidingSlash", true);
            DKanim.SetBool("jumpingSlash", false);
        }*/


        isHitting = true;       //set isHitting to true.

            
        switch (hitBoxIndex)
        {
            case 1:
                // Upwards attack
                upAttackHitbox.SetActive(true);
                //animatorController.SetBool("attackingUp", true);
                break;
            case 2:
                //Downwards attack
                downAttackHitbox.SetActive(true);
                //animatorController.SetTrigger("downThrust");
                break;
            case 3:
                //Spin attack
                //take the leftAttackHitBox and turn that into the spin attack that hits both the left and the right of the player
                SpinAttackHitbox.SetActive(true);
                //animatorController.SetTrigger("attackCombo3");
                //animatorController.SetBool("attackComboEnded",false);
                break;
            case 4:
                //Right attack
                rightAttackHitbox.SetActive(true);
                //animatorController.SetTrigger("attackCombo1");
                //animatorController.SetBool("attackComboEnded",false);

                break;
            case 5:
                //radial attack
                radialAttackHitbox.SetActive(true);
                //animatorController.SetTrigger("radialAttack");
                break;
            default:
                Debug.Log("AN ERROR HAS OCCURRED WHILE TRYING TO ATTACK");
            // Statements to Execute if No Case Matches
            break;
        }



        yield return new WaitForSeconds(hitTime);   //wait the established amount of seconds.

        switch (hitBoxIndex)
        {
            case 1:
                // Upwards attack
                upAttackHitbox.SetActive(false);
                //animatorController.SetBool("attackingUp", false);
                break;
            case 2:
                //Downwards attack
                downAttackHitbox.SetActive(false);
                //animatorController.ResetTrigger("downThrust");
                break;
            case 3:
                //Spin attack
                //take the leftAttackHitBox and turn that into the spin attack that hits both the left and the right of the player
                SpinAttackHitbox.SetActive(false);                
                //animatorController.ResetTrigger("attackCombo3");
                //animatorController.SetBool("attackComboEnded",true);
                break;
            case 4:
                //Right attack
                rightAttackHitbox.SetActive(false);
                //animatorController.ResetTrigger("attackCombo1");
                //animatorController.SetBool("attackComboEnded",true);
                break;
            case 5:
                //radial attack
                radialAttackHitbox.SetActive(false);
                //animatorController.ResetTrigger("radialAttack");
                break;
            default:
                Debug.Log("AN ERROR HAS OCCURRED WHILE TRYING TO ATTACK");
                // Statements to Execute if No Case Matches
                break;
        }
        isHitting = false;                          //set isHitting to false.

        /*DKanim.SetBool("slash", false);
        DKanim.SetBool("glidingSlash", false);
        DKanim.SetBool("jumpingSlash", false);*/

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground") || other.CompareTag("Hidden"))
        {
            checkpoint = new Vector2(other.gameObject.transform.position.x, other.gameObject.transform.position.y + 1);
        }
        if (other.CompareTag("Reset"))
        {
            this.gameObject.transform.position = checkpoint;
        }
        /*if (other.CompareTag("Candle"))
        {
            other.GetComponent<Candle>().LightUpCandle();
        }*/
        /*if (other.CompareTag("Teleport"))
        {
            other.gameObject.GetComponent<EndLevelChecker>().HandleEndOfLevel();
        }*/

    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Teleport"))
        {
            if (PlayerInput.IsPressingUp())
            {
                other.gameObject.GetComponent<EndLevelChecker>().HandleEndOfLevel();
            }
        }
        if (other.gameObject.GetComponent<Collider2D>().CompareTag("EnemyMelee"))
        {
            other.gameObject.GetComponent<Collider2D>().transform.parent.GetComponent<MeleeEnemy>().jumpBackwardsMode = true;
            TakeDamage();
            //also take knockback
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.GetComponent<Collider2D>().CompareTag("Enemy"))
        {
            TakeDamage();
            //also take knockback
        }

    }

    /*private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Squit"))
        {
            this.gameObject.transform.position = other.GetComponent<SquitController>().topRightVertexEscape.position + escapeOffset;
        }
    }*/
    private IEnumerator Die()
    {
        //animatorController.SetBool("playerDed", true);
        yield return new WaitForSeconds(deathTime);   //wait the established amount of seconds.
        ResetToLastCheckPoint();

    }
    public void ResetToLastCheckPoint()
    {
        this.gameObject.transform.position = checkpoint;
        currentHealth = maxhealth;
        //animatorController.SetBool("playerDed", false);
    }

    IEnumerator PausePhysics(float duration)
    {
        Vector2 velocity = myRB.velocity;
        myRB.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(duration);
        myRB.constraints = RigidbodyConstraints2D.FreezeRotation;
        pausePhysicsCoroutine = null;
        myRB.velocity = velocity;
        yield break;
    }
}
