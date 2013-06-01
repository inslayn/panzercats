using UnityEngine;
using System.Collections;

public class NetworkingManager : MonoBehaviour {
	
	[SerializeField]
	GameObject playerPrefab;
	
	[SerializeField]
	Transform spawnTransform;
	
	string serverIP = "172.21.10.252";
	int serverPort = 30000;
	
	void OnServerInitialized()
	{
		SpawnPlayer();
	}
	
	void OnConnectedToServer()
	{
		SpawnPlayer();
	}
	
	void SpawnPlayer()
	{
		Network.Instantiate(playerPrefab, spawnTransform.position, Quaternion.identity, 0);
	}
	
	void OnPlayerDisconnected(NetworkPlayer player)
	{
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}
	
//	void OnDisconnectedFromServer(NetworkDisconnection info)
//	{
//		Network.DestroyPlayerObjects(Network.player);
//		Network.RemoveRPCs(Network.player);
//	}
	
	void Update()
	{	
		if(Network.peerType == NetworkPeerType.Disconnected)
		{
			if(Input.GetKeyDown(KeyCode.Equals))
				Network.Connect(serverIP, serverPort);
		
			if(Input.GetKeyDown(KeyCode.N))
				Network.InitializeServer(32, serverPort, true);
		}
		else if(Input.GetKeyDown(KeyCode.Minus))
			Network.Disconnect();
	}
}
