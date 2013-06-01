using UnityEngine;
using System.Collections;

public class TankModule : MonoBehaviour {
	
	[SerializeField]
	TankModule referenceModule = null;
		
	[SerializeField]
	int damage = 0, maxHP = 100;
		
	int currentHP;
	bool destroyed = false;
	
	float totalHits = 0f;
	Color currentColour = Color.white;

	//Parameter is percentage of HP left
	public event System.Action<float> Damaged;
	
	//====================================================================//
	
	public bool Destroyed {
		get {
			return this.destroyed;
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
		destroyed = true;
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
		destroyed = false;
	}
	
	//====================================================================//
	
	public void WasHit()
	{
		TakeDamage(damage);
	}
	
	//====================================================================//
	
	public void TakeDamage(int damagePoints)
	{
		if(!destroyed)
		{
			Debug.Log ("Module taken damage: " + name + ". remaining HP: " + currentHP );

			currentHP -= damagePoints;
		
			if(currentHP <= 0)
				DisableModule();
		
			if(renderer != null)
			{
				currentColour = Color.Lerp(Color.white, Color.red, 1f - (float)currentHP/maxHP);
				renderer.material.SetColor("_Color", currentColour);
			}

			OnDamaged();
		}
		else if(referenceModule != null)
		{
			DamageReferenceModule();
		}
	}

	protected void OnDamaged() {
		if( Damaged != null ) {
			Damaged( (float)currentHP/maxHP );
		}
	}

	public void Detach() {
		Rigidbody r = gameObject.AddComponent<Rigidbody>();
		r.AddForce( 15f*Vector3.up + 15f*Random.onUnitSphere, ForceMode.Impulse );
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
