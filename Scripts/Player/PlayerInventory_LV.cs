using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
    [Header("Hold")]
	[Tooltip("Empty child transform on player.")]
	[SerializeField] private Transform holdPoint;
	
	[Header("Drop/Throw")]
	[Tooltip("Throw strength")]
	[SerializeField] private float throwForce = 7f;
	[SerializeField] private Vector2 throwDir = new Vector2(1f, 0.35f);
	
	[Header("Pickup Settings")]
	[Tooltip("Time delay for picking up food.")]
	[SerializeField] private float pickupDelay = 0.4f;

	private float pickupTimer;
	
	private PlayerControls controls;
	
	private Pickup heldItem;
	private Rigidbody2D heldRB;
	private Collider2D heldCol;
	
	private void Awake() 
	{
		controls = new PlayerControls();
	}
	
	private void Update()
	{
		if(pickupTimer > 0f)
			pickupTimer -= Time.deltaTime;
	}
	
	/*
	private void OnEnable()
	{
		controls.MainGame.Enable();
		controls.MainGame.Drop.performed += OnDrop;
		controls.MainGame.Eat.performed += OnEat;
	}
	
	private void OnDisable()
	{
		controls.MainGame.Drop.performed -= OnDrop;
		controls.MainGame.Eat.performed -= OnEat;
		controls.MainGame.Disable();
	}
	*/
	
	public bool TryPickup(Pickup item)
	{
		if(heldItem != null) return false;
		if(pickupTimer > 0f) return false;
		
		heldItem = item;
		heldRB = item.GetComponent<Rigidbody2D>();
		heldCol = item.GetComponent<Collider2D>();
		
		if(heldRB != null)
		{
			heldRB.linearVelocity = Vector2.zero;
			heldRB.angularVelocity = 0f;
			heldRB.simulated = false;
		}
		
		if(heldCol != null)
			heldCol.enabled = false;
		
		if(holdPoint != null)
		{
			item.transform.SetParent(holdPoint);
			item.transform.localPosition = Vector3.zero;
			item.transform.localRotation = Quaternion.identity;
		}
		else
		{
			item.transform.SetParent(transform);
		}
		
		return true;
	}
	
	private void OnEat(InputValue v)
	{
		if(v.Get<float>() <= 0.5f) return; // only on press
		if(heldItem == null) return;
		
		Destroy(heldItem.gameObject);
		ClearHeld();
	}
	
	private void OnDrop(InputValue v)
	{
		if(v.Get<float>() <= 0.5f) return; // only on press
		if(heldItem == null) return;
		
		Transform itemT = heldItem.transform;
		itemT.SetParent(null);
		
		if(heldCol != null) heldCol.enabled = true;
		if(heldCol == null) 
		{
			ClearHeld();
			return;
		}
		
		heldRB.simulated = true;
			
		float facing = Mathf.Sign(transform.localScale.x);
		if(facing == 0) facing = 1;
		
		Vector2 dir = new Vector2(facing * throwDir.x, throwDir.y).normalized;
		
		heldRB.linearVelocity = Vector2.zero;
		heldRB.AddForce(dir * throwForce, ForceMode2D.Impulse);
		
		pickupTimer = pickupDelay;
		ClearHeld();
	}
	
	private void ClearHeld()
	{
		heldItem = null;
		heldRB = null;
		heldCol = null;
	}
}
