using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Pickup : MonoBehaviour
{
	private void Reset()
	{
		var col = GetComponent<Collider2D>();
		col.isTrigger = true;
	}
	
	// This function is called when another collider enters the trigger collider attached to this GameObject
	private void OnTriggerEnter2D(Collider2D collision)
	{
		PlayerInventory inv = collision.GetComponent<PlayerInventory>();
		if(inv == null) return;
		
		if(inv.TryPickup(this)){}
	}
}
