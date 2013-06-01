using UnityEngine;
using System.Collections;

public class Tank : MonoBehaviour {
	
	[SerializeField]
	ParticleSystem dustParticle = null;
	
	[SerializeField]
	Transform turrentTransform;
	
	[SerializeField]
	GameObject bulletPrefab;
	
	Transform cachedTransform;
	
	float speed = 2f, rotationSpeed = 200f, vertical, horizontal;

	//----------------------------------------------------------------------------------------
	
	void Start()	
	{
		enabled = networkView.isMine;
		
		cachedTransform = transform;
		
		InvokeRepeating("CheckParticles", 0.1f, 1f);
	}
	
	//----------------------------------------------------------------------------------------
	
	void Update () 
	{
		vertical = Input.GetAxis("Vertical"); 
		horizontal = Input.GetAxis("Horizontal");
		
		if(vertical != 0)
//			cachedTransform.rigidbody.AddRelativeForce(Vector3.forward * speed * Time.deltaTime);
        	cachedTransform.Translate(Vector3.forward * vertical * speed * Time.deltaTime);
		
		if(horizontal != 0)
        	cachedTransform.Rotate(0, (horizontal * rotationSpeed) * Time.deltaTime, 0);
		
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
