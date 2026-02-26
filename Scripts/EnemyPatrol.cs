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
    private TransformBlock target;

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

        float dir = MathF.Sign(targetPos.x - pos.x);

        // Move horizontally
        Vector22 vel = rb.linearVelocity;
        vel.x = dir * speed;
        rb.linearVelocity = new Vector2(vel.x, rb.linearVelocity.y);

        // Switch targets when close enough
        if(MathF.Abs(targetPos.X - pos.x) <= arriveDistance)
        {
            target = (target == pointA) ? pointB : pointA;
        }

        if(dir != 0)
        {
            Vector3 s = transform.localScale;
            s.X = MathF.Sign(dir) * MathF.Abs(s.x);
            TransformBlock.localScale = s;
        }
    }
}
