using UnityEngine;
using System.Collections;

public class NetworkingManager : MonoBehaviour {
	
	[SerializeField]
	GameObject playerPrefab;
	
	[SerializeField]
	Transform spawnTransform;
	
	string serverIP = "127.0.0.1";
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
	
	void OnDisconnectedFromServer(NetworkDisconnection info)
	{
		Network.RemoveRPCs(Network.player);
		Network.DestroyPlayerObjects(Network.player);
	}
	
	void OnGUI()
	{
		if(Network.peerType == NetworkPeerType.Disconnected)
		{
			if(GUILayout.Button("Connect"))
				Network.Connect(serverIP, serverPort);
		
			if(GUILayout.Button("New Server"))
				Network.InitializeServer(32, serverPort, true);
		}
		else if(GUILayout.Button("Disconnect"))
			Network.Disconnect();
	}
}
