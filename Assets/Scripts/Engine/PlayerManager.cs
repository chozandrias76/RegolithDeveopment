using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerManager
{
	PangoWorld							root;
	List<Player>						players=new List<Player>();
	Dictionary<NetworkViewID,Player>	playerObjects=new Dictionary<NetworkViewID, Player>();

	public Player						localPlayer;
	
	public void dumpPlayers(){
		foreach(Player p in players){
			root.chatManager.localChat("Player:"+p.body.GetComponent<NetworkView>().viewID.ToString());
		}
	}
	
	
	public void activatePlayerControls (GameObject playerObject, bool active)
	{
		Transform ocam=playerObject.transform.FindChild ("Main Camera");
//		ocam.camera.enabled = active;
//		ocam.GetComponent<AudioListener>().enabled=active;
		ocam.gameObject.GetComponent<MouseLook> ().enabled = active;
		playerObject.GetComponent<CharacterController> ().enabled = active;
		playerObject.GetComponent<MouseLook> ().enabled = active;
		playerObject.GetComponent<CharacterMotor> ().enabled = active;
		playerObject.GetComponent<FPSInputController> ().enabled = active;
	}
	
	public Player createLocalPlayer(GameObject	body){
		Player player=findPlayerFromObject(body);//registerPlayerObject(body);
		localPlayer=player;
		player.isLocal=true;
		player.updateHUD();
		activatePlayerControls(player.body,true);
		
		Transform ocam=player.body.transform.FindChild ("Main Camera");
		ocam.camera.enabled = true;
		ocam.GetComponent<AudioListener>().enabled = true;
		
		return player;
	}
	
	public void deRegisterPlayerObject(GameObject body){
		NetworkViewID nvid=body.GetComponent<NetworkView>().viewID;
		Player p=findPlayerFromObject(body);
		playerObjects.Remove(nvid);
		players.Remove(p);
		if(localPlayer==p){
			localPlayer=null;
			
		}
	}
	
	public Player registerPlayerObject(GameObject body){
		Player player=new Player(root);
		player.body=body;
		NetworkViewID nvid=body.GetComponent<NetworkView>().viewID;
		playerObjects[nvid]=player;
		Debug.Log ("Registering player:"+nvid.ToString());
		players.Add(player);
		if(root.viewerObject==body){
			//Hack, if this is our local player...
			createLocalPlayer(body);
		}
		return player;
	}
	
	
	public void findPlayersInRadius(Vector3	pos,float	rad,ref List<Player>  result){
		float sqrad=rad*rad;
		foreach(Player p in players)
			if((p.body.transform.position-pos).sqrMagnitude < sqrad)
				result.Add(p);
	}
	
	public Player	findPlayerFromViewID(NetworkViewID vi){
		if(playerObjects.ContainsKey(vi))
			return playerObjects[vi];
		return null;
	}
	public Player	findPlayerFromObject(GameObject gameObject){
		NetworkView view=gameObject.GetComponent<NetworkView>();
		if(view==null)
			return null;	
		return findPlayerFromViewID(view.viewID);
	}
	
	public void	updatePlayers(){
		foreach(Player p in players)
			p.updateStats(); 
	}
	public PlayerManager (PangoWorld _root)
	{
		root=_root;
	}
}


