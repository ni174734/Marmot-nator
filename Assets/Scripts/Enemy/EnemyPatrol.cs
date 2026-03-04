using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol points")]
    public Transform pointA;
    public Transform pointB;
    
    [Header("Movement")]
    public float speed = 3f;
    public float arriveDistance = 0.1f;

    private Rigidbody2D rb;
    private Transform target;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        target = pointB;
    }
    
    void FixedUpdate()
    {
        if(!pointA || !pointB) return;

        Vector2 pos = rb.position;
        Vector2 targetPos = target.position;

        float dir = Mathf.Sign(targetPos.x - pos.x);

        // Move horizontally
        Vector2 vel = rb.linearVelocity;
        vel.x = dir * speed;
        rb.linearVelocity = new Vector2(vel.x, rb.linearVelocity.y);

        // Switch targets when close enough
        if(Mathf.Abs(targetPos.x - pos.x) <= arriveDistance)
        {
            target = (target == pointA) ? pointB : pointA;
        }

        if(dir != 0)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Sign(dir) * Mathf.Abs(s.x);
            transform.localScale = s;
        }
    }
}
