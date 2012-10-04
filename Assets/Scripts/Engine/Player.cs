using System;
using UnityEngine;

public class Player
{
	PangoWorld			root;
	
	public string		name;
	public GameObject	body;
	public float		hp	=	0.75f;
	public float		hpMax = 1.0f;
	public float		mineral = 50.0f;
	public float		mineralMax = 500.0f;
	public float		power = 0.0f;
	public float		powerMax = 1.0f;
	public float		armor = 0.5f;
	public float		armorMax = 1.0f;
	public bool			isLocal = false;
	
	public void applyDamage(float amt){
		armor-=amt;
		if(armor<0.0f)
		{	amt=armor;
			armor=0;
			hp+=amt;
			if(hp<0.0f){
				hp=0.0f;
				
				
				body.transform.position=new Vector3(0,0,0);
				root.chatManager.localChat("PLAYER DESTROYED.");
				
				
				root.world.update();
				root.world.primeCache();
				
				root.pauseSim (true);
				//GameObject.Destroy(body,3.0f);
			}
		}
		updateHUD();
	}
	
	public void applyMinerals(float amt){
		mineral+=amt;
		if(mineral<0.0f)mineral=0.0f;
		if(mineral>mineralMax)mineral=mineralMax;
	}
	
	public void updateStats(){
		bool dirty=false;
		
		if(hp<hpMax){
			hp+=0.001f;
			if(hp>hpMax)
				hp=hpMax;
			dirty=true;
		}
		if(armor<armorMax){
			float repair=World.fmin(0.0001f,power);
			power-=repair;
			if(power<0.0f){repair+=power;power=0;}
			armor+=repair;
			if(armor>armorMax){power+=(armor-armorMax);armor=armorMax;}
			dirty=true;
		}
		if(power<powerMax){
			float	generate=World.fmin(0.0002f,mineral);
			mineral-=generate;
			if(mineral<0.0f){generate+=mineral;mineral=0.0f;}
			power+=generate;
			if(power>powerMax){mineral+=(power-powerMax);power=powerMax;}
			dirty=true;
		}
		if(dirty && isLocal){
			updateHUD();
		}
	}
	
	public void updateHUD (){
		Transform root=body.transform.FindChild("Main Camera").FindChild("HUDModel");
		Transform t=root.FindChild("HUDHitpoints");
		t.localScale=new Vector3(hp/hpMax,t.localScale.y,t.localScale.z);
		t=root.FindChild("HUDArmor");
		t.localScale=new Vector3(armor/armorMax,t.localScale.y,t.localScale.z);
		t=root.FindChild("HUDEnergy");
		t.localScale=new Vector3(power/powerMax,t.localScale.y,t.localScale.z);
		t=root.FindChild("HUDMinerals");
		t.localScale=new Vector3(mineral/mineralMax,t.localScale.y,t.localScale.z);
	}
	public Player(PangoWorld _world){
		root=_world;
	}
}


