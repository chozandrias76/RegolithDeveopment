using System;
using System.Collections.Generic;
using UnityEngine;


public class RenderStateTracker
{
	GameObject						currentObject;
	List<Material>					solidMaterials=new List<Material>();
	List<Shader>					savedShaders=new List<Shader>();
	List<GameObject>				parts=new List<GameObject>();
	
	
	public void findChildren(GameObject obase,int cidx){
		if(cidx==0)
			parts.Add (obase);
		if(cidx>=obase.transform.GetChildCount())
			return;
		Transform child = obase.transform.GetChild(cidx);
		findChildren (child.gameObject,0);
		findChildren (obase,cidx+1);
	}
	
	public void configure(GameObject	gameObject){
		if(currentObject!=null)
			resetRenderState();
		
		currentObject=gameObject;
		solidMaterials.Clear();
		savedShaders.Clear();
		parts.Clear();
		findChildren (gameObject,0);
		foreach(GameObject obj in parts){
			obj.layer = LayerMask.NameToLayer ("Ignore Raycast");
			savedShaders.Add(obj.renderer.materials[0].shader);
			solidMaterials.Add(obj.renderer.materials[0]);
		}
	}
	
	public void resetRenderState(){		
		int sidx=0;
		foreach(Material mr in solidMaterials){
			mr.shader=savedShaders[sidx++];
			mr.color=new Color(1,1,1,1);
		}
	}
	
	public void	changeRenderState(Shader	shader,Color color){
		foreach(Material mr in solidMaterials){
			mr.shader=shader;
			mr.color=color;
		}
	}

	public RenderStateTracker ()
	{
	}
}


