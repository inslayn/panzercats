using UnityEngine;
using System.Collections;

public class NetworkingManager : MonoSingleton<NetworkingManager> {
	
	[SerializeField]
	Tank playerPrefab;
	
	[SerializeField]
	Transform spawnTransform;

	[SerializeField]
	Transform[] spawnPoints = null;

	string serverIP = "172.21.10.252";
    public string ServerIP { get { return serverIP; } set { serverIP = value; } }

	int serverPort = 30000;
	int numberPlayers = 0;

	void Awake() {
		Instance = this;
	}

	void OnServerInitialized()
	{
		Debug.Log ("OnServerInitialized");
		SpawnPlayer();
	}
	
	void OnConnectedToServer()
	{
		Debug.Log ("OnConnectedToServer");
		SpawnPlayer();
	}
	
	void SpawnPlayer()
	{
		Network.Instantiate(playerPrefab, spawnPoints[numberPlayers%spawnPoints.Length].position, Quaternion.identity, 0);
	}

	public void RegisterTank( Tank playerTank ) {
		playerTank.Died += OnPlayerDied;
		numberPlayers++;
	}
	
	void OnPlayerDied()
	{
		numberPlayers--;	
		Debug.Log("Number of players: " + numberPlayers);
		if(numberPlayers == 1)
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

    public void CreateServer() {
        if(Network.peerType == NetworkPeerType.Disconnected)
            Network.InitializeServer(32, serverPort, true);
            
    }

    public void JoinServer(string ip) {
        if(Network.peerType == NetworkPeerType.Disconnected)
            Network.Connect(ip, serverPort);
    }

    public void DisconnectServer() {
        if(Network.peerType != NetworkPeerType.Disconnected)
            Network.Disconnect();
    }

	void Update()
	{	
		if(Network.peerType == NetworkPeerType.Disconnected)
		{
			if(Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Joystick1Button7))
				Network.Connect(serverIP, serverPort);
		
			if(Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.Joystick1Button6))
				Network.InitializeServer(32, serverPort, true);
		}
		else if(Input.GetKeyDown(KeyCode.Minus))
			Network.Disconnect();
	}
}
