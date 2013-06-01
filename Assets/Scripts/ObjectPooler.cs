using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPooler : MonoSingleton<ObjectPooler> {

	[SerializeField]
	GameObject bulletPrefab;
	
	[SerializeField]
	int numberOfBulletsToInstantiate;
	
	List<GameObject> bulletPool = new List<GameObject>();

	public List<GameObject> BulletPool {
		get {
			return this.bulletPool;
		}
	}
	
	void OnServerInitialized()
	{
		InstantiateBullets();
	}
	
	void InstantiateBullets()
	{
		for(int i = 0; i < numberOfBulletsToInstantiate; i++)
		{
			bulletPool.Add((GameObject)Network.Instantiate(bulletPrefab, transform.position, Quaternion.identity, 0));
			bulletPool[i].SetActive(false);
			bulletPool[i].transform.parent = transform;
		}
	}
}