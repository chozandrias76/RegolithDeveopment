using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	string 		gameName="PANGOLINIUM";
	bool 		refreshing;
	HostData[]	hostData;
	bool 		gameStarted=false;
	//string 		gameMode="SinglePlayer";
	bool		connecting=false;
	public 		PangoWorld host;
	// Use this for initialization
	void Start () {
	}
	
	void spawnPlayer(){
	}

	void OnPlayerConnected(NetworkPlayer	player)
	{
		Debug.Log ("OnPlayerConnected...");
	}
	
	void OnPlayerDisconnected(NetworkPlayer	player)
	{
		Debug.Log ("OnPlayerDisconnected ...");
	}
	
	void OnConnectedToServer(){
		Debug.Log ("Connected to server...");
		connecting=false;
		
		//host.OnConnectedToServer();
	}
	
	void OnServerInitialized(){
		Debug.Log ("Server initialized...");
		//host.OnServerInitialized();
	}
	
	void OnMasterServerEvent(MasterServerEvent	evt){
		if(evt == MasterServerEvent.RegistrationSucceeded){
			Debug.Log ("Server registered...");
			
		}
	}
	
	void requestHostList(){
		MasterServer.RequestHostList(gameName);
		refreshing = true;
	//	yield return new WaitForSeconds(2);
	}
	
	void joinGame(HostData host){
		Debug.Log("Joining game:"+host.gameName);
		Network.Connect(host,"MOE");
		connecting=true;
	}
	
	public void networkGUI(){
		int guiY=15;
		int bhite=30;
		if(Network.isClient || Network.isServer || connecting || gameStarted){
			if (GUI.Button (new Rect (200,guiY,150,25),
					Network.isClient?
						(connecting?"Connecting..":"Disconnect..."):"Shutdown!")) {
				
				Network.Disconnect();
				connecting=false;
			}
			return;
		}

		if (GUI.Button (new Rect (200,guiY,150,25), "Start SinglePlayer...")) {
			gameStarted=true;
			host.startSinglePlayerGame();
		}
		
		guiY+=bhite;

		if (GUI.Button (new Rect (200,guiY,150,25), "Start server...")) {
			Network.incomingPassword="MOE";
			var useNat = !Network.HavePublicAddress();
			Network.InitializeServer(32,25001,useNat);
			MasterServer.RegisterHost(gameName,"PangolinEngine","Pangolin terrain engine test..");
		}

		guiY+=bhite;
		int hostCount=hostData!=null?hostData.Length:0;
		if (GUI.Button (new Rect (200,guiY,150,25), "Refresh hosts...("+hostCount+")")) {
			Debug.Log ("Refreshing host list..");
			requestHostList();
		}
		guiY+=bhite;
		for(int i=0;i<hostCount;i++){
			if (GUI.Button (new Rect (200,guiY+(i*bhite),150,25), hostData[i].gameName)) {
				joinGame(hostData[i]);
			}		
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(refreshing){
			if(MasterServer.PollHostList().Length!=0){
				refreshing=false;
				Debug.Log (MasterServer.PollHostList().Length);
				hostData=MasterServer.PollHostList();
			}
		}
	}
}
