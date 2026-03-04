using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    public GameObject gameOverUI; // optional

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    public void GameOver(string reason = "")
    {
        Debug.Log("GAME OVER: " + reason);

        if (gameOverUI != null) gameOverUI.SetActive(true);

        // simplest: reload scene after a delay
        // Invoke(nameof(Reload), 1.5f);
        Time.timeScale = 0f;
    }

    private void Reload()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
