using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.InputSystem;
using DG.Tweening;

public class Mover : MonoBehaviour
{
    //Config
    [Header("Movement")]
    [SerializeField] float runSpeed = 250f;
    [SerializeField] float sensitivity_a = 8f;
    [SerializeField] float sensitivity_r = 12f;
    [SerializeField] float sensitivity_jump = 4f;
    [SerializeField] float sensitivity_onWall = 12f;

    [Header("Jump")]
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float fallMultiplier = 2.5f;
    [SerializeField] float lowJumpMultiplier = 2f;
    [SerializeField] float minFallVelocity = -14.5f;
    [SerializeField] float coyoteTime = 0.1f;
    [SerializeField] float jumpBufferTime = 0.1f;

    [Header("Wall")]
    [SerializeField] float slideSpeed = 1f;
    [SerializeField] float wallJumpLerp = 1f;
    [SerializeField] float wallJumpXDivider = 1.5f;
    [SerializeField] float wallJumpYDivider = 1.5f;
    [SerializeField] float waitTimeToFixWallJump = 0.1f;

    [Header("Dash")]
    [SerializeField] float dashForce = 20f;
    [SerializeField] float dashLerp = 1f;
    [SerializeField] float dashGravity = 3f;
    [SerializeField] float timeToRecovery = 0.3f;
    [SerializeField] float timeToEndDash = 3f;
    [SerializeField] float maxDrag = 8f;
    [SerializeField] float timeToRecoveryDrag = 0.3f;


    [Header("Sprite")]
    [SerializeField] Transform sprite = null;

    //State
    bool isAlive = true;
    bool isJumping = false;
    bool isWallSlidng = true;
    bool isWallJumping = false;
    bool isDashing = false;

    bool canRun = true;

    float defaultGravityScale;

    //Cached Componenet References
    Rigidbody2D myRigidbody2D;
    BoxCollider2D myBodyCollider2D;
    Animator myAnimator;
    PlayerCollisionChecker collisionChecker;

    //Control
    InputActionPhase jumpActionPhase;
    float inputXValue;
    float inputYValue;
    float runThrow;
    float coyoteTimeCounter;
    float jumpBufferCounter;

    ////////////////////////////////// UNITY METHOD //////////////////////////////////////

    void Start()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        myBodyCollider2D = GetComponent<BoxCollider2D>();
        myAnimator = GetComponent<Animator>();
        collisionChecker = GetComponent<PlayerCollisionChecker>();

        defaultGravityScale = myRigidbody2D.gravityScale;
        myRigidbody2D.gravityScale = defaultGravityScale;
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            
        }

        if (isAlive)
        {
            HandleJumpEnd();
            ModifyJumpControl();
            TriggerJump();

            WallSlide();
        }

        UpdateAnimatorParameters();
    }

    private void FixedUpdate()
    {
        if (isAlive)
        {
            Run();
            FlipSprite();
            HandleShortJump();
        }

        ClampFallSpeed();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (myBodyCollider2D.IsTouchingLayers(LayerMask.GetMask("Platform"))) //이게 좀 느리네
        {
            isWallJumping = false;
            isJumping = false;
        }
    }

    ////////////////////////////////// CONTROL //////////////////////////////////////

    public void OnMovement(InputAction.CallbackContext context)
    {
        Vector2 inputValue = context.ReadValue<Vector2>();

        SetInputValue(inputValue);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jumpActionPhase = context.phase;
        if (jumpActionPhase == InputActionPhase.Performed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            // Dash();
        }
    }

    public void SetInputValue(Vector2 inputValue)
    {
        inputXValue = inputValue.x;
        inputYValue = inputValue.y;
    }

    ////////////////////////////////// RUN //////////////////////////////////////

    private void Run()
    {
        if (!canRun) return;

        SetRunThrow();

        Vector2 playerVelocity = new Vector2(runThrow * runSpeed * Time.fixedDeltaTime, myRigidbody2D.velocity.y);

        if (!isWallJumping && !isDashing)
        {
            myRigidbody2D.velocity = playerVelocity;
        }
        else if (isWallJumping)
        {
            myRigidbody2D.velocity = Vector2.Lerp(myRigidbody2D.velocity, playerVelocity, wallJumpLerp * Time.fixedDeltaTime);
        }
        else
        {
            myRigidbody2D.velocity = Vector2.Lerp(myRigidbody2D.velocity, playerVelocity, dashLerp * Time.fixedDeltaTime);
        }

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody2D.velocity.x) > Mathf.Epsilon;
        myAnimator.SetBool("isRunning", playerHasHorizontalSpeed);
    }

    void SetRunThrow()
    {
        float sensitivity = 0f;

        if (collisionChecker.isOnWall && !collisionChecker.isGrounded)
        {
            sensitivity = sensitivity_onWall;
        }
        else if (isJumping)
        {
            sensitivity = sensitivity_jump;
        }
        else
        {
            sensitivity = inputXValue == 0 ? sensitivity_r : sensitivity_a;
        }

        runThrow = Mathf.MoveTowards(runThrow, inputXValue, sensitivity * Time.fixedDeltaTime);
    }

    ////////////////////////////////// JUMP //////////////////////////////////////

    public void TriggerJump()
    {
        // if (isJumping) return;
        if(isDashing) return;
        if (coyoteTimeCounter < 0f || jumpBufferCounter < 0f) return;

        isJumping = true;
        isWallSlidng = false;
        coyoteTimeCounter = -999f;
        jumpBufferCounter = -999f;

        if (collisionChecker.isOnWall && !IsGrounded())
        {
            WallJump();
        }
        else
        {
            Jump();
        }
        // jumpFeedbacks.PlayFeedbacks();
    }

    private void Jump()
    {
        Vector2 jumpVelocityToAdd = new Vector2();
        myRigidbody2D.velocity = new Vector2(myRigidbody2D.velocity.x, 0f);
        jumpVelocityToAdd = new Vector2(0f, jumpForce);

        myRigidbody2D.velocity += jumpVelocityToAdd;
    }

    private void WallJump()
    {
        Vector2 jumpVelocityToAdd = new Vector2();
        Vector2 jumpXDirection = collisionChecker.isOnLeftWall ? Vector2.right : Vector2.left;

        myRigidbody2D.velocity = new Vector2(0f, 0f);
        jumpVelocityToAdd = (jumpXDirection / wallJumpXDivider + Vector2.up / wallJumpYDivider) * jumpForce;
        isWallJumping = true;

        StopCoroutine(DisableCanRunForAWhile(0f));
        StartCoroutine(DisableCanRunForAWhile(waitTimeToFixWallJump));

        myRigidbody2D.velocity += jumpVelocityToAdd;
    }

    bool IsGrounded()
    {
        return collisionChecker.isGrounded;
    }

    void ModifyJumpControl()
    {
        HandleCoyoteTime();
        HandleJumpBuffer();
    }

    void HandleJumpEnd()
    {
        // if(myBodyCollider2D.IsTouchingLayers(LayerMask.GetMask("Platform")))
        // {
        //     // isWallJumping = false;
        //     isJumping = false;
        // }

        if (IsGrounded())
        {
            isJumping = false;
            isWallJumping = false;
            // isDashing = false;
        }
    }

    void HandleCoyoteTime()
    {
        coyoteTimeCounter = (IsGrounded() || isWallSlidng) ? coyoteTime : coyoteTimeCounter - Time.deltaTime;
    }

    void HandleJumpBuffer()
    {
        jumpBufferCounter -= Time.deltaTime;
    }

    private void HandleShortJump()
    {
        if (isWallSlidng || isDashing) { return; }

        if (myRigidbody2D.velocity.y < 0)
        {
            myRigidbody2D.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (myRigidbody2D.velocity.y > 0 && jumpActionPhase != InputActionPhase.Performed)
        {
            myRigidbody2D.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    ////////////////////////////////// WALL //////////////////////////////////////

    void WallSlide()
    {
        if (!canRun) return;

        if (collisionChecker.isOnWall && !collisionChecker.isGrounded)
        {
            float wallSide = collisionChecker.isOnLeftWall ? -1f : 1f;

            if (inputXValue == wallSide)
            {
                myRigidbody2D.velocity = new Vector2(0f, slideSpeed);
                isWallSlidng = true;

                isJumping = false;
                isWallJumping = false;
                isDashing = false;
            }
        }
        else isWallSlidng = false;
    }

    IEnumerator DisableCanRunForAWhile(float timeToDisable) // For Wall Jump
    {
        canRun = false;
        yield return new WaitForSeconds(timeToDisable);
        canRun = true;
    }

    ////////////////////////////////// DASH //////////////////////////////////////

    public void Dash(Vector2 direction)
    {
        // if(isDashing) return;
        StopCoroutine(HandleDash());
        SetRigidbodyDrag(0f);

        //     Camera.main.transform.DOComplete(); // 연출
        // Camera.main.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);
        // FindObjectOfType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));

        // anim.SetTrigger("dash");

        myRigidbody2D.velocity = Vector2.zero;

        // Vector2 dir = new Vector2(inputXValue, inputYValue);
        // direction = dir.normalized;

        myRigidbody2D.velocity += direction * dashForce;
        
        StartCoroutine(HandleDash());
    }

    IEnumerator HandleDash()
    {
        // FindObjectOfType<GhostTrail>().ShowGhost(); // 트레일 연출
        // dashParticle.Play();

        // StartCoroutine(RestrictDoubleGroundDash()); // 연속 대시 방지용
        
        DOVirtual.Float(maxDrag, 0, timeToRecoveryDrag, SetRigidbodyDrag); // Drag 조절

        myRigidbody2D.gravityScale = dashGravity;
        // GetComponent<BetterJumping>().enabled = false;
        isDashing = true;

        yield return new WaitForSeconds(timeToRecovery);

        // dashParticle.Stop();

        myRigidbody2D.gravityScale = 3f;
        // GetComponent<BetterJumping>().enabled = true;

        yield return new WaitForSeconds(timeToEndDash);
        isDashing = false;
    }

    void SetRigidbodyDrag(float dragToSet)
    {
        myRigidbody2D.drag = dragToSet;
    }

    ////////////////////////////////// OTHERS //////////////////////////////////////

    private void ClampFallSpeed()
    {
        myRigidbody2D.velocity = new Vector2(
            myRigidbody2D.velocity.x,
            Mathf.Clamp(myRigidbody2D.velocity.y, minFallVelocity, Mathf.Infinity));
    }

    void UpdateAnimatorParameters()
    {
        myAnimator.SetBool("isOnGround", IsGrounded());
        myAnimator.SetFloat("yVelocity", myRigidbody2D.velocity.y);
        myAnimator.SetBool("isWallSliding", isWallSlidng);

        FlipSprite();
    }

    private void FlipSprite()
    {
        float localScaleX = 0f;

        if (isWallSlidng)
        {
            localScaleX = collisionChecker.isOnLeftWall ? 1f : -1f;
        }
        else
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            localScaleX = Mathf.Sign(mousePos.x - sprite.position.x);
        }

        sprite.localScale = new Vector2(localScaleX, 1f);
    }
}
