using UnityEngine;
using System.Collections;

public class TankModule : MonoBehaviour {
	
	[SerializeField]
	TankModule referenceModule = null;
		
	[SerializeField]
	int damage = 0, maxHP = 100;
		
	int currentHP;
	bool damaged = false;
	
	float totalHits = 0f;
	Color currentColour = Color.white;
	
	//====================================================================//
	
	public bool Damaged {
		get {
			return this.damaged;
		}
	}	
	
	//====================================================================//
	
	void Start()
	{
		Debug.Log (renderer.material);
		if(damage == 0)
		{
			Debug.Log("You didn't set up the damage for " + gameObject.name);	
		}
		
		totalHits = (maxHP / damage) / 255f;
		
		EnableModule();
	}
	
	//====================================================================//
	
	void DisableModule()
	{
		damaged = true;
	}
	
	//====================================================================//
	
	void DamageReferenceModule()
	{
		referenceModule.WasHit();	
	}
	
	//====================================================================//
	
	public void EnableModule()
	{
		if(renderer != null)
			renderer.material.SetColor("_Color", Color.white);
		
		currentHP = maxHP;
		damaged = false;
	}
	
	//====================================================================//
	
	public void WasHit()
	{
		TakeDamage(damage);
	}
	
	//====================================================================//
	
	public void TakeDamage(int damagePoints)
	{
		if(!damaged)
		{
			currentHP -= damagePoints;
		
			if(currentHP <= 0)
				DisableModule();
		
			if(renderer != null)
			{
				currentColour = Color.Lerp(Color.white, Color.red, 1f - (float)currentHP/maxHP);
				renderer.material.SetColor("_Color", currentColour);
			}
		}
		else if(referenceModule != null)
		{
			DamageReferenceModule();
		}
	}
	
	//====================================================================//
	/*
	void OnCollisionEnter(Collision col)
	{
		if(col.collider.CompareTag("Bullet"))
		{
			WasHit();	
		}
	}
	*/
	//====================================================================//
	
}
