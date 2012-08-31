using UnityEngine;
using System.Collections;

public class MenuGUI : MonoBehaviour
{

	public void OnSelected (bool on)
	{
		if (on) {
			renderer.material.shader = Shader.Find (" Glossy");
			renderer.material.SetColor ("_Color", Color.red);
//			iTween.MoveTo(gameObject,iTween.Hash ("z",-1,"time",1));
		} else if (!on) {
			renderer.material.shader = Shader.Find (" Glossy");
			renderer.material.SetColor ("_Color", Color.white);
			//	iTween.MoveTo(gameObject,iTween.Hash ("z",1,"time",1));
		}
	}

}
