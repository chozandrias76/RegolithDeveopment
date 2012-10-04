using System;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
	public static int					unit_inactive=0;
	public static int					unit_active=1;

	public 	NetworkViewID 				ownerView;
	public 	Player						owningPlayer;
	protected PangoWorld				root;
	int									unitState;
	World.Region 						region;
	public Transform					cachedTransform;
	
	static int[] 						tmpCoord=new int[3];
	public bool							checkFooting;
	
	public virtual void unitStart(){
	}
	
	public virtual void unitUpdate(){
	}
	
	public void configure(){
		if(root==null){
			root=PangoWorld.getRoot();
			cachedTransform=transform;
			moveTo(cachedTransform.position);
			unitStart();
		}
	}	

	public void moveTo(Vector3	toPos){
		root.world.getRgnCoord(toPos,ref tmpCoord);
		if(region==null){
			region = root.world.getRegion(tmpCoord);
			region.units.Add(this);
			return;
		}
		if( (tmpCoord[0]!=region.coord[0])||
			(tmpCoord[1]!=region.coord[1])||
			(tmpCoord[2]!=region.coord[2]))
		{
			region.units.Remove(this);	
			region = root.world.getRegion(tmpCoord);
			region.units.Add(this);
		}
		cachedTransform.position=toPos;
	}
	
	void Start(){
		configure();
	}
	
	
	static RaycastHit rayHit=new RaycastHit();
	
	void Update(){
		if(checkFooting){
			GameObject hit=World.rayCast(transform.position,transform.position-new Vector3(0,20.0f),ref rayHit);
			if(hit!=null){
				if(rayHit.point.y<transform.position.y){
					Vector3 tmp=transform.position;
					tmp.y=rayHit.point.y;
					transform.position=tmp;
				}
			}
		}
		unitUpdate();
	}
	
	public void initializeUnit(NetworkViewID viewID){
		configure();
		ownerView=viewID;
		owningPlayer=root.playerManager.findPlayerFromViewID(viewID);
	}
	
	public void setUnitState(int newState){
		configure();
		if(newState==unit_active)
		{
			enabled=true;
	
		}
		else if(newState==unit_inactive)
		{
			enabled=false;
			
		}
	}
	public Unit ()
	{
	}
}


