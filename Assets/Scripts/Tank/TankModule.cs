using UnityEngine;
using System.Collections;

public class TankModule : MonoBehaviour {
	
	[SerializeField]
	TankModule referenceModule = null;
		
	[SerializeField]
	int damage = 0, maxHP = 100;
		
	int currentHP;
	bool damaged = false;
	
	//====================================================================//
	
	public bool Damaged {
		get {
			return this.damaged;
		}
	}	
	
	//====================================================================//
	
	void Start()
	{
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
		}
		else if(referenceModule != null)
		{
			DamageReferenceModule();
		}
	}
	
	//====================================================================//
	
	void OnCollisionEnter(Collision col)
	{
		if(col.gameObject.CompareTag("bullet"))
		{
			WasHit();	
		}
	}
	
	//====================================================================//
	
}
