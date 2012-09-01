using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Grenade : MonoBehaviour
{
	PangoWorld	world;
	int 		fragCount;
	static void spawnFX(string name,Vector3 pos,float life){
		GameObject pexp = (GameObject)Instantiate(Resources.Load(name));
		pexp.transform.position=pos;
		Destroy (pexp,life);
	}
	void OnCollisionEnter(Collision collision){
		Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
		GameObject psys = (GameObject)Instantiate(Resources.Load("Sparks"));
		psys.transform.position=gameObject.transform.position;
		ParticleEmitter pe=(ParticleEmitter)psys.GetComponent("ParticleEmitter");
		pe.localVelocity.Set(0,-6,0);
		Destroy(psys,0.25f);
		spawnFX ("explosion",gameObject.transform.position,1.5f);
		spawnFX ("Fireworks",gameObject.transform.position,1.5f);
		
		world.editWorld(gameObject.transform.position,3.3f,-3.0f,PangoWorld.sphereTool);		
		if(fragCount++>1){
			Destroy(gameObject);
		}
	}
	void OnDestroy(){
		//Debug.Log ("Die");

	}
	void Update (){
	
	}
	void Start (){

	}
	
	public static void fireGrenade(PangoWorld _world){
		Transform  cam = Camera.main.transform;
		GameObject gobject=GameObject.CreatePrimitive(PrimitiveType.Sphere);
		Grenade grenade=gobject.AddComponent<Grenade>();
		grenade.world=_world;		
		gobject.transform.position=cam.position+(cam.forward*2.0f);
		gobject.transform.localScale*=0.5f;
		//grenade.collider.gameObject.
		Rigidbody rigidBody = (Rigidbody)gobject.AddComponent<Rigidbody>();
		rigidBody.velocity=(cam.forward*15.0f);
		Destroy(gobject,30.0f);
		
	}

	public Grenade ()
	{
	}
}

