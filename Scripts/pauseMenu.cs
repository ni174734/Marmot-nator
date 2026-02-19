using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class pauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
	public GameObject firstSelected; // Button to select (e.g., Resume)
    public PlayerInput playerInput; // Assign in Inspector
    public bool isPaused = false;
	
    public void togglePause()
    {	
        if (isPaused)
		{
			Resume();
		}
		else 
		{
			Pause();
		}
    }

    public void Resume() 
    {
        pauseMenuUI.SetActive(false);
		isPaused = false;
        Time.timeScale = 1f;
		playerInput.SwitchCurrentActionMap("MainGame");
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
		isPaused = true;
		playerInput.SwitchCurrentActionMap("UI");
        Time.timeScale = 0f;
		EventSystem.current.SetSelectedGameObject(null);
		EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    public void quitGame() 
    {
        SceneManager.LoadSceneAsync(0);
        Time.timeScale = 1;
    }
}
