using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Grenade : MonoBehaviour
{
	PangoWorld	root;
	int 		fragCount;
	float		absorbedMass;
	bool		exploded;
	bool		absorbed;
	int 		scanCountdown;
	static AudioClip 	absorbSound =  Resources.Load("Sounds/Absorb") as AudioClip;
	
	static void spawnFX(string name,Vector3 pos,float life){
		GameObject pexp = (GameObject)Instantiate(Resources.Load("FX/"+name));
		
		pexp.transform.position=pos;
		Destroy (pexp,life);
	}
	
	static	List<Player>	scanResult=new List<Player>();
	
	void OnCollisionEnter(Collision collision){

		if(exploded){
			return;
		}
//			if(gameObject.GetComponent<AudioSource>().isPlaying==false){
		//	}
		
		//Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
		GameObject psys = (GameObject)Instantiate(Resources.Load("FX/Sparks"));
		psys.transform.position=gameObject.transform.position;
		ParticleEmitter pe=(ParticleEmitter)psys.GetComponent("ParticleEmitter");
		pe.localVelocity.Set(0,-6,0);
		Destroy(psys,0.25f);
		spawnFX ("explosion",gameObject.transform.position,0.25f);
		gameObject.GetComponent<AudioSource>().Play();
		if(root){
			float sumDensity=root.world.editWorld(root.gameObject,gameObject.transform.position,3.3f,-3.0f,World.sphereTool);
			
			spawnFX ("Fluffy Smoke Large",gameObject.transform.position,0.5f+(sumDensity*0.25f));
			absorbedMass=-sumDensity;
			
			scanResult.Clear();
			root.playerManager.findPlayersInRadius(gameObject.transform.position,4.0f,ref scanResult);
			foreach(Player plyr in scanResult)
				plyr.applyDamage(0.1f);
			
			Rigidbody rb=gameObject.GetComponent<Rigidbody>();
			gameObject.GetComponent<SphereCollider>().enabled=false;
			
			rb.useGravity=false;
			rb.velocity=Vector3.zero;
			//gameObject.GetComponent<MeshRenderer>().enabled=false;
			exploded=true;
			
		}		
	}
	
	void OnDestroy(){
		//Debug.Log ("Die");

	}
	void Update (){
		
		if(exploded){
			if(scanCountdown++>30){
				scanResult.Clear();
				root.playerManager.findPlayersInRadius(gameObject.transform.position,10.0f,ref scanResult);
				Vector3 sumVel=Vector3.zero;
				foreach(Player plyr in scanResult){
					Vector3 diff=plyr.body.transform.position-gameObject.transform.position;
					diff.Normalize();
					sumVel=sumVel+diff;
					if(absorbed==false){
						//PlayerManager.Player player=root.playerManager.findPlayerFromObject(collision.collider.gameObject);
						//if(player!=null){
							AudioSource snd=gameObject.GetComponent<AudioSource>();
							snd.clip=absorbSound;
							snd.Play();
							Destroy(gameObject,1.5f);
							spawnFX ("explosion",gameObject.transform.position,1.5f);
							plyr.applyMinerals(absorbedMass);
							absorbedMass=0.0f;
							absorbed=true;
						//}
					}
				}
				Rigidbody rb=gameObject.GetComponent<Rigidbody>();
				Vector3 vel=rb.velocity;
				vel+=sumVel*1.0f;
				rb.velocity=vel;
				scanCountdown=0;
			}
		}
	}
	
	void Start (){

	}
	
	public static void fireGrenade(PangoWorld _world,Vector3 position,Vector3 direction){
		Vector3		launchPos = position+(direction*2.0f);
		GameObject  grenade;
		//if(Network.isServer||Network.isClient)
		//	grenade=(GameObject)Network.Instantiate(Resources.Load("grenadePrefab"),launchPos,Quaternion.identity,0);
		//else
			grenade=(GameObject)GameObject.Instantiate(Resources.Load("Objs/grenadePrefab"),launchPos,Quaternion.identity);
		
		grenade.GetComponent<Grenade>().root=_world;
		GameObject.Destroy(grenade,30.0f);
		
		
		GameObject pexp = (GameObject)Instantiate(Resources.Load("FX/Smoke Trail"));
		pexp.transform.parent=grenade.transform;
		pexp.transform.position=grenade.transform.position;
		//pexp.transform.position=pos;
		Destroy (pexp,30.0f);
		
		
		grenade.GetComponent<Rigidbody>().velocity=direction*15.0f;
		
		
		/*
		GameObject gobject=GameObject.CreatePrimitive(PrimitiveType.Sphere);
		Grenade grenade=gobject.AddComponent<Grenade>();
		
		gobject.transform.position=cam.position+(cam.forward*2.0f);
		gobject.transform.localScale*=0.5f;
		Rigidbody rigidBody = (Rigidbody)gobject.AddComponent<Rigidbody>();
		rigidBody.velocity=(cam.forward*15.0f);
		Destroy(gobject,30.0f);
		*/		
	}

	public Grenade ()
	{
	}
}

