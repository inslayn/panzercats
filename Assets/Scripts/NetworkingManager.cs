using UnityEngine;
using System.Collections;

public class NetworkingManager : MonoBehaviour {
	
	[SerializeField]
	Tank playerPrefab;
	
	[SerializeField]
	Transform spawnTransform;
	
	string serverIP = "172.21.10.252";
	int serverPort = 30000;
	int numberPlayers = 0;
	
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
		Tank playerTank = (Tank)Network.Instantiate(playerPrefab, spawnTransform.position, Quaternion.identity, 0);
		
		playerTank.Died += OnPlayerDied;
		numberPlayers++;
	}
	
	void OnPlayerDied()
	{
		numberPlayers--;	
		if(numberPlayers == 0)
		{
			Debug.Log(" GAME OVER! ");
			Network.Disconnect();
		}
		
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
			if(Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Joystick1Button6))
				Network.Connect(serverIP, serverPort);
		
			if(Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.Joystick1Button7))
				Network.InitializeServer(32, serverPort, true);
		}
		else if(Input.GetKeyDown(KeyCode.Minus))
			Network.Disconnect();
	}
}
