using UnityEngine;
using UnityEngine.UI;

public class FightManager : MonoBehaviour
{
    public static FightManager Instance;
	
	[Header("UI")]
	public GameObject fightUI;
	public Slider progressBar;
	
	[Header("Fight Settings")]
	[Tooltip("how long you have to win")]
	public float fightDuration = 3.0f;
	[Tooltip("bar full = win")]
	public float winThreshold = 1.0f;
	[Tooltip("per good input")]
	public float mashGain = 0.08f;
	[Tooltip("drains per second (adds pressure)")]
	public float passiveDrain = 0.18f;
	[Tooltip("stick x must exceed this to count")]
	public float wiggleThreshold = 0.18f;
	[Tooltip("prevents counting the same hold")]
	public float inputCooldown = 0.18f;
	
	[Header("Player Invincibility")]
	public float invincibleAfterWin = 1.0f;
	
	private bool active;
	private float timer;
	private float progress;
	
	private Transform playerT;
	private Rigidbody2D playerRB;
	private MonoBehaviour playerMoveScript;

	private Transform enemyT;
	private Rigidbody2D enemyRB;
	
	private PlayerControls controls;
	private int lastDir;
	private float lastInputTime;
	
	private System.Action onWin;
	private System.Action onLose;
	
	private float invincibleTimer;
	
	private void Awake()
	{
		if(Instance == null) 
			Instance = this;
		else
		{ 
			Destroy(gameObject); 
			return;
		}
		
		if(fightUI != null) fightUI.SetActive(false);
		
		controls = new PlayerControls();
	}
	
	void OnEnable()
	{
		controls.MainGame.Enable();
	}
	
	void OnDisable()
	{
		controls.MainGame.Disable();
	}
	
	private void Update()
	{
		if(invincibleTimer > 0f) 
			invincibleTimer -= Time.deltaTime;
		
		if(!active) 
			return;
		
		timer -= Time.deltaTime;
		
		progress = Mathf.Max(0f, progress - passiveDrain * Time.deltaTime);
		UpdateUI();
		
		HandleMashInput();
		
		if(progress >= winThreshold)
		{
			EndFight(true);
			return;
		}
		
		if(timer <= 0f)
		{
			EndFight(false);
			return;
		}
	}
	
	private void HandleMashInput()
	{
		Vector2 move = controls.MainGame.Move.ReadValue<Vector2>();
		
		int dir = 0;
		if(move.x > wiggleThreshold)
			dir = 1;
		else if(move.x < -wiggleThreshold)
			dir = -1;
		
		if(dir != 0)
		{
			if(Time.time - lastInputTime > inputCooldown && dir != lastDir)
			{
				lastDir = dir;
				lastInputTime = Time.time;
				
				progress = Mathf.Min(winThreshold, progress + mashGain);
				UpdateUI();
			}
		}
		
		if(controls.MainGame.Jump.WasPressedThisFrame())
		{
			progress = Mathf.Min(winThreshold, progress + mashGain);
			UpdateUI();
		}
		
		if(controls.MainGame.Eat.WasPressedThisFrame())
		{
			progress = Mathf.Min(winThreshold, progress + mashGain * 0.8f);
			UpdateUI();
		}
	}
	
	private void UpdateUI()
	{
		if(progressBar != null)
			progressBar.value = Mathf.Clamp01(progress / winThreshold);
	}
	
	public bool IsInvincible => invincibleTimer > 0f;
	public bool IsActive => active;
	
	public bool TryStartFight(Transform player, Rigidbody2D playerRb, MonoBehaviour playerControllerToDisable, 
	Transform enemy, Rigidbody2D enemyRb, System.Action winCallback, System.Action loseCallback)
	{
		if(active) return false;
		if(IsInvincible) return false;
		
		active = true;
		timer = fightDuration;
		progress = 0f;
		lastDir = 0;
		lastInputTime = 0f;
		
		playerT = player;
		playerRB = playerRb;
		playerMoveScript = playerControllerToDisable;
		
		enemyT = enemy;
		enemyRB = enemyRb;
		
		onWin = winCallback;
		onLose = loseCallback;
		
		LockActor(playerRB);
		LockActor(enemyRB);
		
		if(playerMoveScript != null)
			playerMoveScript.enabled = false;
		
		if(fightUI != null) 
			fightUI.SetActive(true);
		
		UpdateUI();
		
		return true;
	}
	
	private void EndFight(bool playerWon)
	{
		UnlockActor(playerRB);
		UnlockActor(enemyRB);
		if(playerMoveScript != null)
			playerMoveScript.enabled = true;
		
		active = false;
		
		if(fightUI != null)
			fightUI.SetActive(false);
		
		if(playerWon)
		{
			invincibleTimer = invincibleAfterWin;
			onWin?.Invoke();
		}
		else
		{
			onLose?.Invoke();
		}
		
		// cleanup refs
		playerT = null;
		playerRB = null;
		playerMoveScript = null;
		enemyT = null;
		enemyRB = null;
		onWin = null;
		onLose = null;
	}
	
	private void LockActor(Rigidbody2D rb)
	{
		if(rb == null) return;
		rb.linearVelocity = Vector2.zero;
		rb.angularVelocity = 0f;
		rb.constraints = RigidbodyConstraints2D.FreezeAll;
	}
	
	private void UnlockActor(Rigidbody2D rb)
	{
		if(rb == null) return;
		rb.constraints = RigidbodyConstraints2D.FreezeRotation;
	}
}
