using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    static public bool isHurting => Time.time - Instance.hurtTimeStamp < Instance.invincibilityTime;
    static public int Hp => Instance.currentHealth;
    public AudioClip[] sfx_swordSounds;
    public AudioClip sfx_jump;
    public AudioClip sfx_hurt;

    private int consecutiveHits = 0;

    public static Player Instance { get; protected set; }
    private Rigidbody2D myRB;
    public float moveSpeed;
    //private bool canPlay;

    public float invincibilityTime = 1.2F;

    private BoxCollider2D boxCollider2D;

    //for jumping
    //public float originalJumpSpeed;
    public float jumpSpeed;
    public float jumpTime;
    private float jumpTimeCountdown;
    private bool isJumping;
    public float originalGravityScale;
    public float DownThrustGravityModifier;

    public Transform topLeftRaycast;
    public Transform topRightRaycast;
    public Transform bottomLeftRaycast;
    public Transform bottomRightRaycast;
    public float rayCastMagnitude;

    private bool inputMovePaused = false;
    private bool inputAttackPaused = false;


    //for attacking
    //[SerializeField] private GameObject leftAttackHitbox;
    [SerializeField] private GameObject rightAttackHitbox;
    [SerializeField] private GameObject spinAttackHitbox;
    [SerializeField] private GameObject upAttackHitbox;
    [SerializeField] private GameObject downAttackHitbox;
    [SerializeField] private GameObject radialAttackHitbox;
    private bool isHitting = false;
    public float hitTime;
    public float hitCooldown;   // should be less than hit time
    public float hitComboWindow;   // should be less than hit time


    
    private float hitTimeStamp = 0; // the time when the player attacked
    private float comboTimeStamp = 0; // the time when the player attacked

    //health stuff
    public int maxhealth;
    private int currentHealth;

    Coroutine hitCoroutine = null;
    Coroutine pausePhysicsCoroutine = null;
    Coroutine pauseInputMoveCoroutine = null;
    Coroutine pauseInputAttackCoroutine = null;  
    Coroutine attackSequenceCoroutine = null;
    public float deathTime;


    [HideInInspector]public SpriteAnimator animator;

    public bool FacingRight => transform.rotation.y == 0;

    private Vector3 checkpoint;

    //sfx
    //[SerializeField] private AudioSource shotSFX;
    //[SerializeField] private AudioSource jumpSFX;

    //animator
    //[SerializeField] private Animator animatorController;

    //for groundChecks
    bool isGrounded;
    bool isHittingCeiling;
    bool isHittingRightWall;
    bool isHittingLeftWall;
    // Start is called before the first frame update

    bool canAerialAttack = false;



    bool isGroundPounding = false;

    bool PhysicsPaused => myRB.constraints == RigidbodyConstraints2D.FreezeAll;

    float hurtTimeStamp = 0;


    void Awake()
    {
        currentHealth = maxhealth;
        //shootCooldownCounter = 0f;
        //essence = null;
        //jumpTimeCountdown = jumpTime;
        myRB = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        //canPlay = true;
        checkpoint = this.gameObject.transform.position;
        Instance = this;

        animator = GetComponent<SpriteAnimator>();
    }

    void Start()
    {
        isGroundPounding = false;
        myRB.gravityScale = originalGravityScale;
        myRB.velocity = Vector2.zero;

        upAttackHitbox.SetActive(false);
        downAttackHitbox.SetActive(false);
        rightAttackHitbox.SetActive(false);
        spinAttackHitbox.SetActive(false);
        radialAttackHitbox.SetActive(false);
        animator.PlayDefault();
        hurtTimeStamp = -invincibilityTime;

        myRB.constraints = RigidbodyConstraints2D.FreezeRotation;

        StopAllCoroutines();

        hitCoroutine = null;
        pausePhysicsCoroutine = null;
        pauseInputMoveCoroutine = null;
        pauseInputAttackCoroutine = null;  
        attackSequenceCoroutine = null;
    }

    void Update()
    {

        bool wasGrounded = isGrounded;
        isGrounded = boxCollider2D.IsGrounded(bottomLeftRaycast.position, bottomRightRaycast.position, this.transform.localScale.x, rayCastMagnitude);
        isHittingCeiling = boxCollider2D.IsHittingCeiling(topLeftRaycast.position, topRightRaycast.position, this.transform.localScale.x, rayCastMagnitude);
        isHittingRightWall = boxCollider2D.IsHittingRightWall(topRightRaycast.position, bottomRightRaycast.position, this.transform.localScale.x, rayCastMagnitude);
        isHittingLeftWall = boxCollider2D.IsHittingLeftWall(topLeftRaycast.position, bottomLeftRaycast.position, this.transform.localScale.x, rayCastMagnitude);
        if (Game.gameStarted)
        {
            if (isGrounded)
            {
                if (isGroundPounding)   // if is falling fast
                {
                    CameraController.Instance.VertShake(4);          // shake screen
                    DoPhysicsPause(.4F);
                    downAttackHitbox.SetActive(false);
                    DoInputAttackPause(.4F);
                    isGroundPounding = false;
                }

                myRB.gravityScale = originalGravityScale;
                if (radialAttackHitbox.activeSelf)
                {
                    radialAttackHitbox.SetActive(false);
                    DoPhysicsPause(.05F);
                    animator.PlayDefault();
                }
                canAerialAttack = true;

                if (!wasGrounded)
                {
                    upAttackHitbox.SetActive(false);
                    myRB.velocity = new Vector2(myRB.velocity.x, 0);
                    SnapToPixel();
                }
            }


            if (inputMovePaused)
            {
                if (spinAttackHitbox.activeSelf)
                {
                    myRB.velocity = new Vector2(FacingRight ? 3.0F : -3.0F, myRB.velocity.y);
                }
                if (rightAttackHitbox.activeSelf && isGrounded)
                {
                    if (Time.time - hitTimeStamp > .15F)
                    {
                        myRB.velocity = new Vector2(0, myRB.velocity.y);
                    }
                }
            }
            else
            {
                if (!spinAttackHitbox.activeSelf)
                    myRB.velocity = new Vector2(0, myRB.velocity.y);

                if (PlayerInput.IsPressingLeft())
                {
                    myRB.velocity = new Vector2(-moveSpeed, myRB.velocity.y);
                    this.transform.rotation = new Quaternion(0f, 180f, this.transform.rotation.z, this.transform.rotation.w);
                }
                else if (PlayerInput.IsPressingRight())
                {
                    myRB.velocity = new Vector2(moveSpeed, myRB.velocity.y);
                    this.transform.rotation = new Quaternion(0f, 0f, this.transform.rotation.z, this.transform.rotation.w);
                }
                else
                {
                    SnapToPixel();
                }

                if (PlayerInput.IsPressingDown())
                {
                    myRB.velocity = new Vector2(0, myRB.velocity.y);
                }

                if (isGrounded && PlayerInput.HasPressedJumpKey())
                {
                    isJumping = true;
                    jumpTimeCountdown = jumpTime;
                    myRB.velocity = new Vector2(myRB.velocity.x, jumpSpeed);
                    SoundSystem.PlaySfx(sfx_jump, 2);
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
            }


            AttackInputs();


            if (PlayerInput.HasPressedResetKey())
            {
                LevelManager.Instance.ResetToLastCheckPoint();
            }

            if (PlayerInput.HasPressedEscapeKey())
            {
                if (Game.isPaused)
                    Game.Unpause();
                else
                    Game.Pause();
            }


            Animate();
        }
    }

    void SnapToPixel()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Round(pos.x / Game.PIXEL) * Game.PIXEL;
        pos.y = Mathf.Floor(pos.y / Game.PIXEL) * Game.PIXEL;
        transform.position = pos;
    }

    void FixedUpdate()
    {
        if (Time.time - hurtTimeStamp < invincibilityTime)
        {
            animator.Ren.color = animator.Ren.color.a == 0 ? Color.white : new Color(0,0,0,0);
        }
        else
        {
            animator.Ren.color = Color.white;
        }

        if (HUD.Instance)
            HUD.Instance.Flash(animator.Ren.color.a == 1, "Main Text Layer", "BG");
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Hidden") && other.contacts[0].normal.y > 0)
        {
            Vector3 pos = transform.position;
            pos.y = other.collider.bounds.max.y + boxCollider2D.bounds.size.y / 2.0F;
            transform.position = pos;
        }

        if (other.collider.CompareTag("Hidden") || other.collider.CompareTag("Ground")) 
        {
            SnapToPixel();
        }
            
    }


    void Animate()
    {
        if (isGrounded)
        {
            if (upAttackHitbox.activeSelf)
            {
                animator.Play(AnimMode.Hang, "upslash ground");
            }
            else if (spinAttackHitbox.activeSelf)
            {
                animator.Play(AnimMode.Looped, "spin");
            }
            else if (rightAttackHitbox.activeSelf)
            {
                animator.Play(AnimMode.Hang, consecutiveHits <= 1 ? "slash 1" : "slash 2");
            }
            else if (!PhysicsPaused && !inputMovePaused && PlayerInput.IsPressingDown())
            {
                animator.Play(AnimMode.Looped, "duck");
            }
            else if (!PhysicsPaused && !inputMovePaused && (PlayerInput.IsPressingLeft() || PlayerInput.IsPressingRight()))
            {
                animator.Play(AnimMode.Looped, "run");
            }
            else if (!PhysicsPaused)
            {
                animator.Play(AnimMode.Looped, "idle");
            }
        }
        else
        {
            if (spinAttackHitbox.activeSelf)
            {
                animator.Play(AnimMode.Looped, "spin");
            }
            else if (downAttackHitbox.activeSelf)
            {
                animator.Play(AnimMode.Hang, "downslash");
            }
            else if (radialAttackHitbox.activeSelf)
            {
                animator.Play(AnimMode.Looped, "somersault");
            }
            else if (upAttackHitbox.activeSelf)
            {
                animator.Play(AnimMode.Hang, "upslash air");
            }
            else if (myRB.velocity.y > 0)
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

    public void DoInputMovePause(float time)
    {
        if (pauseInputMoveCoroutine != null)
            StopCoroutine(pauseInputMoveCoroutine);
        pauseInputMoveCoroutine = StartCoroutine(PauseInputMove(time));
    }

    public void DoInputAttackPause(float time)
    {
        if (pauseInputAttackCoroutine != null)
            StopCoroutine(pauseInputAttackCoroutine);
        pauseInputAttackCoroutine = StartCoroutine(PauseInputAttack(time));
    }

    public void TakeDamage()
    {
        if (Time.time - hurtTimeStamp >= invincibilityTime)
        {
            hurtTimeStamp = Time.time;
            SoundSystem.PlaySfx(sfx_hurt, 3);
            
            currentHealth--;
            if(currentHealth <= 0)
            {
                //for now this is death
                StartCoroutine(Die());
            }
        }
    }

    private void AttackInputs()
    {
        
        if (Time.time - comboTimeStamp > hitComboWindow)
            consecutiveHits = 0;

        if (PlayerInput.HasPressedAttackKey())
        {
            if (Time.time - hitTimeStamp >= hitCooldown && !PhysicsPaused && !inputAttackPaused)
            {
                if (attackSequenceCoroutine != null)
                    StopCoroutine(attackSequenceCoroutine);
                attackSequenceCoroutine = StartCoroutine(AttackSequence());
            }
        }
    }

    private IEnumerator Hit(int hitBoxIndex)
    {

        isHitting = true;       //set isHitting to true.

        float hitMod = 1.0F;

        upAttackHitbox.SetActive(false);
        downAttackHitbox.SetActive(false);
        rightAttackHitbox.SetActive(false);
        spinAttackHitbox.SetActive(false);
        radialAttackHitbox.SetActive(false);
            
        switch (hitBoxIndex)
        {
            case 1:
                // Upwards attack
                upAttackHitbox.SetActive(true);
                if (!isGrounded)
                    hitMod = 1.2F;
                else
                    hitMod = .85F;
                break;
            case 2:
                //Downwards attack
                downAttackHitbox.SetActive(true);
                hitMod = 500;//for down attacks we want the hitbox active for as long as possible
                break;
            case 3:
                //Spin attack
                //take the leftAttackHitBox and turn that into the spin attack that hits both the left and the right of the player
                spinAttackHitbox.SetActive(true);
                hitMod = 2;
                break;
            case 4:
                //Right attack
                rightAttackHitbox.SetActive(true);

                break;
            case 5:
                //radial attack
                radialAttackHitbox.SetActive(true);
                hitMod = 2;
                break;
            default:
                Debug.Log("AN ERROR HAS OCCURRED WHILE TRYING TO ATTACK");
            // Statements to Execute if No Case Matches
            break;
        }



        yield return new WaitForSeconds(hitTime * hitMod);   //wait the established amount of seconds.

        switch (hitBoxIndex)
        {
            case 1:
                // Upwards attack
                upAttackHitbox.SetActive(false);
                break;
            case 2:
                //Downwards attack
                downAttackHitbox.SetActive(false);
                break;
            case 3:
                //Spin attack
                spinAttackHitbox.SetActive(false);                
                break;
            case 4:
                //Right attack
                rightAttackHitbox.SetActive(false);
                break;
            case 5:
                //radial attack
                radialAttackHitbox.SetActive(false);
                break;
            default:
                Debug.Log("AN ERROR HAS OCCURRED WHILE TRYING TO ATTACK");
                // Statements to Execute if No Case Matches
                break;
        }
        isHitting = false;                          //set isHitting to false.

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Reset"))
        {
            this.gameObject.transform.position = checkpoint;
        }
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

    private IEnumerator Die()
    {
        //animatorController.SetBool("playerDed", true);
        yield return new WaitForSeconds(deathTime);   //wait the established amount of seconds.

        if (Game.lives > 0)
        {
            Game.lives--;
            ResetToLastCheckPoint();
        }
        else
        {
            // game over
        }

    }
    public void ResetToLastCheckPoint()
    {
        this.gameObject.transform.position = checkpoint;
        currentHealth = maxhealth;
        Start();
    }

    IEnumerator PausePhysics(float duration)
    {
        Vector2 velocity = myRB.velocity;
        myRB.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(duration);
        myRB.constraints = RigidbodyConstraints2D.FreezeRotation;
        pausePhysicsCoroutine = null;
        myRB.velocity = velocity;
        SnapToPixel();
        yield break;
    }

    IEnumerator PauseInputMove(float duration)
    {
        inputMovePaused = true;
        myRB.velocity = Vector2.zero;
        yield return new WaitForSeconds(duration);
        inputMovePaused = false;
        SnapToPixel();
        yield break;
    }

    IEnumerator PauseInputAttack(float duration)
    {
        inputAttackPaused = true;
        yield return new WaitForSeconds(duration);
        inputAttackPaused = false;
        SnapToPixel();
        yield break;
    }

    IEnumerator AttackSequence()
    {
        if (!isGrounded)
        {
            if (canAerialAttack)
                canAerialAttack = false;
            else
            {
                attackSequenceCoroutine = null;
                yield break;
            }
        }

        
        Vector2 lastInputDirection = Vector2.zero;
        if (isGrounded)
        {
            if (PlayerInput.IsPressingUp())
            {
                DoInputMovePause(.08F);
            }
            else
            {
                DoInputMovePause(0.001F);
                myRB.velocity = new Vector2(FacingRight ? 4F : -4F, myRB.velocity.y);
            }
        }
        else
            DoPhysicsPause(.08F);

        do
        {
            yield return new WaitForEndOfFrame();
            if (PlayerInput.IsPressingDown() && !isGrounded)
                lastInputDirection = Vector2.down;
            else if (PlayerInput.IsPressingUp() && !upAttackHitbox.activeSelf)
                lastInputDirection = Vector2.up;
            else if (PlayerInput.IsPressingLeft())
                lastInputDirection = Vector2.left;
            else if (PlayerInput.IsPressingRight())
                lastInputDirection = Vector2.right;

        }
        while ((PhysicsPaused && !isGrounded) || (inputMovePaused && isGrounded));


        if (isGrounded)
            animator.PlayDefault();


        if (lastInputDirection == Vector2.down)
        {
            if (isGrounded)
            {

            }
            else
            {
                SoundSystem.PlaySfx(sfx_swordSounds[0], 4);   //play attack sfx
                
                myRB.velocity = Vector3.down * 38;
                DoInputAttackPause(.5F);
                isGroundPounding = true;
                myRB.gravityScale = 0;
                if (hitCoroutine != null)
                    StopCoroutine(hitCoroutine);
                hitCoroutine = StartCoroutine(Hit(2));
            }
        }
        else if (lastInputDirection == Vector2.up)
        {
            if (isGrounded)
            {
                SoundSystem.PlaySfx(sfx_swordSounds[0], 4);   //play attack sfx
                DoPhysicsPause(.12F);
                DoInputMovePause(.3F);
                hitTimeStamp = Time.time;
            }
            else
            {
                myRB.velocity = Vector3.up * 15;
                myRB.gravityScale = originalGravityScale * .97F;
            }

            SoundSystem.PlaySfx(sfx_swordSounds[0], 4);
            if (hitCoroutine != null)
                StopCoroutine(hitCoroutine);
            hitCoroutine = StartCoroutine(Hit(1));
        }
        else
        {
            if (isGrounded)
            {
                consecutiveHits++;

                switch (consecutiveHits)
                {
                    case 1:
                        SoundSystem.PlaySfx(sfx_swordSounds[0], 4);   //play attack sfx
                        DoPhysicsPause(.08F);
                        DoInputMovePause(.3F);
                        myRB.velocity = Vector2.zero;
                        if (hitCoroutine != null)
                            StopCoroutine(hitCoroutine);
                        hitCoroutine = StartCoroutine(Hit(4));
                        
                        break;
                    case 2:
                        SoundSystem.PlaySfx(sfx_swordSounds[1], 4);   //play attack sfx

                        DoPhysicsPause(.08F);
                        DoInputMovePause(.3F);
                        myRB.velocity = Vector2.zero;
                        if (hitCoroutine != null)
                            StopCoroutine(hitCoroutine);
                        hitCoroutine = StartCoroutine(Hit(4));
                        
                        break;
                    case 3: //do spin attack here and reset the other trigger here
                        SoundSystem.PlaySfx(sfx_swordSounds[3], 4);
                        DoInputMovePause(.33F);
                        DoInputAttackPause(.66F);
                        if (hitCoroutine != null)
                            StopCoroutine(hitCoroutine);
                        hitCoroutine = StartCoroutine(Hit(3));
                        consecutiveHits = 0;
                        break;

                }
                comboTimeStamp = Time.time;
                hitTimeStamp = Time.time;
            }
            else
            {
                // radial spin
                SoundSystem.PlaySfx(sfx_swordSounds[3], 4);
                if (hitCoroutine != null)
                    StopCoroutine(hitCoroutine);
                hitCoroutine = StartCoroutine(Hit(5));
            }
        }

        attackSequenceCoroutine = null;
        yield break;
    }
}
