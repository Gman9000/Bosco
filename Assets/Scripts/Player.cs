using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
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

    //for attacking
    //[SerializeField] private GameObject leftAttackHitbox;
    [SerializeField] private GameObject rightAttackHitbox;
    [SerializeField] private GameObject upAttackHitbox;
    [SerializeField] private GameObject downAttackHitbox;
    [SerializeField] private GameObject radialAttackHitbox;
    private bool isHitting = false;
    public float hitTime;



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
    [SerializeField] private Animator animatorController;

    //for groundChecks
    bool isGrounded;
    // Start is called before the first frame update
    void Awake()
    {
        //shootCooldownCounter = 0f;
        //essence = null;
        //jumpTimeCountdown = jumpTime;
        myRB = GetComponent<Rigidbody2D>();
        myRB.gravityScale = originalGravityScale;
        boxCollider2D = GetComponent<BoxCollider2D>();
        //canPlay = true;
        checkpoint = this.gameObject.transform.position;
        Instance = this;
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
        if (isGrounded)
        {
            myRB.gravityScale = originalGravityScale;
        }
        //myRB.velocity = new Vector2(0f, 0f);
        myRB.velocity = new Vector2(0f, myRB.velocity.y);
        //if (LevelManager.Instance.GetGameStartStatus() && !PauseMenu.Instance.Paused())
        //{
            if (PlayerInput.IsPressingLeft())
            {
                myRB.velocity = new Vector2(-moveSpeed, myRB.velocity.y);
                this.transform.rotation = new Quaternion(0f, 180f, this.transform.rotation.z, this.transform.rotation.w);
                animatorController.SetFloat("horizontalVelocity", Mathf.Abs(myRB.velocity.x));
            }

            if (PlayerInput.IsPressingRight())
            {
                myRB.velocity = new Vector2(moveSpeed, myRB.velocity.y);
                this.transform.rotation = new Quaternion(0f, 0f, this.transform.rotation.z, this.transform.rotation.w);
                animatorController.SetFloat("horizontalVelocity", myRB.velocity.x);
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
            if(PlayerInput.HasPressedAttackKey() && isGrounded)
            {
                StartCoroutine(Hit(4));
            }
            if (PlayerInput.IsPressingUp() && PlayerInput.HasPressedAttackKey() && isGrounded)
            {
                StartCoroutine(Hit(1));
            }
            if (PlayerInput.IsPressingDown() && PlayerInput.HasPressedAttackKey() && isGrounded)
            {
                StartCoroutine(Hit(2));
            }
            /*if (PlayerInput.IsPressingLeft() && PlayerInput.HasPressedAttackKey() && isGrounded)
            {
                StartCoroutine(Hit(3));
            }*/
            if ((PlayerInput.IsPressingRight() || PlayerInput.IsPressingLeft()) && PlayerInput.HasPressedAttackKey() && isGrounded)
            {
                StartCoroutine(Hit(4));
            }
            if(!isGrounded && PlayerInput.HasPressedAttackKey() && !PlayerInput.IsPressingDown())
            {
                StartCoroutine(Hit(5));
            }
            if(!isGrounded && PlayerInput.HasPressedAttackKey() && PlayerInput.IsPressingDown())
            {
                myRB.gravityScale = originalGravityScale * 2;
                StartCoroutine(Hit(2));
            }
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
                break;
            case 2:
                //Downwards attack
                downAttackHitbox.SetActive(true);
                break;
            /*case 3:
                //Left attack
                leftAttackHitbox.SetActive(true);
                break;*/
            case 4:
                //Right attack
                rightAttackHitbox.SetActive(true);
                break;
            case 5:
                //radial attack
                radialAttackHitbox.SetActive(true);
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
                break;
            case 2:
                //Downwards attack
                downAttackHitbox.SetActive(false);
                break;
            /*case 3:
                //Left attack
                leftAttackHitbox.SetActive(false);
                break;*/
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

        /*DKanim.SetBool("slash", false);
        DKanim.SetBool("glidingSlash", false);
        DKanim.SetBool("jumpingSlash", false);*/

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            checkpoint = new Vector2(other.gameObject.transform.position.x, other.gameObject.transform.position.y + 1);
        }
        if (other.CompareTag("Reset"))
        {
            this.gameObject.transform.position = checkpoint;
        }
    }
    /*private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Squit"))
        {
            this.gameObject.transform.position = other.GetComponent<SquitController>().topRightVertexEscape.position + escapeOffset;
        }
    }*/

    public void ResetToLastCheckPoint()
    {
        this.gameObject.transform.position = checkpoint;
    }
}
