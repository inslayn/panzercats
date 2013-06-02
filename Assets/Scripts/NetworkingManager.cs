using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;

public class NetworkingManager : MonoBehaviour {
	
	enum gameState {
		invalid = 0,
		mainmenu = 1,
		joinmenu = 2,
		hostmenu = 3,
		waitserver = 4,
		waitclient = 5,
		failconnect = 6,
		playing = 7
	}
	
	[SerializeField]
	GameObject playerPrefab;
	
	[SerializeField]
	Transform spawnTransform;
	
	string serverIP = "172.21.10.252";
	string localAddress = "172.21.10.252";
	int serverPort = 30000;
	int GameState = (int)gameState.waitserver;
	int playerCount = 0;
	
	GameObject localPlayerObject;
	
	void Start()
	{
		localAddress = GetLocalIPAddress();
		Debug.Log (localAddress);
		GameState = (int)gameState.mainmenu;	
	}

	public string GetLocalIPAddress()
	{
	   IPHostEntry host;
	   string localIP = "";
	   host = Dns.GetHostEntry(Dns.GetHostName());
	   foreach (IPAddress ip in host.AddressList)
	   {
	     if (ip.AddressFamily == AddressFamily.InterNetwork)
	     {
	       localIP = ip.ToString();
	     }
	   }
	   return localIP;
	}
	
	
	
	void Update()
	{	
		/*
		if(Network.peerType == NetworkPeerType.Disconnected)
		{
			if(Input.GetKeyDown(KeyCode.Equals))
				Network.Connect(serverIP, serverPort);
		
			if(Input.GetKeyDown(KeyCode.N))
				Network.InitializeServer(32, serverPort, true);
		}
		else if(Input.GetKeyDown(KeyCode.Minus))
			Network.Disconnect();
			*/
	}
	
	//----------------------------------------------------------------------------------------
	// SERVER
	//----------------------------------------------------------------------------------------
	
	void StartServer()
	{
		Network.InitializeServer(32, serverPort, true);
	}
	
	//----------------------------------------------------------------------------------------
	
	void OnServerInitialized()
	{
		GameState = (int)gameState.waitclient;
		SpawnPlayer();
	}
	
	//----------------------------------------------------------------------------------------
	
	void OnPlayerConnected(NetworkPlayer p) 
	{
//		if(Network.isServer)
//		{
			playerCount++;
			
			// allocate a networkViewID for the new player
			//NetworkViewID newViewID = Network.AllocateViewID();
		
			SpawnPlayer();
				
			Debug.Log("Player " + newViewID.ToString() + " connected from " + p.ipAddress + ":" + p.port);
		//}
    }
	
	//----------------------------------------------------------------------------------------
	
	void SpawnPlayer()
	{
		Network.Instantiate(playerPrefab, spawnTransform.position, Quaternion.identity, 0);
	}
	
	
	//----------------------------------------------------------------------------------------
	
	void OnPlayerDisconnected(NetworkPlayer player)
	{
	//	Network.RemoveRPCs(player);
	//	Network.DestroyPlayerObjects(player);

		if(Network.isServer)
		{
			playerCount--;
			
			Debug.Log("Player " + player.ToString() + " disconnected.");
			// we send this to everyone, including to
			// ourself (the server) to clean-up
			Network.RemoveRPCs(player);
		}
	}
	
	//----------------------------------------------------------------------------------------
	// CLIENT
	//----------------------------------------------------------------------------------------
	
	void ConnectToServer()
	{
		Network.Connect(serverIP, serverPort);
		GameState = (int)gameState.waitserver;
	}
	
	//----------------------------------------------------------------------------------------
	
	void OnGUI()
	{
		switch (GameState) 
		{
			
		case (int)gameState.mainmenu:
			if(GUILayout.Button("Join Game"))
			{
				GameState = (int)gameState.joinmenu;
			}
			if(GUILayout.Button("Host Game"))
			{
				GameState = (int)gameState.hostmenu;
			}
			if(GUILayout.Button("Quit"))
			{
				Application.Quit();
			}
			break;
			
		case (int)gameState.joinmenu:
			
			if(GUILayout.Button("Join!"))
			{
				ConnectToServer();
			}
			if(GUILayout.Button("Cancel"))
			{
				GameState = (int)gameState.mainmenu;
			}
			break;

		case (int)gameState.hostmenu:
			if(GUILayout.Button("Host!"))
			{
				StartServer();
			}
			if(GUILayout.Button("Cancel"))
			{
				GameState = (int)gameState.mainmenu;
			}
			break;

		case (int)gameState.waitserver:
			GUILayout.Label("Connecting...");
			if(GUILayout.Button("Cancel"))
			{
				Network.Disconnect();
				GameState = (int)gameState.joinmenu;
			}
			break;
			
		case (int)gameState.failconnect:
			GUILayout.Label("Connection to server failed");
			if(GUILayout.Button("I'll check my firewall, IP Address, Server Address, etc..."))
			{
				GameState = (int)gameState.joinmenu;
			}
			break;
			
		case (int)gameState.waitclient:
			GUILayout.Label("SERVER RUNNING");
			GUILayout.Label("waiting for connections...");
			GUILayout.Space(16);
			GUILayout.BeginHorizontal();
				GUILayout.Label("Connected Players: " + playerCount.ToString());
			GUILayout.EndHorizontal();

			if(GUILayout.Button("Kill Server"))
			{
				Network.Disconnect();
				GameState = (int)gameState.hostmenu;
			}
			break;
			
		case (int)gameState.playing:
			GUILayout.Label("..:: PANZER KATZ ::..");
			GUILayout.Label("---------------------");
			GUILayout.Label("WASD keys to move");
			GUILayout.Label("Left mouse click shoot");
			GUILayout.Label("Mouse for orientation");
			break;	
		}
	}
}
