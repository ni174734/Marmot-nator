using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwapper : MonoBehaviour
{
    [SerializeField] private string sceneName;
	
	private void OnTriggerEnter2D(Collider2D collision)
	{
		playerController player = collision.gameObject.GetComponent<playerController>();
		if(player)
			SceneManager.LoadScene(sceneName);
	}
}
