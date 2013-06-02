using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	[SerializeField]
	ParticleSystem explosionParticleSystem = null;

	int bounceCount = 2;

	[RPC]
	void Explode() {
		Instantiate( explosionParticleSystem, transform.position, explosionParticleSystem.transform.rotation );
		if( Network.isServer ) {
			Network.Destroy(gameObject);
		}
    }

    void OnCollisionEnter( Collision col ) {
		if( Network.isServer ) {
			if( col.transform.CompareTag("Ground") ) {
				bounceCount--;
				if( bounceCount == 0 ) {
					networkView.RPC("Explode",RPCMode.All);
				}
			} else {
				networkView.RPC("Explode",RPCMode.All);
			}
		}
    }
}
