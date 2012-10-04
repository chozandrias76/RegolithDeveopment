using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

//Awww ya cool.

//big dongs

public class PangoWorld : MonoBehaviour
{
	public World			world;
	public NetworkManager	networkManager;
	public PlayerManager	playerManager;
	public ChatManager		chatManager;
	public BuildManager		buildManager;
	
	public GameObject	viewerObject;
	public GameObject	waterObject;
	public GameObject	cameraObject;
	
	public float		seaLevel;
	
	//bool				mouseCaptured = true;
	bool 				simRunning = true;
	bool 				simIsPaused = false;
	
	static	PangoWorld	root;
	public static PangoWorld getRoot(){
		return root;
	}
	PangoWorld(){
		root=this;	
	}
	
	void Reset ()
	{
	}
	
	public void startSinglePlayerGame ()
	{
		Debug.Log ("startSinglePlayerGame");
		createSinglePlayer ();
	}
	
	void createNetworkPlayer ()
	{
		GameObject	playerObj = (GameObject)Network.Instantiate (Resources.Load ("Objs/playerPrefab"), new Vector3 (0, 0, 0), Quaternion.identity, 0);
		cameraObject.GetComponent<AudioListener>().enabled=false;
		viewerObject = playerObj;
		pauseSim (false);
	}
	
	void createSinglePlayer ()
	{
		GameObject	playerObj = (GameObject)Instantiate (Resources.Load ("Objs/playerPrefab"), new Vector3 (0, 0, 0), Quaternion.identity);
		cameraObject.GetComponent<AudioListener>().enabled=false;
		viewerObject = playerObj;
		pauseSim (false);
	}
	
	void OnPlayerConnected (NetworkPlayer	networkPlayer)
	{
		Debug.Log ("OnPlayerConnected");
		
//		GameObject remotePlayer=(GameObject)Network.Instantiate(Resources.Load("remotePlayerPrefab"),new Vector3(0,0,0),Quaternion.identity,0);
	}
	
	void OnPlayerDisconnected (NetworkPlayer	networkPlayer)
	{
		Debug.Log ("OnPlayerDisconnected");
		Network.RemoveRPCs (networkPlayer);
		Network.DestroyPlayerObjects (networkPlayer);
	}
	
	public void OnConnectedToServer ()
	{
		Debug.Log ("OnConnectedToServer");
		createNetworkPlayer ();
	}
	
	public void OnServerInitialized ()
	{
		Debug.Log ("OnServerInitialized");
		createNetworkPlayer ();
	}
	
	public void OnApplicationQuit()
	{
		world.saveWorld();
	}
	
	void Start ()
	{
		world = new World (this);
		playerManager = new PlayerManager(this);
		chatManager = new ChatManager(this);
		buildManager = new BuildManager(this);

		cameraObject = GameObject.Find ("Camera");
		
		waterObject = GameObject.Find ("Thrax Water");
		
		seaLevel=waterObject.transform.position.y;
		
		networkManager = GetComponent<NetworkManager> ();
		networkManager.host = this;
		//Debug.Log ("START.");
		
		Screen.showCursor = false;
		Screen.lockCursor = true;
		
		viewerObject = GameObject.Find ("Camera");//First Person Controller");
	//	viewerObject.layer = LayerMask.NameToLayer ("Ignore Raycast");
	
		world.loadWorld();
		
		
		world.primeCache ();
		
		pauseSim (true);

	}
	
	void Update ()
	{
		if(world==null){
			//Application.Quit();
			Debug.Log ("Pangosim has been reinstantiated!!");
			//Application.LoadLevel(0);
			//Start ();
		}
		
		world.update ();
		
		chatManager.Update ();
		
		
		if (Input.GetKeyDown (KeyCode.Escape) == true) {
			if (simIsPaused) {
				simIsPaused = false;
				pauseSim (false);
			} else {
				simIsPaused = true;
				pauseSim (true);
			}
		}
		
		playerManager.updatePlayers();

		buildManager.Update ();

		if(chatManager.inChat==false){
			if (Input.GetKeyDown (KeyCode.E) == true && playerManager.localPlayer!=null) {
				Transform cam = playerManager.localPlayer.body.transform.FindChild ("Main Camera").camera.transform;
				doPlayerTargetAction (cam.position, cam.forward, 0.0f, BuildManager.action_fire);
			}
		}
		//Load/generate queued regions one per frame...
		world.updateGenerator ();
		
		if (waterObject != null && (viewerObject!=null)) {	//Make water object follow player...
			Vector3 vpos = viewerObject.transform.position;
			vpos.y = waterObject.transform.position.y;
			waterObject.transform.position = vpos;
		}
		if (world.isRegionLoaded (world.regionName) == false) {
			if (simIsPaused == false)
				pauseSim (true);
		} else {//Keep updating the players current region hasnt loaded yet..	
			if (simIsPaused == false)
				pauseSim (false);
		}
	}
	
	
	void OnGUI ()
	{
		//int ry=0;
		if (simIsPaused == true) {
			GUI.TextArea (new Rect (10, 10, 150, 100), "Controls: AWSD   E=Grenade  Ctrl-LMouse=Build Ctrl-RMouse=Destroy Esc=Toggle UI");
			if (GUI.Button (new Rect (10, 115, 150, 25), "Resume..")) {
				pauseSim (false);
			}
			networkManager.networkGUI ();
			
		}
		chatManager.Render ();
		
	}
	
	public void attachCameraToObject (GameObject go)
	{
		cameraObject.camera.transform.transform.position = go.transform.position;
		cameraObject.camera.transform.parent = go.transform;
		cameraObject.GetComponent<MouseLook> ().axes = MouseLook.RotationAxes.MouseY;
	}
	
	
	void	captureMouse (bool sieze)
	{
		
//		if(mouseCaptured==true&&
		if (sieze == true) {
			
			if (playerManager.localPlayer!=null) {
				playerManager.activatePlayerControls (playerManager.localPlayer.body, true);
			}
			Screen.showCursor = false;
			Screen.lockCursor = true;
			//mouseCaptured = false;
		} else {// if(mouseCaptured==false&&sieze==true){
			
			if (playerManager.localPlayer!=null) {
				playerManager.activatePlayerControls (playerManager.localPlayer.body, false);
			}
			Screen.showCursor = true;
			Screen.lockCursor = false; 
			//mouseCaptured = true;
		}
	}
	
	public void pauseSim (bool	pause)
	{
		if (pause == true && simRunning == true) {
			simRunning = false;
			simIsPaused = true;
//			Time.timeScale=0.0f;
			captureMouse (false);
			Debug.Log ("Sim paused");	
			
			//GameObject.Find("Logo").guiText.text = "OHHHL:LLLD LONG JOHNSON!";

		} else if (pause == false && simRunning == false) {
			simRunning = true;
			simIsPaused = false;
//			Time.timeScale=1.0f;
			captureMouse (true);
			Debug.Log ("Sim resumed.");
		}
	}
	/*
	[Serializable ()]
	public class SaveData : ISerializable {
		public SaveData(){
		}
	}
	public void Save () {

		SaveData data = new SaveData ();
	
		Stream stream = File.Open("MySavedGame.game", FileMode.Create);
		BinaryFormatter bformatter = new BinaryFormatter();
	        bformatter.Binder = new VersionDeserializationBinder(); 
		Debug.Log ("Writing Information");
		bformatter.Serialize(stream, data);
		stream.Close();
	}

public void Load () {

	SaveData data = new SaveData ();
	Stream stream = File.Open("MySavedGame.gamed", FileMode.Open);
	BinaryFormatter bformatter = new BinaryFormatter();
	bformatter.Binder = new VersionDeserializationBinder(); 
	Debug.Log ("Reading Data");
	data = (SaveData)bformatter.Deserialize(stream);
	stream.Close();
}
	*/
	
	public void allChat(string str){
		if (Network.isServer || Network.isClient)
		{	if(playerManager.localPlayer!=null)
				networkView.RPC ("playerChat", RPCMode.All, new object[]{
					playerManager.localPlayer.body.networkView.viewID,str});
		}else
			chatManager.localChat(str);
	}
	
	[RPC]
	void	playerChat (NetworkViewID	netviewID, string chat)
	{
		chatManager.localChat(chat);
	}
	
	
	public void doPlayerTargetAction (Vector3	pos, Vector3	look, float power, int action)
	{
		if (Network.isServer || Network.isClient)
			networkView.RPC ("playerTargetAction", RPCMode.All, new object[]{
				playerManager.localPlayer.body.networkView.viewID,pos,look,power,action});
		else
			playerTargetAction (playerManager.localPlayer.body.networkView.viewID, pos, look, power, action);
	}
	[RPC]
	void	playerTargetAction (NetworkViewID	netviewID, Vector3	pos, Vector3	look, float power, int action)
	{	//Debug.Log ("Got player action:"+netviewID.ToString());
		buildManager.targetAction(netviewID,pos,look,power,action);
	}
	
	public void doPlayerToolAction (Vector3 position,Quaternion orientation, float power, int action, int toolType)
	{
		if (Network.isServer || Network.isClient)
			networkView.RPC ("playerToolAction", RPCMode.All, new object[]{
				playerManager.localPlayer.body.networkView.viewID,position,orientation,power,action,toolType});
		else
			playerToolAction (playerManager.localPlayer.body.networkView.viewID, position,orientation, power, action,toolType);
	}
	
	[RPC]
	void	playerToolAction (NetworkViewID	netviewID,Vector3 position,Quaternion orientation, float power, int action,int toolType)
	{	//Debug.Log ("Got player action:"+netviewID.ToString());
		buildManager.toolAction(netviewID,position,orientation,power,action,toolType);
	}
}