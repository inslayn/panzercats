using UnityEngine;
using System.Collections;

public class TankModule : MonoBehaviour {
	
	[SerializeField]
	TankModule referenceModule = null;
		
	[SerializeField]
	int maxHP = 1;

	int currentHP = 1;
		
	public bool Destroyed {
		get {
			return this.currentHP == 0;
		}
	}

	Color currentColour = Color.white;

	//Parameter is percentage of HP left
	public event System.Action<float> Damaged;

	//====================================================================//
	
	void Start()
	{
		currentHP = maxHP;
		if(renderer != null)
			renderer.material.SetColor("_Color", Color.white);
		EnableModule();
	}
	
	//====================================================================//
	
	void DisableModule()
	{

	}

	//====================================================================//
	
	public void EnableModule()
	{


	}
	
	//====================================================================//
	
	public void TakeDamage(int damagePoints)
	{
		if(!Destroyed)
		{


			currentHP -= damagePoints;
		
			OnDamaged();

			if( Destroyed ) {
				DisableModule();
			}
		}
		else if(referenceModule != null)
		{
			referenceModule.TakeDamage(damagePoints);
		}
	}

	void OnSerializeNetworkView( BitStream stream, NetworkMessageInfo info ) {
		if( stream.isWriting ) {
			stream.Serialize( ref currentHP );
		} else {
			stream.Serialize( ref currentHP );
			OnDamaged();
		}
	}

	protected void OnDamaged() {
		Debug.Log ("Module HP: " + name + " = " + currentHP );

		if(currentHP <= 0)
			DisableModule();
		
		if(renderer != null)
		{
			currentColour = Color.Lerp(Color.white, Color.red, 1f - (float)currentHP/maxHP);
			renderer.material.SetColor("_Color", currentColour);
		}

		if( Damaged != null ) {
			Damaged( (float)currentHP/maxHP );
		}
	}

	public void Detach() {
		transform.parent = null;
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
