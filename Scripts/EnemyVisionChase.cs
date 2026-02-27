using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyVisionChase : MonoBehaviour
{
    public enum State 
	{
		Patrol,
		Chase
	}
	
	public State state = State.Patrol;
	
	[Header("Patrol Points")]
	public Transform pointA;
    public Transform pointB;
    public float patrolSpeed = 2.5f;
	public float arriveDistance = 0.15f;
	
	[Header("Chase")]
	public Transform player;
    public float chaseSpeed = 4.5f;
	public float losePlayerTime = 1.0f;
	
	[Header("Vision")]
	public float viewDistance = 0.15f;
    [Range(0, 180)] public float viewAngle = 60f;
	public LayerMask playerMask;
    public LayerMask obstacleMask;
	
	[Header("Tuning")]
    public float stickDeadzone = 0.05f;
	
	private Rigidbody2D rb;
	private Transform patrolTarget;
	private float lastSeenTimer;
	
	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
        patrolTarget = pointB;
        lastSeenTimer = 999f;
	}
	
	void FixedUpdate()
	{
		if(!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
		
		bool canSee = (player != null) && CanSeePlayer();
		
		if(canSee)
		{
			state = State.Chase;
			lastSeenTimer = 0f;
		}
		else
		{
			lastSeenTimer += Time.fixedDeltaTime;
			if(state == State.Chase && lastSeenTimer >= losePlayerTime)
				state = State.Patrol;
		}
		
		if(state == State.Patrol) DoPatrol();
		else DoChase();
	}
	
	void DoPatrol()
	{
		if(!pointA || !pointB) return;
		
		Vector2 pos = rb.position;
		Vector2 targetPos = patrolTarget.position;

		float dx = targetPos.x - pos.x;
		float dir = Mathf.Sign(dx);
		
		rb.linearVelocity = new Vector2(dir * patrolSpeed, rb.linearVelocity.y);
		
		if(Mathf.Abs(dx) <= arriveDistance)
			patrolTarget = (patrolTarget == pointA) ? pointB : pointA;
		
		Face(dir);
	}
	
	void DoChase()
	{
		if(!player) return;
		
		float dx = player.position.x - transform.position.x;
		float dir = 0f;
		
		if(Mathf.Abs(dx) > stickDeadzone)
			dir = Mathf.Sign(dx);
		
		rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);
		
		Face(dir);
	}
	
	void Face(float dir)
	{
		if(dir == 0) return;
		Vector3 s = transform.localScale;
		s.x = Mathf.Sign(dir) * Mathf.Abs(s.x);
		transform.localScale = s;
	}
	
	bool CanSeePlayer()
	{
		Vector2 origin = rb.position;
        Vector2 toPlayer = (Vector2)player.position - origin;

        // Distance
        if (toPlayer.magnitude > viewDistance) return false;

        // Angle cone
        Vector2 forward = new Vector2(Mathf.Sign(transform.localScale.x), 0f);
        float angle = Vector2.Angle(forward, toPlayer);
        if (angle > viewAngle * 0.5f) return false;

        // Optional: quick player-layer overlap check (not required but helps)
        // if (!Physics2D.OverlapCircle(origin, viewDistance, playerMask)) return false;

        // Line of sight raycast (blocks on obstacles)
        RaycastHit2D hit = Physics2D.Raycast(origin, toPlayer.normalized, viewDistance, obstacleMask | playerMask);
        if (!hit) return false;

        // If the first thing hit is player, we see them
        return ((1 << hit.collider.gameObject.layer) & playerMask) != 0;
	}
	
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Draw view distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        // Draw cone lines (approx)
        Vector3 origin = transform.position;
        Vector3 forward = new Vector3(Mathf.Sign(transform.localScale.x == 0 ? 1 : transform.localScale.x), 0, 0);

        Quaternion leftRot = Quaternion.Euler(0, 0, viewAngle * 0.5f);
        Quaternion rightRot = Quaternion.Euler(0, 0, -viewAngle * 0.5f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + (leftRot * forward) * viewDistance);
        Gizmos.DrawLine(origin, origin + (rightRot * forward) * viewDistance);
    }
#endif
}
