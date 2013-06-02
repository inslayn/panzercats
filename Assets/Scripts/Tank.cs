using UnityEngine;
using System.Collections;

public class Tank : MonoBehaviour {
	
	public enum CameraView
	{
		firstPerson,
		thirdPerson
	};
	
	CameraView currentCameraView;
	
	[SerializeField]
	ParticleSystem dustParticle = null;
	
	[SerializeField]
	GameObject bulletPrefab;

	[SerializeField]
    GameObject[] disableIfNotMine = null;

	[SerializeField]
	Transform turretTransform = null, cannonTransform = null, bulletSpawnTransform = null, catTransform = null, cameraTransform = null;

	[SerializeField]
	Collider cannonCollider = null;

	[SerializeField]
	TankModule commandModule = null;

	[SerializeField]
	TankModule leftTread = null, rightTread = null, engine = null, turret = null, cannon = null;
	
	Vector3 origCamPos;

	[SerializeField]
	ParticleSystem explosionParticles = null, cannonFireParticles = null;

	Transform cachedTransform;
	
	float speed = 2f, rotationSpeed = 200f, vertical, horizontal;

	float fireCooldownTime;
	
	bool isInCockpit, isDead;

	bool isLocalPlayer = false;

	public bool IsLocalPlayer {
		get {
			return this.isLocalPlayer;
		}
		set {
			isLocalPlayer = value;
		}
	}

	//Player joined the game
	public event System.Action<int> Joined;

	//----------------------------------------------------------------------------------------
	
	void Start()	
	{
		//enabled = networkView.isMine;
		Debug.Log("networkView.isMine = " + networkView.isMine );

		if( !networkView.isMine ) {

			foreach( GameObject g in disableIfNotMine ) {
				Debug.Log("DISABLE: " + g );
				g.SetActive(false);
			}
		}

		cachedTransform = transform;
		
		origCamPos = cameraTransform.localPosition;
		
		InvokeRepeating("CheckParticles", 0.1f, 1f);

		lastMousePosition = Input.mousePosition;

		commandModule.Damaged += HandleDamaged;
	}
	
	//----------------------------------------------------------------------------------------
	
	void HandleDamaged( float percentageHealth ) {
		if( !isDead && percentageHealth <= 0f ) {
			isDead = true;
			TankModule[] modules = GetComponentsInChildren<TankModule>();
			ParticleSystem p = (ParticleSystem)Instantiate( explosionParticles, transform.position, Quaternion.identity );
			Destroy( p.gameObject, 5f );
			foreach( TankModule m in modules ) {
				if( m != commandModule ) {
					m.Detach();
					Invoke ("NetworkDestroy",10f);
				}
			}
		}
	}
	
	//----------------------------------------------------------------------------------------
	
	void NetworkDestroy() {
		if( Network.isServer ) {
			Network.Destroy( gameObject);
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

			if( leftTread.Destroyed ) {
				leftThrottle = 0f;
			}
			if( rightTread.Destroyed ) {
				rightThrottle = 0f;
			}
			if( engine.Destroyed ) {
				leftThrottle = 0f;
				rightThrottle = 0f;
			}

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
				networkView.RPC("FireBullet",RPCMode.All);
			}

			Vector3 mouseDelta = Input.mousePosition-lastMousePosition;
			lastMousePosition = Input.mousePosition;

			turretTransform.Rotate( -mouseDelta.x*30f*Time.deltaTime, 0f, 0f );
			cannonTransform.Rotate( 0f, mouseDelta.y*30f*Time.deltaTime, 0f );

			Vector3 turretAngles = cannonTransform.localEulerAngles;
			turretAngles.y = Mathf.Clamp( turretAngles.y, 270f-15f, 270f+15f );
			cannonTransform.localEulerAngles = turretAngles;
		
			// Swap 1st person and 3rd person perspectives
			
			if(Input.GetKeyDown(KeyCode.Alpha1))
			{
				currentCameraView = CameraView.firstPerson;
				cameraTransform.localPosition = origCamPos;
				cameraTransform.localRotation = Quaternion.identity;
			}
			else if(Input.GetKeyDown(KeyCode.Alpha2))
			{
				if(currentCameraView != CameraView.thirdPerson)
					cameraTransform.Rotate(new Vector3(15f, 0f, 0f));
				
				currentCameraView = CameraView.thirdPerson;
				cameraTransform.localPosition = new Vector3(0f, 1.43f, -8.17f);
			}
			
			// Move cat up and down
			
			if(Input.GetMouseButtonDown(1) && currentCameraView == CameraView.firstPerson)
			{
				float moveToPos = 0f;
				
				if(isInCockpit)
				{
					isInCockpit = false;
					moveToPos = 1f;
				}
				else
				{
					isInCockpit = true;
					moveToPos = -1f;
				}
		
				iTween.MoveAdd(catTransform.gameObject, iTween.Hash("y", moveToPos, "time", 1f));
			}
		}
	}

	
	//----------------------------------------------------------------------------------------

	[RPC]
	void FireBullet()
	{
		if( networkView.isMine ) {
			GameObject bullet = (GameObject)Network.Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.identity, 0);

			Physics.IgnoreCollision(bullet.collider, cannonCollider );
			
			bullet.transform.parent = transform.parent;

			Vector3 spawnPointVelocity = rigidbody.GetPointVelocity( bulletSpawnTransform.position );
			bullet.rigidbody.velocity = spawnPointVelocity;
			bullet.rigidbody.AddForce(bulletSpawnTransform.forward * 80f, ForceMode.Impulse);
		}

		ParticleSystem p = (ParticleSystem)Instantiate( cannonFireParticles, bulletSpawnTransform.position, Quaternion.FromToRotation( Vector3.forward, bulletSpawnTransform.forward ) );
		Destroy( p.gameObject, 2f );

	//	yield return new WaitForSeconds(5f);
		

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
		if( Network.isServer ) {
			if(col.collider.CompareTag("Bullet"))
			{
				foreach(ContactPoint c in col.contacts) {
					if( c.thisCollider ) {
						TankModule m = c.thisCollider.GetComponent<TankModule>();
						if( m ) {
							m.networkView.RPC( "TakeDamage", RPCMode.All, new object[]{1} );
						}
					}
				}

			}
		}
	}
}
