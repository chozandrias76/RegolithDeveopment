using UnityEngine;
using System.Collections;

public class RotateLogo : MonoBehaviour {
	float	frame;
	float 	theta;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Quaternion q=new Quaternion();
		
		q.eulerAngles=new Vector3(-25.8f*Mathf.Sin((theta*0.02f)),1.0f*theta,-20.8f*Mathf.Sin((theta*0.025f)));
		float tdt=Time.deltaTime;
		if((int)frame%80>73)
			theta+=120.0f*tdt;
		else theta+=3.0f*tdt;
		frame+=tdt*10.0f;
		transform.rotation=q;
	}
}
