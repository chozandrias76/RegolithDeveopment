using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager
{
	PangoWorld						root;
	float 							curEditRadius = 3.3f;
	RaycastHit 						hit = new RaycastHit ();
	GameObject 						cursorObject;
	int								editCooldown = 0;
	int								lastSelectedSlot = -1;
	int								selectedSlot = 0;
	public static int 				action_fire = 0;
	public static int 				action_mine = 1;
	public static int 				action_build = 2;
	
	RenderStateTracker				renderStateSwitcher=new RenderStateTracker();
	class Tool
	{
		public static Dictionary<int,Tool>	toolMap=new Dictionary<int,Tool>();
		public GameObject	cursor;
		public int			id;
		public float 		energyCost;
		public float 		mineralCost;
		public bool			canApply(Player	player){
			if((player.power >= energyCost)
				&&(player.mineral>=mineralCost))
				return true;
			return false;
		}
		
		
		public void setUnitState(GameObject obj,int state){
			Unit t=obj.GetComponent<Unit>();//Activate its script if any...
			if(t!=null)
				t.setUnitState(state);
			else{
				obj.layer = LayerMask.NameToLayer ("Ignore Raycast");
			}
		}
		
		public void	apply(Player player,Vector3 position,Quaternion orientation){
			player.mineral-=mineralCost;
			player.power-=energyCost;
			GameObject obj=(GameObject)GameObject.Instantiate(cursor,position,orientation);
			
			Unit unit=obj.GetComponent<Unit>();
			if(unit!=null)
				unit.initializeUnit(player.body.networkView.viewID);
			
			setUnitState(obj,Unit.unit_active);		
		}
		
		public Tool(int _id,GameObject	c,float _energyCost,float	_mineralCost){
			cursor=c;
			
			id=_id;
			toolMap[id]=this;
			energyCost=_energyCost;
			mineralCost=_mineralCost;
			cursor.SetActiveRecursively(false);
		}
	}
	
	Tool[]	toolSlots=new Tool[]{
		new Tool(0,GameObject.CreatePrimitive (PrimitiveType.Sphere),1,1),
		new Tool(1,(GameObject)GameObject.Instantiate(Resources.Load("Objs/TurretBase")),0,0),
		new Tool(2,(GameObject)GameObject.Instantiate(Resources.Load("Objs/Silo")),0,0),
		new Tool(3,(GameObject)GameObject.Instantiate(Resources.Load("Objs/PowerNode")),0,0),
	};
	
	public void	toolAction (NetworkViewID	netviewID, Vector3 position,Quaternion orientation, float power, int action, int tool)
	{
		Player player = root.playerManager.findPlayerFromViewID(netviewID);
		Tool ptool=Tool.toolMap[tool];
		if(ptool.canApply(player))
			ptool.apply(player,position,orientation);
	}
	
	
	public void	targetAction (NetworkViewID	netviewID, Vector3	pos, Vector3	look, float power, int action)
	{
		
		if (action == BuildManager.action_fire){
			Grenade.fireGrenade (root, pos, look);
		}else if (action == action_mine) {
			
			GameObject psys = (GameObject)GameObject.Instantiate (Resources.Load ("FX/Sparks"));
			psys.transform.position = pos;
			GameObject.Destroy (psys, 0.5f);
			ParticleEmitter pe = (ParticleEmitter)psys.GetComponent ("ParticleEmitter");
			pe.localVelocity = look * 6.0f;
			
			float sumDensity=root.world.editWorld (root.gameObject, pos, curEditRadius, power, World.sphereTool);
			Player player = root.playerManager.findPlayerFromViewID(netviewID);
			if(player!=null){
				if(sumDensity<-0.01f)
				{
					root.chatManager.localChat(String.Format("Acquired {0:0.00} minerals.",-sumDensity));
				}
				else if(sumDensity>0.01f)
				{
					root.chatManager.localChat(String.Format("Depleted {0:0.00} minerals.",sumDensity));
				}
			}
		}else if(action == action_build){
			
		}
	}
	
	public static Shader			diffuseShader=Shader.Find("Diffuse");
	public static Shader			transparentShader=Shader.Find ("Transparent/Diffuse");
	public static Color 		canBuildColor=new Color(0,1,0,0.5f);
	public static Color 		cantBuildColor=new Color(1,0,0,0.5f);

	void setCursor(GameObject	ncurs){
		if(cursorObject!=null)
		{
			renderStateSwitcher.resetRenderState();
			cursorObject.SetActiveRecursively(false);
		}
		cursorObject = ncurs;
		renderStateSwitcher.configure(cursorObject);
		renderStateSwitcher.changeRenderState(transparentShader,canBuildColor);
		cursorObject.SetActiveRecursively(true);
	}
	
	void updateToolSlots(){
		if(lastSelectedSlot!=selectedSlot){
			lastSelectedSlot=selectedSlot;
			setCursor(toolSlots[selectedSlot].cursor);
		}
	}
	
	public void Update ()
	{
		if (Camera.main == null)
			return;
		if(Input.GetKeyDown(KeyCode.F1))selectedSlot=0;
		else if(Input.GetKeyDown(KeyCode.F2))selectedSlot=1;
		else if(Input.GetKeyDown(KeyCode.F3))selectedSlot=2;
		else if(Input.GetKeyDown(KeyCode.F4))selectedSlot=3;
		updateToolSlots();
		
		bool editEnabled = Input.GetKey (KeyCode.LeftControl);
		Transform cam = Camera.main.transform;
		editCooldown--;
		//float crsScl=0.25f;
		//float dur=0.1f;
		if (cursorObject != null) {
			if (editEnabled == false){
				renderStateSwitcher.changeRenderState(transparentShader,Color.red);
				return;
			}
		}
		
		/*	Debug.DrawRay(cam.position, cam.forward*hit.distance);//,Color.green,dur);
			Debug.DrawRay(hit.point+new Vector3(0,-crsScl,0), new Vector3(0,2*crsScl,0));//,Color.red,dur);
			Debug.DrawRay(hit.point+new Vector3(-crsScl,0,0), new Vector3(2*crsScl,0,0));//,Color.red,dur);
			Debug.DrawRay(hit.point+new Vector3(0,0,-crsScl), new Vector3(0,0,2*crsScl));//,Color.red,dur);
		*/
		int layerMask=LayerMask.NameToLayer("Terrain");//~(LayerMask.NameToLayer ("Ignore Raycast")|LayerMask.NameToLayer ("Terrain"));
		bool gotHit = Physics.Raycast (cam.position + (cam.forward * World.fmax (2.1f,(curEditRadius*1.1f))), cam.forward, out hit);//, 100.0f,layerMask);
		
		bool canBuild=false;
		if (gotHit) {
			
			//renderer.enabled=true;
			Player player = root.playerManager.localPlayer;
			Tool ptool=toolSlots[selectedSlot];
			if(ptool.canApply(player))
				canBuild=true;
			
			//cursorObject.GetComponent<MeshRenderer>().materials[0].color=Color.green;
			//cursorObject.GetComponent<MeshRenderer>().materials[0].color=Color.red;
			
			cursorObject.transform.position = hit.point;
			//buildUnitTransform(hit.point,hit.normal);
			
			Vector3 crs=Vector3.Cross(hit.normal,cam.forward);
			crs.Normalize();
	        Quaternion rotation = new Quaternion();
	        rotation.SetLookRotation(crs,hit.normal);
	        cursorObject.transform.rotation = rotation;
			
		}
		
		renderStateSwitcher.changeRenderState(transparentShader,canBuild?canBuildColor:cantBuildColor);
		
		if (editEnabled && gotHit && (Input.GetMouseButton (0) || Input.GetMouseButton (1)) && (editCooldown <= 0)) {
			editCooldown = 25;
			
			//Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			//Debug.DrawRay (ray.origin, ray.direction * 10, Color.yellow);
			// Update is called once per frame
			//if (Physics.Raycast(ray, out hit, 500.0f))
			{
				//if (hit.transform.tag != "Untagged") 
				{	
					//Debug.Log ("Hit:" + hit.transform.tag);
					//if (hit.transform.tag == "Finish")
					
					float editValue = Input.GetMouseButton (0) ? -1.0f : 1.0f;
					
					if(selectedSlot==0){//"Sphere"){
						//Debug.Log ("Fire the missiles");
						//float crsScl=0.25f;
						//Debug.DrawRay(cam.position, cam.forward*hit.distance,Color.green,5.0f);
						//Debug.DrawRay(hit.point+new Vector3(0,-crsScl,0), new Vector3(0,2*crsScl,0),Color.red,5.0f);
						//Debug.DrawRay(hit.point+new Vector3(-crsScl,0,0), new Vector3(2*crsScl,0,0),Color.red,5.0f);
						//Debug.DrawRay(hit.point+new Vector3(0,0,-crsScl), new Vector3(0,0,2*crsScl),Color.red,5.0f);

						root.doPlayerTargetAction (hit.point, hit.normal, editValue, action_mine);
					}else{
						Transform ptrans=cursorObject.transform;
						root.doPlayerToolAction(ptrans.position,ptrans.rotation,editValue,action_build,toolSlots[selectedSlot].id);
					}
				}
			}
		}
	}
	
	public BuildManager (PangoWorld _root)
	{
		root=_root;
		selectedSlot = 0;
	}
}


