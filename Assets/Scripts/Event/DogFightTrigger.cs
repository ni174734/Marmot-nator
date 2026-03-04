using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DogFightTrigger : MonoBehaviour
{
    public float fightRange = 1.2f;
    public float pushBackForce = 9f;

    private Rigidbody2D rb;
    private Transform player;
    private Rigidbody2D playerRB;
    private MonoBehaviour playerController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            playerRB = player.GetComponent<Rigidbody2D>();
            playerController = player.GetComponent<playerController>();
        }
    }

    private void Update()
    {
        if (FightManager.Instance == null) return;
        if (FightManager.Instance.IsActive || FightManager.Instance.IsInvincible) return;
        if (player == null) return;

        if (Vector2.Distance(transform.position, player.position) > fightRange) return;

        FightManager.Instance.TryStartFight(
            player, playerRB, playerController,
            transform, rb,
            OnPlayerWin,
            OnPlayerLose
        );
    }

    private void OnPlayerWin()
    {
        // push dog away from player
        Vector2 dir = ((Vector2)(transform.position - player.position)).normalized;
        rb.AddForce(new Vector2(dir.x, 0.4f).normalized * pushBackForce, ForceMode2D.Impulse);
    }

    private void OnPlayerLose()
    {
        GameOverManager.Instance?.GameOver("Caught by dog!");
    }
}