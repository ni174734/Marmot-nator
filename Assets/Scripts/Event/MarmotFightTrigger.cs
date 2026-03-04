using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MarmotFightTrigger : MonoBehaviour
{
    public float fightRange = 1.1f;
    public float knockbackForce = 7f;
    public float sendBackDistance = 4f;

    private Rigidbody2D rb;
    private Transform player;
    private Rigidbody2D playerRB;
    private PlayerInventory inv;
    private MonoBehaviour playerController;

    private Vector3 homePos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        homePos = transform.position;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            playerRB = player.GetComponent<Rigidbody2D>();
            inv = player.GetComponent<PlayerInventory>();
            playerController = player.GetComponent<playerController>(); // your movement script name
        }
    }

    private void Update()
    {
        if (FightManager.Instance == null) return;
        if (FightManager.Instance.IsActive || FightManager.Instance.IsInvincible) return;
        if (player == null || inv == null) return;

        // Marmot only fights if player has food/item
        if (!inv.HasItem) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > fightRange) return;

        FightManager.Instance.TryStartFight(
            player, playerRB, playerController,
            transform, rb,
            OnPlayerWin,
            OnPlayerLose
        );
    }

    private void OnPlayerWin()
    {
        // send marmot back (simple: teleport back toward home)
        transform.position = homePos;
    }

    private void OnPlayerLose()
    {
        // knock player back away from marmot
        if (playerRB != null)
        {
            Vector2 dir = ((Vector2)(player.position - transform.position)).normalized;
            playerRB.AddForce(new Vector2(dir.x, 0.6f).normalized * knockbackForce, ForceMode2D.Impulse);
        }

        // steal and "eat" (destroy) the held item
        GameObject stolen = inv.ConsumeHeldItemObject();
        if (stolen != null) Destroy(stolen);

        // marmot goes back (or keep chasing — your call)
        transform.position = homePos;
    }
}
