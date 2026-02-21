using UnityEngine;
using UnityEngine.InputSystem;

public class playerController : MonoBehaviour
{
    private PlayerControls playerControls;
    public pauseMenu pause;

    private Rigidbody2D rb;

    [Header("Move")]
	[Tooltip("Max player speed.")]
    [SerializeField] private float maxSpeed = 8f;
	[Tooltip("How fast you reach max speed.")]
    [SerializeField] private float accel = 60f;         // how fast you reach max speed
	[Tooltip("How fast you stop.")]
    [SerializeField] private float decel = 70f;         // how fast you stop
	[Tooltip("if x -> 0, less control in the air.")]
    [SerializeField] private float airAccel = 35f;      // less control in air
    [SerializeField] private float airDecel = 35f;

    [Header("Jump")]
	[Tooltip("How high you jump.")]
    [SerializeField] private float jumpForce = 14f;
	[Tooltip("Seconds after leaving ground you can still jump.")]
    [SerializeField] private float coyoteTime = 0.12f;      // seconds after leaving ground you can still jump
	[Tooltip("Seconds before landing a jump press is stored.")]
    [SerializeField] private float jumpBufferTime = 0.12f;  // seconds before landing a jump press is stored
	[Tooltip("Release jump early -> shorter jump.")]
    [SerializeField] private float jumpCutMultiplier = 0.5f; // release jump early -> shorter jump

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.1f);
    [SerializeField] private LayerMask groundLayer;
	
	[Header("Sprint")]
	[SerializeField] private float sprintMultiplier = 1.6f;
	[SerializeField] private float doubleTapTime = 0.25f;
	[SerializeField] private float tapThreshold = 0.7f;   // how far stick must be pushed to count as a "tap"

	private bool isSprinting;
	private int sprintDir;            // -1 = left, +1 = right
	private int prevDir;              // last frame's direction bucket
	private float lastTapTimeRight;
	private float lastTapTimeLeft;
	private int lastTapDirection; // -1 left, 1 right

    [Header("Visual (Optional)")]
    [SerializeField] private bool flipSprite = true;

    private float moveInput;               // -1..1
    private bool jumpHeld;
    private float coyoteTimer;
    private float jumpBufferTimer;

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
    }

    void Awake()
    {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()  => playerControls.MainGame.Enable();
    void OnDisable() => playerControls.MainGame.Disable();

    // Input System callback (Action: Move [Vector2])
    private void OnMove(InputValue inputValue)
    {
        moveInput = inputValue.Get<Vector2>().x; // horizontal only
		
		int direction = 0;
		if(moveInput > tapThreshold){ direction = 1; }
		else if(moveInput < -tapThreshold){ direction = -1; }
		
		bool isNewTap = (prevDir == 0 && direction != 0);
		
		if(isNewTap)
		{
			if(direction == 1)
			{
				if(Time.time - lastTapTimeRight <= doubleTapTime)
				{ 
					isSprinting = true;
					sprintDir = 1;
				}
				lastTapTimeRight = Time.time;
			}
			else //if()
			{
				if(Time.time - lastTapTimeLeft <= doubleTapTime)
				{ 
					isSprinting = true;
					sprintDir = -1;
				}
				lastTapTimeLeft = Time.time;
			}
		}
		
		// Stop sprint if you let go OR you push the opposite direction
		if(direction == 0 || (isSprinting && direction != sprintDir)) 
		{
			isSprinting = false;
			sprintDir = 0;
		}
		
		prevDir = direction;
    }

    // Input System callback (Action: Jump [Button])
    private void OnJump(InputValue inputValue)
    {
        // treat this as "pressed or held"
        float v = inputValue.Get<float>(); // 1 when pressed/held, 0 when released
        bool pressed = v > 0.5f;

        if (pressed)
        {
            jumpHeld = true;
            jumpBufferTimer = jumpBufferTime; // store jump input
        }
        else
        {
            jumpHeld = false;

            // variable jump: if we're moving up and release jump, cut it
            if (rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            }
        }
    }

    private void OnPause()
    {
        pause.togglePause();
    }

    void Update()
    {
        // update timers
        if (IsGrounded())
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        jumpBufferTimer -= Time.deltaTime;

        // If we have buffered jump + we're allowed to jump (grounded or coyote)
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            DoJump();
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        // Optional: flip sprite based on movement
        if (flipSprite && Mathf.Abs(moveInput) > 0.01f)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Sign(moveInput) * Mathf.Abs(s.x);
            transform.localScale = s;
        }
    }

    void FixedUpdate()
    {
        // choose accel/decel based on grounded
        bool grounded = IsGrounded();
        float a = grounded ? accel : airAccel;
        float d = grounded ? decel : airDecel;

		float speed = isSprinting ? maxSpeed * sprintMultiplier : maxSpeed;
        float targetSpeed = moveInput * speed;
		
        float speedDiff = targetSpeed - rb.linearVelocity.x;

        // accelerate when input is present, otherwise decelerate to 0
        float rate = (Mathf.Abs(targetSpeed) > 0.01f) ? a : d;

        float movement = speedDiff * rate;
        rb.AddForce(new Vector2(movement, 0f), ForceMode2D.Force);

        // clamp max speed
        float clampedX = Mathf.Clamp(rb.linearVelocity.x, -speed, speed);
        rb.linearVelocity = new Vector2(clampedX, rb.linearVelocity.y);
    }

    private void DoJump()
    {
        // reset vertical velocity so jump is consistent (no tiny hops if falling)
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
#endif
}
