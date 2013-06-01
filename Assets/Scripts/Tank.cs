using UnityEngine;
using System.Collections;

public class Tank : MonoBehaviour {
	
	[SerializeField]
	ParticleSystem dustParticle = null;
	
	[SerializeField]
	Transform turrentTransform;
	
	[SerializeField]
	GameObject bulletPrefab;

	[SerializeField]
	Camera tankCamera = null;

	Transform cachedTransform;
	
	float speed = 2f, rotationSpeed = 100f, vertical, horizontal;

	//----------------------------------------------------------------------------------------
	
	void Start()	
	{
		enabled = networkView.isMine;
		tankCamera.enabled = networkView.isMine;

		cachedTransform = transform;
		
		InvokeRepeating("CheckParticles", 0.1f, 1f);
	}
	
	//----------------------------------------------------------------------------------------
	
	void FixedUpdate () 
	{
		vertical = Input.GetAxis("Vertical"); 
		horizontal = Input.GetAxis("Horizontal");

		Vector3 thrustVector = cachedTransform.forward;
		thrustVector.y = 0f;

		float forwardThrottle = vertical;
		float leftThrottle = horizontal+vertical;
		float rightThrottle = -horizontal+vertical;

		float treadDifference = leftThrottle-rightThrottle;

		rigidbody.AddTorque( 0, 10f*(treadDifference * rotationSpeed)/(1f+5f*rigidbody.velocity.magnitude), 0 );
		rigidbody.AddForce( 100f*(leftThrottle+rightThrottle)*thrustVector );

	}

	void Update() {
		if(Input.GetKeyDown(KeyCode.Space))
			StartCoroutine(FireBullet());
	}

	//----------------------------------------------------------------------------------------
	
	IEnumerator FireBullet()
	{
		GameObject bullet = (GameObject)Network.Instantiate(bulletPrefab, turrentTransform.position, Quaternion.identity, 0);
		
		bullet.transform.parent = transform.parent;
		
//		GameObject b = ObjectPooler.Instance.BulletPool[0];
//		
//		b.SetActive(true);
//		
//		ObjectPooler.Instance.BulletPool.Remove(b);
//		
//		b.transform.position = turrentTransform.position;
//		
//		b.transform.rotation = Quaternion.identity;
//		
		bullet.rigidbody.AddForce(cachedTransform.forward * 10f, ForceMode.Impulse);
//		
		yield return new WaitForSeconds(0.5f);
//		
//		ResetBullet(b);
		
		Network.Destroy(bullet);
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
	
	void OnCollisionEnter(Collision c)
	{
		if(c.collider.CompareTag("Bullet"))
			print("Collision");
	}
	
	//----------------------------------------------------------------------------------------
	
	[RPC]
	void TakeDamage(float amount)
	{
//		if(Network.isServer)
			
	}
}
