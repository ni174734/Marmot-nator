using UnityEngine;

public class PestControlKillZone : MonoBehaviour
{
    public float killRange = 1.0f;

    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (FightManager.Instance != null && FightManager.Instance.IsInvincible) return;
        if (player == null) return;

        if (Vector2.Distance(transform.position, player.position) <= killRange)
        {
            GameOverManager.Instance?.GameOver("Pest control got you!");
        }
    }
}