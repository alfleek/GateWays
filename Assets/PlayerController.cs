using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public Animator animator;
    public Rigidbody2D rb;
    public LayerMask groundLayer;


    public bool facingRight { get; private set; }
    private bool isJumping;
    public float lastGrounded;
    private bool isJumpCut;
    private bool isJumpFalling;
    private Vector2 moveInput;
    private float lastJumpInput;

    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpTimeToApex;
    [SerializeField] private float fallGravityMult;
    [SerializeField] private float jumpCutGravityMult;
    [SerializeField] private float terminalVel;
    [SerializeField] private float jumpForce;
    private float gravityStrength;
    private float gravityScale;

    [SerializeField] private float runMaxSpeed;
    [SerializeField] private float runAccel;
    private float runAccelForce;
    [SerializeField] private float runDecel;
    private float runDecelForce;

    [SerializeField] private float coyoteTime;
    [SerializeField] private float jumpInputBuffer;
    [SerializeField] private Vector2 groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize;

    public UnityEvent OnDeath;
    public bool isDead;
    private Vector2 respawnPoint;

    public bool hasKey;


    // Start is called before the first frame update
    void Start()
    {
        isDead = false;
        hasKey = false;
        SetRespawnPoint(transform.position);
        rb = GetComponent<Rigidbody2D>();
        facingRight = true;

        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);
        gravityScale = gravityStrength / Physics2D.gravity.y;
        rb.gravityScale = gravityScale;
        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

        runAccelForce = ((1 / Time.fixedDeltaTime) * runAccel) / runMaxSpeed;
        runDecelForce = ((1 / Time.fixedDeltaTime) * runDecel) / runMaxSpeed;
    }

    void FixedUpdate()
    {
        if (isDead) return;
        Run();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;
        lastGrounded -= Time.deltaTime;
        lastJumpInput -= Time.deltaTime;

        if (IsGrounded()) animator.SetTrigger("Grounded");

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        if (moveInput.x != 0 && (moveInput.x > 0 != facingRight)) Flip();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            lastJumpInput = jumpInputBuffer;
        }
        if (Input.GetKeyUp(KeyCode.Space) && isJumping && rb.velocity.y > 0)
        {
            isJumpCut = true;
        }


        //Grounded
        if (!isJumping && IsGrounded())
        {
            lastGrounded = coyoteTime;
            animator.ResetTrigger("Jump");
        }

        //Falling
        if (isJumping && rb.velocity.y < 0)
        {
            isJumping = false;
            isJumpFalling = true;
            animator.ResetTrigger("Grounded");
        }

        //Recently Grounded
        if (lastGrounded > 0 && !isJumping)
        {
            isJumpCut = false;

            if (!isJumping) isJumpFalling = false;
        }

        //Jump
        if (CanJump() && lastJumpInput > 0 && gameObject.GetComponent<Teleportable>().lastTeleported < 0)
        {
            isJumping = true;
            isJumpCut = false;
            isJumpFalling = false;
            Jump();
        }

        if (isJumpCut)
        {
            //Higher gravity if space released early
            rb.gravityScale = gravityScale * jumpCutGravityMult;
        }
        else if (rb.velocity.y < 0)
        {
            //Higher gravity if falling
            rb.gravityScale = gravityScale * fallGravityMult;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -terminalVel));
        }
        else
        {
            //Default gravity if standing or moving upwards
            rb.gravityScale = gravityScale;
        }


        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("VerticalSpeed", rb.velocity.y);

    }

    private void Run()
    {
        float targetSpeed = moveInput.x * runMaxSpeed;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccelForce : runDecelForce;

        if (Mathf.Abs(targetSpeed) == 0f && (lastGrounded < 0 || gameObject.GetComponent<Teleportable>().lastTeleported >= 0) && moveInput.y >= 0)
        {
            //Keep our momentum in the air
            accelRate = 0;
        }

        float speedDif = targetSpeed - rb.velocity.x;
        float movement = speedDif * accelRate;
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    private void Jump()
    {
        lastJumpInput = 0;
        lastGrounded = 0;

        float force = jumpForce;
        //Adjust force needed if we are falling already (In case of coyote time)
        if (rb.velocity.y < 0)
        {
            force -= rb.velocity.y;
        }

        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        animator.SetTrigger("Jump");
        animator.ResetTrigger("Grounded");
    }
    public void Flip()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(groundCheckPoint + (Vector2)transform.position, groundCheckSize, 0, Vector2.down, 0.05f, groundLayer);
    }

    private bool CanJump()
    {
        return lastGrounded > 0 && !isJumping;
    }

    public void SetRespawnPoint(Vector2 position)
    {
        respawnPoint = position;
    }
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Lethal"))
        {
            isDead = true;
            animator.SetBool("isDead", true);
            rb.velocity = Vector2.zero;
            OnDeath.Invoke();
        }
    }

    private void Respawn()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheckPoint + (Vector2)transform.position, groundCheckSize);
    }
}
