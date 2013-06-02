using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	[SerializeField]
	ParticleSystem explosionParticleSystem = null, trailParticleSystem = null;

	int bounceCount = 2;

	[RPC]
	void Explode() {
		trailParticleSystem.transform.parent = null;
		Destroy( trailParticleSystem.gameObject, 2f );

		ParticleSystem p = (ParticleSystem)Instantiate( explosionParticleSystem, transform.position, explosionParticleSystem.transform.rotation );
		Destroy ( p.gameObject, 5f );
		if( networkView.isMine ) {
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
