using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour{
	Vector3		start;
	Vector3		end;
	float		velocity;
	float 		age;
	Transform	bodyTransform;
	float		lastProgress;
	Vector3		rayNormal;
	static RaycastHit	rayHit=new RaycastHit();
	static AudioClip 	hitSound =  Resources.Load("Sounds/Pow1") as AudioClip;
	Projectile(){
		age=0;
		velocity=30.0f;
	}
	// Use this for initialization
	void Start () {
		AudioSource snd=gameObject.GetComponent<AudioSource>();
		snd.pitch=(World.frand()*0.4f)+0.5f;
		gameObject.layer = LayerMask.NameToLayer ("Ignore Raycast");
		bodyTransform=transform;
		start=bodyTransform.position;
		rayNormal=bodyTransform.forward;
		
		end=start+(rayNormal*300.0f);
		
		Quaternion newRot=new Quaternion();
		newRot.SetLookRotation(end-start,bodyTransform.up);
		bodyTransform.rotation=newRot;
		gameObject.active=true;
		lastProgress=0;
	}
	
	// Update is called once per frame
	void Update () {
		
		if(age>=1.0f)return;
		age+=1.0f/60.0f;
		if(age>1.0f)age=1.0f;
		float progress=(age*velocity);
		Vector3 raystart=(rayNormal*lastProgress)+start;
		Vector3 rayend=(rayNormal*progress)+start;
		

		bodyTransform.position=rayend;
		float dlen=progress-lastProgress;
		if(Physics.Raycast(raystart,rayNormal,out rayHit,dlen,~0)==true){
		//	lastProgress=0;
	//		age=0;
			GameObject go=rayHit.collider.gameObject;
			
			if(go!=null){
				if(go.name.StartsWith("player")){
					
					Player player=PangoWorld.getRoot().playerManager.findPlayerFromObject(go);//registerPlayerObject(body);
					if(player!=null){
						player.applyDamage(0.1f);
					}
				}
				//Debug.Log ("GOt hit:"+go.name);
		
				AudioSource snd=gameObject.GetComponent<AudioSource>();
				snd.pitch=(World.frand()*0.4f)+0.4f;
				snd.clip=hitSound;
				snd.Play();
				/*
					GameObject pexp = (GameObject)Instantiate(Resources.Load("Smoke Trail"));
					pexp.transform.parent=transform;
					pexp.transform.position=transform.position;
					Destroy (pexp,1.0f);
				*/
				GetComponent<Renderer>().enabled=false;
				PangoWorld root=PangoWorld.getRoot();
				float sumDensity=root.world.editWorld(root.gameObject,gameObject.transform.position,3.3f,-3.0f,World.sphereTool);
			
				
			}			
			GameObject psys = (GameObject)GameObject.Instantiate (Resources.Load ("FX/Sparks"));
			psys.transform.position = raystart;
			GameObject.Destroy (psys, 0.5f);
			age=1.0f;
		}
		if(age==1.0f){
			//Destroy...
			Destroy(gameObject,0.5f);
		}else
			lastProgress=progress;
	}
}
