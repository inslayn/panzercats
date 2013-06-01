using UnityEngine;
using System.Collections;

public class Tank : MonoBehaviour {
	
	[SerializeField]
	ParticleSystem dustParticle = null;
	
	[SerializeField]
	GameObject bulletPrefab;

	[SerializeField]
	Camera tankCamera = null;

	[SerializeField]
	Transform turretTransform = null, cannonTransform = null, bulletSpawnTransform = null;

	[SerializeField]
	Collider cannonCollider = null;

	[SerializeField]
	TankModule commandModule = null;

	Transform cachedTransform;
	
	float speed = 2f, rotationSpeed = 200f, vertical, horizontal, health = 100f;

	float fireCooldownTime;

	//----------------------------------------------------------------------------------------
	
	void Start()	
	{
		//enabled = networkView.isMine;
		tankCamera.enabled = networkView.isMine;

		cachedTransform = transform;
		
		InvokeRepeating("CheckParticles", 0.1f, 1f);

		lastMousePosition = Input.mousePosition;

		commandModule.Damaged += HandleDamaged;
	}

	void HandleDamaged( float percentageHealth ) {
		if( percentageHealth <= 0f ) {
			isDead = true;
			TankModule[] modules = GetComponentsInChildren<TankModule>();
			foreach( TankModule m in modules ) {
				if( m != commandModule ) {
					m.Detach();
				}
			}
		}
	}
	//----------------------------------------------------------------------------------------

	void FixedUpdate() {

		if(!isDead && networkView.isMine)
		{
			vertical = Input.GetAxis("Vertical"); 
			horizontal = Input.GetAxis("Horizontal");
			
			Vector3 thrustVector = cachedTransform.forward;
			thrustVector.y = 0f;
			
			float forwardThrottle = vertical;
			float leftThrottle = horizontal+vertical;
			float rightThrottle = -horizontal+vertical;
			
			float treadDifference = leftThrottle-rightThrottle;
			
			rigidbody.AddTorque( 0, 10f*(treadDifference * rotationSpeed)/(1f+3f*rigidbody.velocity.magnitude), 0 );
			rigidbody.AddForce( 100f*(leftThrottle+rightThrottle)*thrustVector );

		}
	}

	Vector3 lastMousePosition;
	//Vector3 targetTurretAngles, targetCannonAngles;

	void Update() {
		if(!isDead && networkView.isMine)
		{
			if( Time.time > fireCooldownTime && Input.GetMouseButtonDown(0)) {
				fireCooldownTime = Time.time + 2f;
				StartCoroutine(FireBullet());
			}

			Vector3 mouseDelta = Input.mousePosition-lastMousePosition;
			lastMousePosition = Input.mousePosition;

			turretTransform.Rotate( -mouseDelta.x*30f*Time.deltaTime, 0f, 0f );
			cannonTransform.Rotate( 0f, mouseDelta.y*30f*Time.deltaTime, 0f );

			Vector3 turretAngles = cannonTransform.localEulerAngles;
			turretAngles.y = Mathf.Clamp( turretAngles.y, 270f-15f, 270f+15f );
			cannonTransform.localEulerAngles = turretAngles;

		}
	}

	
	//----------------------------------------------------------------------------------------
	
	IEnumerator FireBullet()
	{
		GameObject bullet = (GameObject)Network.Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.identity, 0);

		Physics.IgnoreCollision(bullet.collider, cannonCollider );
		
		bullet.transform.parent = transform.parent;

		Vector3 spawnPointVelocity = rigidbody.GetPointVelocity( bulletSpawnTransform.position );
		bullet.rigidbody.velocity = spawnPointVelocity;
		bullet.rigidbody.AddForce(bulletSpawnTransform.forward * 40f, ForceMode.Impulse);

		yield return new WaitForSeconds(5f);
		
		Network.Destroy(bullet.GetComponent<NetworkView>().viewID);
	}
	
	//----------------------------------------------------------------------------------------
	
	void ResetBullet(GameObject bullet)
	{
//		bullet.SetActive(false);
//		
//		bullet.transform.position = turrentTransform.position;
//			
//		bullet.rigidbody.velocity = Vector3.zero;
//		
//		bullet.rigidbody.angularVelocity = Vector3.zero;
//		
//		ObjectPooler.Instance.BulletPool.Add(bullet);
	}
	
	//----------------------------------------------------------------------------------------
	
	void CheckParticles()
	{
		if(vertical != 0)
			dustParticle.Play();
		else if(vertical == 0 && dustParticle.isPlaying)
			dustParticle.Stop();
	}
	
	//----------------------------------------------------------------------------------------
	
	void OnCollisionEnter(Collision col)
	{		
		if(col.collider.CompareTag("Bullet"))
		{
			foreach(ContactPoint c in col.contacts) {
				if( c.thisCollider ) {
					TankModule m = c.thisCollider.GetComponent<TankModule>();
					if( m ) {
						m.WasHit();
						}
					}
				}

		}
			//networkView.RPC("TakeDamage", RPCMode.Others, 5f);
		//	TakeDamage(5f);
	}
	
	//----------------------------------------------------------------------------------------
	
	void TakeDamage(float amount)
	{
		Mathf.Clamp(health -= amount, 0f, 100f);
		
		if(health <= 0)
			isDead = true;
	}
	
	bool isDead;
}
