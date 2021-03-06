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
	Transform turretTransform = null, cannonTransform = null, bulletSpawnTransform = null, catTransform = null, cameraTransform = null, hatchTransform = null, cannonRecoilTransform = null;

	[SerializeField]
	Collider cannonCollider = null;

	[SerializeField]
	TankModule commandModule = null;

	[SerializeField]
	TankModule leftTread = null, rightTread = null, engine = null, turret = null, cannon = null;
	
	Vector3 origCamPos;

	[SerializeField]
	ParticleSystem explosionParticles = null, cannonFireParticles = null;

	[SerializeField]
	AudioSource engineAudioSource = null, reloadAudioSource = null;
	
	[SerializeField]
	Material tankMaterial = null;
	
	Transform cachedTransform;
	
	float speed = 2f, rotationSpeed = 200f, vertical, horizontal;

	float fireCooldownTime;
	
	bool isInCockpit = true, isDead;

	//Player joined the game
	public event System.Action Died;

    [SerializeField]
    GameObject riftCamera;

    [SerializeField]
    Camera mouseCamera;

	//----------------------------------------------------------------------------------------
	
	void Start()	
	{
		//enabled = networkView.isMine;
		Debug.Log("networkView.isMine = " + networkView.isMine );

        if(networkView.isMine) {
            if(!OVRDevice.IsHMDPresent()) {
                riftCamera.SetActive(false);
                mouseCamera.enabled = true;
            }
        }
        else {
            foreach(GameObject g in disableIfNotMine) {
                Debug.Log("DISABLE: " + g);
                g.SetActive(false);
            }
        }

		Screen.lockCursor = true;

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
				}
			}
			OnDied();
			Invoke ("NetworkDestroy",10f);
		}
	}
	
	//----------------------------------------------------------------------------------------
	
	void NetworkDestroy() {
		if( networkView.isMine ) {
			Network.Destroy(gameObject);
		}
	}
	
	void OnDied()
	{
		if(Died != null)	
			Died();
	}
	
	//----------------------------------------------------------------------------------------
	
	public void SetTankTexture(Texture newTexture)
	{
		tankMaterial.SetTexture("_MainTex", newTexture);	
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

			float engineIntensity = Mathf.Abs(leftThrottle)+Mathf.Abs(rightThrottle);
			engineAudioSource.pitch = 1f + 0.5f*engineIntensity;
			engineAudioSource.volume = Mathf.Clamp01( 0.5f + engineIntensity );

			if( leftTread.Destroyed ) {
				leftThrottle = 0f;
			}
			if( rightTread.Destroyed ) {
				rightThrottle = 0f;
			}
			if( engine.Destroyed ) {
				leftThrottle = 0f;
				rightThrottle = 0f;
				engineAudioSource.volume = Mathf.Lerp( engineAudioSource.volume, 0f, 0.1f*Time.deltaTime );
				engineAudioSource.pitch = Mathf.Lerp( engineAudioSource.pitch, 0f, 0.1f*Time.deltaTime );
			}


			float treadDifference = leftThrottle-rightThrottle;
			
			rigidbody.AddTorque( 0, 10f*(treadDifference * rotationSpeed)/(1f+3f*rigidbody.velocity.magnitude), 0 );
			rigidbody.AddForce( 100f*(leftThrottle+rightThrottle)*thrustVector );

		}
	}

	Vector3 lastMousePosition;
	//Vector3 targetTurretAngles, targetCannonAngles;
	
	float rotationY = 0f, rotationX = 0f;
	float joystickSpeed = 2000f, joystickClamp = 300f;
	
	void Update() {
		if(!isDead && networkView.isMine)
		{
			if( Time.time > fireCooldownTime && (Input.GetMouseButtonDown(0) || (Input.GetAxis("Fire2") == -1))) {
				fireCooldownTime = Time.time + 2f;
				networkView.RPC("FireBullet",RPCMode.All);
			}
			
			// Right analog stick
			float Xon = Mathf.Abs (Input.GetAxis("Joystick X"));
			float Yon = Mathf.Abs (Input.GetAxis("Joystick Y"));
			
			if (Yon > 0.05f)
			{
				rotationY += Input.GetAxis("Joystick Y") * joystickSpeed;
			}
			if (Xon > 0.05f)
			{
				rotationX += Input.GetAxis("Joystick X") * joystickSpeed;
			}
			
			rotationY = Mathf.Clamp(rotationY, -joystickClamp, joystickClamp);
 			rotationX = Mathf.Clamp(rotationX, -joystickClamp, joystickClamp);
		
			turretTransform.Rotate( -rotationY * Time.deltaTime, 0f, 0f );
			cannonTransform.Rotate( 0f, -rotationX * Time.deltaTime, 0f );
			rotationY = rotationX = 0f;

			turretTransform.Rotate( -Input.GetAxis("Mouse X")*80f*Time.deltaTime, 0f, 0f );
			cannonTransform.Rotate( 0f, Input.GetAxis("Mouse Y")*5f*Time.deltaTime, 0f );

			Vector3 turretAngles = cannonTransform.localEulerAngles;
			turretAngles.y = Mathf.Clamp( turretAngles.y, 270f-3f, 270f+15f );
			cannonTransform.localEulerAngles = turretAngles;
		
			// Swap 1st person and 3rd person perspectives
			
			if(!OVRDevice.IsHMDPresent() && (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Joystick1Button4)))
			{
				currentCameraView = CameraView.firstPerson;
				cameraTransform.localPosition = origCamPos;
				cameraTransform.localRotation = Quaternion.identity;
			}
			else if(!OVRDevice.IsHMDPresent() && (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Joystick1Button5)))
			{
				if(currentCameraView != CameraView.thirdPerson)
					cameraTransform.Rotate(new Vector3(15f, 0f, 0f));
				
				currentCameraView = CameraView.thirdPerson;
				cameraTransform.localPosition = new Vector3(0f, 3f, -3f);
			}
			
			// Move cat up and down

			if((Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Joystick1Button9)))
			{
				iTween.Stop(cameraTransform.gameObject);
				
				float moveToPos = 0f, catMoveToPos = 0f;
				
				if(isInCockpit)
				{
					isInCockpit = false;
										
					if(currentCameraView == CameraView.firstPerson)
						moveToPos = 0.3f;
					
					catMoveToPos = -0.88f;
					
					iTween.RotateTo(hatchTransform.gameObject, iTween.Hash("rotation", new Vector3(0f, 160f, 0f), "isLocal", true));
				}
				else
				{
					isInCockpit = true;
					
					//if(currentCameraView == CameraView.firstPerson)
					//	moveToPos = -0.6f;
					
					catMoveToPos = -1.89f;
					
					iTween.RotateTo(hatchTransform.gameObject, iTween.Hash("rotation", new Vector3(0f, 0f, 0f), "isLocal", true));
				}
		
				//if(currentCameraView == CameraView.firstPerson)
				//	iTween.MoveTo(cameraTransform.gameObject, iTween.Hash("y", moveToPos, "time", 1f, "isLocal", true));
				
				iTween.MoveTo(catTransform.gameObject, iTween.Hash("y", catMoveToPos, "time", 1f, "isLocal", true));
			}
		}
	}

	//----------------------------------------------------------------------------------------

	void OnNetworkInstantiate( NetworkMessageInfo info ) {
		NetworkingManager.Instance.RegisterTank(this);
	}

	[RPC]
	void FireBullet()
	{
		if( networkView.isMine ) {
			GameObject bullet = (GameObject)Network.Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.identity, 0);

			//Physics.IgnoreCollision(bullet.collider, cannonCollider );
			
			//bullet.transform.parent = transform.parent;

			Vector3 spawnPointVelocity = rigidbody.GetPointVelocity( bulletSpawnTransform.position );
			bullet.rigidbody.velocity = spawnPointVelocity;
			bullet.rigidbody.AddForce(bulletSpawnTransform.forward * 80f, ForceMode.Impulse);
		}

		reloadAudioSource.time = 0f;
		reloadAudioSource.Play();

	//	iTween.MoveTo( cannonRecoilTransform.gameObject, iTween.Hash( "x", .05f, "time", 0.25f, "islocal", true, "oncomplete", "OnRecoilComplete", "oncompletetarget", gameObject, "easetype", iTween.EaseType.easeOutBack ) );

		ParticleSystem p = (ParticleSystem)Instantiate( cannonFireParticles, bulletSpawnTransform.position, Quaternion.FromToRotation( Vector3.forward, bulletSpawnTransform.forward ) );
		Destroy( p.gameObject, 2f );

	//	yield return new WaitForSeconds(5f);
		

	}

	void OnRecoilComplete() {
		//iTween.MoveTo( cannonRecoilTransform.gameObject, iTween.Hash( "x", 0f, "time", 1.65f, "islocal", true, "easetype", iTween.EaseType.easeOutQuint ) );
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
