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
	
	float speed = 2f, rotationSpeed = 200f, vertical, horizontal, health = 100f;
	
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
		if(!isDead)
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
	}
	
	//----------------------------------------------------------------------------------------
	
	IEnumerator FireBullet()
	{
		GameObject bullet = (GameObject)Network.Instantiate(bulletPrefab, turrentTransform.position, Quaternion.identity, 0);
		
		Physics.IgnoreCollision(bullet.collider, transform.collider);
		
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
	
	void OnCollisionEnter(Collision c)
	{		
		if(c.collider.CompareTag("Bullet"))
			//networkView.RPC("TakeDamage", RPCMode.Others, 5f);
			TakeDamage(5f);
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
