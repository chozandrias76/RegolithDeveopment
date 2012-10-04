using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Turret : Unit {
	static	List<Player>	scanResult=new List<Player>();
	Transform 	ourTransform;
	Transform 	base1;
	Transform 	base2;
	Transform 	muzzle;
	int 		checkCounter;
	int			checkInterval=60;
	float		fireCounter=0;
	float 		fireInterval=0.3f;
	float		muzzleFlash=0;
	// Use this for initialization
	public override void  unitStart () {
		ourTransform=transform;
		base1=ourTransform.GetChild(0);//GetComponentsInChildren<Material>()
		base2=base1.GetChild(0);
		muzzle=base2.GetChild(0);
	}

	void trackTurret(Vector3 target)
	{
		Vector3 delt=target-base1.position;
		Vector3 dxz=new Vector3(Vector3.Dot(ourTransform.right,delt),0,Vector3.Dot(ourTransform.forward,delt));
		Vector3 dup1=new Vector3(0,1,0);
		//float theta=atan2(dxz.x,dxz.z);
		Quaternion q=new Quaternion();
		q.SetLookRotation(dxz,dup1);//dup1,dxz);
		base1.transform.localRotation=q;
		
		delt=target-base2.position;
		dxz=new Vector3(Vector3.Dot(base1.right,delt),Vector3.Dot(base1.up,delt),Vector3.Dot(base1.forward,delt));
		//dxz=new Vector3(Vector3.Dot(base1.right,delt),Vector3.Dot(base1.up,delt),Vector3.Dot(base1.forward,delt));
		//q.SetLookRotation(Vector3.Cross(base1.up,dxz),dup1);
		q.SetLookRotation(dxz,dup1);
		base2.transform.localRotation=q;
	}
	
	static RaycastHit	rayHit=new RaycastHit();
	GameObject			currentTarget;
	// Update is called once per frame
	public override void unitUpdate () {
		if(checkCounter++>=checkInterval){
			configure();
			scanResult.Clear();
			currentTarget=null;
			root.playerManager.findPlayersInRadius(gameObject.transform.position,30.0f,ref scanResult);
			foreach(Player plyr in scanResult){
				if(owningPlayer==plyr)
					continue;
				currentTarget=plyr.body;
				//If target is not owner of turret...
				//plyr.applyDamage(0.1f);
				break;

			}
			checkCounter=0;
			checkFooting=true;
		}
		if(currentTarget==null)
			return;

		trackTurret(currentTarget.transform.position);
		
		fireCounter+=Time.deltaTime;
		if(fireCounter>=fireInterval){

			bool hasLineOfSight=false;
			Vector3 raystart=currentTarget.transform.position;
			Vector3 rayend=muzzle.transform.position;
			Vector3 rayNormal=(rayend-raystart);
			float dlen=rayNormal.magnitude;
			rayNormal/=dlen;
			if(Physics.Raycast(raystart,rayNormal,out rayHit,dlen,~0)==false){
				hasLineOfSight=true;
			}
			//if(Physics.Raycast(raystart,rayNormal,out rayHit,dlen,~0)==true){
			//	GameObject go=rayHit.collider.gameObject;
			//	if(go!=null&&go.name.StartsWith("player"))
			//		hasLineOfSight=true;
			//}
			if(hasLineOfSight){					
				GameObject	projectile = (GameObject)GameObject.Instantiate (Resources.Load ("Objs/laserBolt"), muzzle.position,muzzle.rotation);		
			}				
			fireCounter=0;
		}
		if(muzzleFlash>0.0f){
			muzzle.gameObject.renderer.materials[0].color=new Color(1,1,1,muzzleFlash);
			muzzleFlash-=0.1f;
		}
	}
}
