using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {
	PangoWorld 	world;
	Player 		player;
	Transform	cachedTransform;
	// Use this for initialization
	//Transform ptransform;
	//CharacterMotor	cmotor;
	void Start () {
		world=GameObject.Find("Root").GetComponent<PangoWorld>();
		player=world.playerManager.registerPlayerObject(gameObject);
		cachedTransform=transform;
		//ptransform=transform;
		//cmotor=GetComponent<CharacterMotor>();
	}
	void OnDestroy(){
		world.playerManager.deRegisterPlayerObject(gameObject);
	}
		
	bool defaultFog = RenderSettings.fog;
	Color defaultFogColor = RenderSettings.fogColor;
	float defaultFogDensity = RenderSettings.fogDensity;
	float defaultFogStart = RenderSettings.fogStartDistance;
	float defaultFogEnd = RenderSettings.fogEndDistance;
	Material defaultSkybox = RenderSettings.skybox;
	
	Color fallFogColor = Color.white;
	float fallFogDensity = 100.0f;
	float fallFogStart = 0.0f;
	float fallFogEnd = 5.0f;
	
	
	Color waterFogColor = Color.green;
	float waterFogDensity = 100.0f;
	float waterFogStart = 0.0f;
	float waterFogEnd = 20.0f;
	
	
	// Update is called once per frame
	void Update () {
		//player.a
		float phite = cachedTransform.position.y;
		if(phite<world.seaLevel){
			if(	phite<-100)
			{
				//FALL DEATH FOG
				float lerp=Mathf.Clamp((-100.0f-phite)/100.0f,0,1);
				player.applyDamage(0.01f*lerp);
				RenderSettings.fogColor=Color.Lerp(defaultFogColor, fallFogColor, lerp);
				RenderSettings.fogDensity=Mathf.Lerp(defaultFogDensity,fallFogDensity,lerp);
				RenderSettings.fogStartDistance=Mathf.Lerp(defaultFogStart,fallFogStart,lerp);
			}else{
				//WATER FOG
				float lerp=Mathf.Clamp((world.seaLevel-phite)/3.0f,0,1);
				RenderSettings.fogColor=Color.Lerp(defaultFogColor, waterFogColor, lerp);
				RenderSettings.fogDensity=Mathf.Lerp(defaultFogDensity,waterFogDensity,lerp);
				RenderSettings.fogStartDistance=Mathf.Lerp(defaultFogStart,waterFogStart,lerp);
				RenderSettings.fogEndDistance=Mathf.Lerp(defaultFogEnd,waterFogEnd,lerp);
			}
		}
		
		if(Input.GetKey(KeyCode.Space)){
		//if(ptransform.position.y>10.0f){
			//cmotor.SetVelocity(cmotor.buoyantForce(ptransform.position.y-20.0f);
		//}
		//	Vector3 tmp=ptransform.position;
		//	tmp.y+=0.1f;
		//	ptransform.position=tmp;
		}
//			body.velocity.Set (body.velocity.x,body.velocity.y+10.0f,body.velocity.z);
//			characterMotor.gravity=0.0f;
//		else
//			characterMotor.gravity=10.0f;
//		else
//			characterMotor.gravity=20.0f;
	}
}
