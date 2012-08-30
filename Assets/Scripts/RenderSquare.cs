using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//Awww ya cool.

//big dongs

public class RenderSquare : MonoBehaviour
{
	static int 		MAX_VERTS = 65000;
	static int 		MAX_TRIS = 666000;
	int 		curVert = 0;
	int 		curIndex = 0;
	static int 	chunkDim = 20;
	float 		noiseRes = 0.01f;
	int 		planarSweep = 3;
	int 		depthSweep = 3;
	
	Vector3[] 	vertices = new Vector3[MAX_VERTS];//(chunkDim+1)*(chunkDim+1)];
	Vector3[] 	normals = new Vector3[MAX_VERTS];//(chunkDim+1)*(chunkDim+1)];
	Vector2[] 	uvs = new Vector2[MAX_VERTS];//[(chunkDim+1)*(chunkDim+1)];
	int[] 		indices = new int[MAX_TRIS];//[chunkDim*chunkDim * 6];
	
	
	static int cacheRowSize=(chunkDim+3);
	static int ox=(cacheRowSize*cacheRowSize);
	static int oy=cacheRowSize;
	static int oz=1;
	
	MCubes.GRIDCELL cell = new MCubes.GRIDCELL ();
	
	struct GridVertex
	{
		public float 	density;
		public Vector3	position;
		public Vector3 	normal;
	}
	static int	chunkVertexCount = (chunkDim+3)*(chunkDim+3)*(chunkDim+3);
	//GridVertex[]	chunkVertices=new GridVertex[chunkVertexCount];
	
	RaycastHit 		hit = new RaycastHit();
	System.Random 	rand=new System.Random();
	
	float frand(){
		return (float)rand.NextDouble();
	}
	
	Dictionary<string,Region> 	loadedRegions = new Dictionary<string, Region>();
	List<Region>				loadingQueue = new List<Region>();
	
	Dictionary<Region,bool>		visibleMap = new Dictionary<Region, bool>();
	
	int			viewRX;
	int			viewRY;
	int			viewRZ;
	string 		regionName;
	GameObject	viewerObject;
	
	
	
	float sin (float val)
	{
		return (float)Math.Sin ((double)val);
	}

	float cos (float val)
	{
		return (float)Math.Cos ((double)val);
	}
	
	void Reset ()
	{
	}

	void capVolumeDomain(int rx,int ry,int rz,ref MCubes.GRIDCELL cell){
		if(rx==0)cell.val[0]=cell.val[3]=cell.val[4]=cell.val[7]=0.0;
		else if(rx==chunkDim-1)cell.val[1]=cell.val[2]=cell.val[5]=cell.val[6]=0.0;
		if(ry==0)cell.val[0]=cell.val[1]=cell.val[4]=cell.val[5]=0.0;
		else if(ry==chunkDim-1)cell.val[2]=cell.val[3]=cell.val[6]=cell.val[7]=0.0;
		if(rz==0)cell.val[0]=cell.val[1]=cell.val[2]=cell.val[3]=0.0;
		else if(rz==chunkDim-1)cell.val[4]=cell.val[5]=cell.val[6]=cell.val[7]=0.0;
	}
	
	float fmax(float a,float b){
		return a>b?a:b;
	}
	
	float fmin(float a,float b){
		return a<b?a:b;
	}
	
	
	float evaluateFieldOriginal(ref float nx,ref float ny,ref float nz)
	{
		float densityVariance=1.0f;//((float)SimplexNoise.noise (nx*densityVarianceScale, ny*densityVarianceScale, nz*densityVarianceScale)-0.5f)*-0.5f;
		float noiseScl=0.2f;//((float)SimplexNoise.noise (nx*densityVarianceScale, ny*densityVarianceScale, nz*densityVarianceScale)-0.5f)*-0.5f;
		float nv = (float)SimplexNoise.noise (nx, ny, nz)*densityVariance;
		float nv2 = (float)SimplexNoise.noise (nx*noiseScl, ny*noiseScl, nz*noiseScl);
		//gv = ny;		//Add ground
		nv+=(ny*-3.4f);
		//Add noise
		nv=nv+(nv2*0.5f);
		return fmin(fmax(nv,0.0f),1.0f);
	}
	
	float evaluateFieldNiceTerrain(ref float nx,ref float ny,ref float nz)
	{
		float densityVariance=1.0f;//((float)SimplexNoise.noise (nx*densityVarianceScale, ny*densityVarianceScale, nz*densityVarianceScale)-0.5f)*-0.5f;
		float nv = (float)SimplexNoise.noise (nx, ny, nz)*densityVariance;
		float nv2 = (float)SimplexNoise.noise (nx*6.1f, ny*6.1f, nz*6.1f);
		//gv = ny;		//Add ground
		nv+=(ny*-1.4f);
		//Add noise
		nv=nv+(nv2*0.1f);
		return fmin(fmax(nv,0.0f),1.0f);
	}
	
	float evaluateField(ref float nx,ref float ny,ref float nz)
	{
		float densityVariance=1.0f;//((float)SimplexNoise.noise (nx*densityVarianceScale, ny*densityVarianceScale, nz*densityVarianceScale)-0.5f)*-0.5f;
		float nv = (float)SimplexNoise.noise (nx, ny, nz)*densityVariance;
		float nv2 = (float)SimplexNoise.noise (nx*6.1f, ny*6.1f, nz*6.1f);
		//gv = ny;		//Add ground
		nv+=(ny*-1.4f);
		//Add noise
		nv=nv+(nv2*0.1f);
		return fmin(fmax(nv,0.0f),1.0f);
	}
	
	Vector3[]	v3subarray(ref Vector3[]	v,int len){
		Vector3[] gen=new Vector3[len];
		Array.Copy (v,gen,len);
		return gen;
	}
	
	Vector2[]	v2subarray(ref Vector2[]	v,int len){
		Vector2[] gen=new Vector2[len];
		Array.Copy (v,gen,len);
		return gen;
	}
	
	int[]	vIntsubarray(ref int[]	v,int len){
		int[] gen=new int[len];
		Array.Copy (v,gen,len);
		return gen;
	}
	
	Vector3	vecD3(double dx,double dy,double dz){
		return new Vector3((float)dx,(float)dy,(float)dz);
	}
	
	void initializeField(int x, int y, int z,ref GridVertex[]	chunkVertices){	
		int vtop=0;
		//Build the procedural field for this chunk...
		for (int rx=-1; rx<=chunkDim+1; rx++)
			for (int ry=-1; ry<=chunkDim+1; ry++)
				for (int rz=-1; rz<=chunkDim+1; rz++)
				{
					chunkVertices[vtop].position = new Vector3 (rx, ry, rz);
				
					float nx = (x + rx) * noiseRes;
					float ny = (y + ry) * noiseRes;
					float nz = (z + rz) * noiseRes;
					chunkVertices[vtop].density = evaluateField(ref nx,ref ny,ref nz);
					vtop++;
				}
	}
	
	static int caddr(int cx,int cy,int cz){
		return (cx*cacheRowSize*cacheRowSize)+(cy*cacheRowSize)+cz;
	}
	static int[] normalNeighbors={
		caddr(0,0,-1),caddr(0,0,1),
		caddr(0,-1,0),caddr(0,1,0),
		caddr(-1,0,0),caddr(1,0,0)
	};
	static	int[] cacheNeighbors={
		caddr(0,0,0),caddr(1,0,0),caddr(1,1,0),caddr(0,1,0),
		caddr(0,0,1),caddr(1,0,1),caddr(1,1,1),caddr(0,1,1)
	};
	
	void generateNormalField(ref GridVertex[]	chunkVertices){
		
		int cbase=ox+oy+oz;
		
		//Compute normals on volume verts...	
		for (int rx=0; rx<=chunkDim; rx++){
			int cplane=cbase;
			for (int ry=0; ry<=chunkDim; ry++){
				int cidx=cplane;
				for (int rz=0; rz<=chunkDim; rz++)
				{
					float nmlx=chunkVertices[cidx+normalNeighbors[0]].density-chunkVertices[cidx+normalNeighbors[1]].density;
					float nmly=chunkVertices[cidx+normalNeighbors[2]].density-chunkVertices[cidx+normalNeighbors[3]].density;
					float nmlz=chunkVertices[cidx+normalNeighbors[4]].density-chunkVertices[cidx+normalNeighbors[5]].density;
					chunkVertices[cidx].normal=new Vector3(nmlx,nmly,nmlz);
					chunkVertices[cidx].normal.Normalize();
					cidx+=oz;
				}
				cplane+=oy;
			}
			cbase+=ox;
		}
	}
	
	void modifyRegion(Region rgn,Vector3 origin,float editRadius,float targDelta){
		int cbase=0;//ox+oy+oz;
		//cbase*=2;
		
		float er2 = editRadius*editRadius;
		for (int rx=0; rx<=chunkDim+2; rx++){
			int cplane=cbase;
			for (int ry=0; ry<=chunkDim+2; ry++){
				int cidx=cplane;
				for (int rz=0; rz<=chunkDim+2; rz++) {
					Vector3 pos=rgn.data[cidx].position+rgn.origin;
					Vector3 dlt=pos-origin;
					if(dlt.sqrMagnitude<er2){
						float d=rgn.data[cidx].density;
						d=d+((1.0f-(dlt.magnitude/editRadius))*targDelta);	//Effect this element...
						rgn.data[cidx].density=dclamp (d);
					}
					cidx+=oz;
				}
				cplane+=oy;
			}
			cbase+=ox;
		}
	}

	
	void generateChunk (ref GridVertex[]	chunkVertices,ref Mesh mesh)
	{
		curVert = 0;
		curIndex = 0;
		int cbase=ox+oy+oz;
		
		//Loop through the density volume and call marching cubes on each cube... 
		for (int rx=0; rx<chunkDim; rx++){
			int cplane=cbase;
			for (int ry=0; ry<chunkDim; ry++){
				int cidx=cplane;
				for (int rz=0; rz<chunkDim; rz++) {
					for(int ax=0;ax<8;ax++){
						GridVertex gv=chunkVertices[cidx+cacheNeighbors[ax]];
						cell.p[ax]=gv.position;
						cell.n[ax]=gv.normal;
						cell.val[ax]=gv.density;
					}
					MCubes.Polygonise (cell, 0.5, ref vertices, ref normals, ref uvs, ref indices, ref curVert, ref curIndex);
					cidx+=oz;
				}
				cplane+=oy;
			}
			cbase+=ox;
		}
		
		//Make a mesh to hold this chunk and assign the verts/tris
		mesh.Clear(true);
		mesh.vertices = v3subarray(ref vertices,curVert);
		mesh.uv = v2subarray(ref uvs,curVert);
		mesh.normals = v3subarray(ref normals,curVert);
		mesh.triangles = vIntsubarray(ref indices,curIndex);
		//mesh.RecalculateNormals ();
		
	}
	/*
	Mesh generateChunk2 (int x, int y, int z)
	{
		
		curVert = 0;
		curIndex = 0;
		
		for (int rx=0; rx<chunkDim; rx++)
			for (int ry=0; ry<chunkDim; ry++)
				for (int rz=0; rz<chunkDim; rz++) {
			
					cell.p [0] = new Vector3 (rx, ry, rz);
					cell.p [1] = new Vector3 (rx + 1, ry, rz);
					cell.p [2] = new Vector3 (rx + 1, ry + 1, rz);
					cell.p [3] = new Vector3 (rx, ry + 1, rz);
					cell.p [4] = new Vector3 (rx, ry, rz + 1);
					cell.p [5] = new Vector3 (rx + 1, ry, rz + 1);
					cell.p [6] = new Vector3 (rx + 1, ry + 1, rz + 1);
					cell.p [7] = new Vector3 (rx, ry + 1, rz + 1);

					for (int t=0; t<8; t++) {
						float nx = (cell.p [t].x + x) * noiseRes;
						float ny = (cell.p [t].y + y) * noiseRes;
						float nz = (cell.p [t].z + z) * noiseRes;
				
						cell.val [t] = evaluateField(ref nx,ref ny,ref nz);
				
						cell.n[t] = new Vector3(0,1,0);//evaluateFieldNormal(ref nx,ref ny,ref nz);
						//Following code caps the volume at the edge of the chunk...
						//capVolumeDomain(rx,ry,rz,ref cell);
					}
					MCubes.Polygonise2 (cell, 0.5, ref vertices, ref normals, ref uvs, ref indices, ref curVert, ref curIndex);
				}
		
		
		//Make a mesh to hold this chunk and assign the verts/tris
		Mesh mesh = new Mesh ();
		
		mesh.vertices = v3subarray(ref vertices,curVert);
		mesh.uv = v2subarray(ref uvs,curVert);
		mesh.triangles = vIntsubarray(ref indices,curIndex);
		mesh.normals = v3subarray(ref normals,curVert);

		//mesh.RecalculateNormals ();
		
		return mesh;
	}
	*/
	
	class Region
	{
		public string 		key;
		public GameObject	obj;
		public GridVertex[]	data;
		public int[]		coord;
		public Vector3		origin;
		public Region(string _key){
			key=_key;
		}
	};
	
	void generateMesh(Region chunk,GameObject archetype,ref GridVertex[]	chunkData)
	{
		generateNormalField(ref chunkData);
		Mesh mesh = new Mesh();
		generateChunk(ref chunkData,ref mesh);
		chunk.obj.GetComponent<MeshFilter> ().mesh=mesh;
		chunk.obj.GetComponent<MeshCollider> ().sharedMesh = mesh;
		//chunk.AddComponent(chunk.GetComponent<"Material">().);
		//Material mat = new Material (Shader.Find ("Diffuse"));
		chunk.obj.GetComponent<MeshRenderer> ().sharedMaterial = archetype.GetComponent<MeshRenderer> ().sharedMaterial;
	}
	
	void	buildChunk(GameObject archetype,ref Region chunk){
		int[] ccoord=chunk.coord;
		chunk.obj.AddComponent<MeshRenderer>();
		chunk.obj.AddComponent<MeshFilter>();
		chunk.obj.AddComponent<MeshCollider>();
		chunk.obj.tag = "Finish";
		int x=ccoord[0];
		int y=ccoord[1];
		int z=ccoord[2];
		//Generate the mesh from simplex noise terrain algorithm...
		
		int rgnx=x*chunkDim;
		int rgny=y*chunkDim;
		int rgnz=z*chunkDim;
		if(chunk.data==null)
		{	//Generate the field data for this chunk for the first time...
			chunk.data=new GridVertex[chunkVertexCount];
			
			initializeField(rgnx,rgny,rgnz,ref chunk.data);
		}
		generateMesh(chunk,archetype,ref chunk.data);
	}
	
	string getRegionName(int x,int y,int z){
		return "ck" + x + ":" + y + ":" + z;
	}
	
	Region getChunk (GameObject archetype, int x, int y, int z)
	{
		//Gets the chunk located at chunk coordinat x,y,z
		string ckName=getRegionName(x,y,z);
		
		if(loadedRegions.ContainsKey(ckName))
			return loadedRegions[ckName];
		
		Region chunk = new Region(ckName);
		chunk.obj = new GameObject(ckName);
		chunk.obj.transform.position = chunk.origin = new Vector3 (x * chunkDim, y * chunkDim, z * chunkDim);
		
		chunk.coord=new int[]{x,y,z};
		
		loadedRegions[ckName]=chunk;
		loadingQueue.Add(chunk);
		return chunk;
	}
	
	
	bool regionLoaded(string name){
		return loadedRegions.ContainsKey(name);
	}
	
	void updateGenerator(){
		if(loadingQueue.Count>0){
			Region topObj=loadingQueue[loadingQueue.Count-1];
			buildChunk(gameObject,ref topObj);
			loadingQueue.RemoveRange(loadingQueue.Count-1,1);
		}else{
		}
	}
	
	List<Region>	deadRegions=new List<Region>();
	void destroyDead(){
		foreach(Region rg in deadRegions){
			loadingQueue.Remove(rg);
			loadedRegions.Remove(rg.key);
			//if(rg.obj!=null)
			{
				MeshCollider 	mc=rg.obj.GetComponent<MeshCollider>();
				MeshFilter 		mf=rg.obj.GetComponent<MeshFilter>();
				//mc.sharedMesh = null;
				//Destroy(mf.mesh);
				//mf.mesh = null;
				Destroy(rg.obj);
				rg.obj=null;
			}
		}
		if(deadRegions.Count>0){
			Resources.UnloadUnusedAssets();
			GC.Collect();
		}
		deadRegions.Clear();
	}
	void	updateCache(){
		//Debug.Log ("Generating Terrain....");
		
		regionName=getRegionName(viewRX,viewRY,viewRZ);
		visibleMap.Clear();
		for (int tx=-planarSweep; tx<=planarSweep; tx++)
			for (int ty=-depthSweep; ty<=depthSweep; ty++)
				for (int tz=-planarSweep; tz<=planarSweep; tz++) {
					Region chunk=getChunk (this.gameObject, tx+viewRX, ty+viewRY, tz+viewRZ);
					visibleMap[chunk]=true;
				}
		//	GetComponent<MeshFilter> ().mesh = mesh;
		//	GetComponent<MeshCollider>().sharedMesh = mesh;
		//Debug.Log("Terrain finished.");
		
		foreach(Region rg in loadedRegions.Values){
			if(visibleMap.ContainsKey(rg)==false){
				deadRegions.Add(rg);
			}
		}
		destroyDead();
	}
	
	float dclamp(float density){
		//return density>0.5f?1.0f:0.0f;
		return fmin(1.0f,fmax(0.0f,density));
	}
	int[]	viewRgnCoord=new int[3];
	
		Vector3[] rgnCubeVerts=new Vector3[]{
			new Vector3(0,0,0),
			new Vector3(chunkDim,0,0),
			new Vector3(chunkDim,chunkDim,0),
			new Vector3(0,chunkDim,0),
			new Vector3(0,0,chunkDim),
			new Vector3(chunkDim,0,chunkDim),
			new Vector3(chunkDim,chunkDim,chunkDim),
			new Vector3(0,chunkDim,chunkDim)
		};
	int[]	rgnCubeIndices=new int[]{
		0,1,1,2,2,3,3,0,
		4,5,5,6,6,7,7,4,
		0,4,1,5,2,6,3,7
	};
	
	void getRgnCoord(Vector3 pos,ref int[] coord){
		coord[0]=(int)Math.Floor(pos.x/chunkDim);
		coord[1]=(int)Math.Floor(pos.y/chunkDim);
		coord[2]=(int)Math.Floor(pos.z/chunkDim);
	}
	int[] tmpMin = new int[3];
	int[] tmpMax = new int[3];
	
	List<Region>	regionCollisions=new List<Region>();
	
	void getCollidingRegions(Vector3 pmin,Vector3 pmax,ref List<Region>	colliders){
		
		getRgnCoord(pmin	,ref tmpMin);
		getRgnCoord(pmax	,ref tmpMax);
		for(int tx=tmpMin[0];tx<=tmpMax[0];tx++)
		for(int ty=tmpMin[1];ty<=tmpMax[1];ty++)
		for(int tz=tmpMin[2];tz<=tmpMax[2];tz++)
			colliders.Add(getChunk(this.gameObject,tx,ty,tz));
	}
	
	void getCollidingRegions(Vector3 pos,float radius,ref List<Region>	colliders){
		radius+=1.0f;	//Pad every search to get 
		Vector3 vrad=new Vector3(radius,radius,radius);
		Vector3 pmin=pos-vrad;
		Vector3 pmax=pos+vrad;
		getCollidingRegions(pmin,pmax,ref colliders);
	}
	
	void hilightRegion(Vector3 pos,Color color,float duration){
		getRgnCoord(pos	,ref viewRgnCoord);
		Vector3 rgnOrg=new Vector3(viewRgnCoord[0],viewRgnCoord[1],viewRgnCoord[2]) * chunkDim;
		for(int t=0;t<24;t+=2){
			Debug.DrawLine(rgnOrg+rgnCubeVerts[rgnCubeIndices[t]],rgnOrg+rgnCubeVerts[rgnCubeIndices[t+1]],color,duration);
		}
	}
	
	void hilightRegion(Region rgn,Color color,float duration){
		Vector3 rgnOrg=new Vector3(rgn.coord[0],rgn.coord[1],rgn.coord[2]) * chunkDim;
		for(int t=0;t<24;t+=2){
			Debug.DrawLine(rgnOrg+rgnCubeVerts[rgnCubeIndices[t]],rgnOrg+rgnCubeVerts[rgnCubeIndices[t+1]],color,duration);
		}
	}
	int		editCooldown=0;
	
	void Update ()
	{
		getRgnCoord(viewerObject.transform.position	,ref viewRgnCoord);
		
		//regionName=getRegionName(rx,ry,rz);
		
		if((viewRgnCoord[0]!=viewRX)||(viewRgnCoord[1]!=viewRY)||(viewRgnCoord[2]!=viewRZ)){
			viewRX=viewRgnCoord[0];
			viewRY=viewRgnCoord[1];
			viewRZ=viewRgnCoord[2];
			//Debug.Log ("RgnChange:"+viewRX+":"+viewRY+":"+viewRZ);
			//hilightRegion (viewerObject.transform.position,Color.red,0.5f);
			updateCache();
		}
		
		//viewerObject.camera.
		 Transform  cam = Camera.main.transform;
		 float 		editRadius=3.3f;
		 bool 		gotHit=Physics.Raycast (cam.position+(cam.forward*fmax(2.1f,editRadius)), cam.forward, out hit, 300);
		 if (gotHit && (Input.GetMouseButton(0)||Input.GetMouseButton(1)) && (editCooldown--==0)){
			editCooldown=1;
			
			//Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
	    	//Debug.DrawRay (ray.origin, ray.direction * 10, Color.yellow);
			// Update is called once per frame
			//if (Physics.Raycast(ray, out hit, 500.0f))
			{
				//if (hit.transform.tag != "Untagged") 
				{	
					//Debug.Log ("Hit:" + hit.transform.tag);
					//if (hit.transform.tag == "Finish")
					{
						//Debug.Log ("Fire the missiles");
						//float crsScl=0.25f;
						//Debug.DrawRay(cam.position, cam.forward*hit.distance,Color.green,5.0f);
						//Debug.DrawRay(hit.point+new Vector3(0,-crsScl,0), new Vector3(0,2*crsScl,0),Color.red,5.0f);
						//Debug.DrawRay(hit.point+new Vector3(-crsScl,0,0), new Vector3(2*crsScl,0,0),Color.red,5.0f);
						//Debug.DrawRay(hit.point+new Vector3(0,0,-crsScl), new Vector3(0,0,2*crsScl),Color.red,5.0f);
						
						
						
						GameObject psys = (GameObject)Instantiate(Resources.Load("Sparks"));
						psys.transform.position=hit.point;
						ParticleEmitter pe=(ParticleEmitter)psys.GetComponent("ParticleEmitter");
						pe.localVelocity=hit.normal*6.0f;
						Destroy(psys,0.5f);
						
						
						regionCollisions.Clear();
						
						getCollidingRegions(hit.point,editRadius,ref regionCollisions);
						bool validEdit=true;
						foreach(Region r in regionCollisions){
							if(r.data==null){
								Debug.Log ("Chunk not loaded, edit cancelled..");
								validEdit=false;
								break;
							}
						}
						if(validEdit){
							foreach(Region rgn in regionCollisions){
								modifyRegion(rgn,hit.point,editRadius,Input.GetMouseButton(0)?-1.0f:1.0f);
								generateMesh(rgn,this.gameObject,ref rgn.data);
								//hilightRegion(rgn,Color.blue,1.0f);
							}
						}
						//Region chunk=getChunk (this.gameObject, viewRX, viewRY, viewRZ);
						//if(chunk!=null && chunk.data!=null){
						//	modifyChunk(ref chunk.data,ref chunk.origin);
						//	generateMesh(ref chunk,this.gameObject,ref chunk.data);
						//	Debug.Log ("Rebuilt chunk:"+chunk.key);
						//}
					}
				}
			}
		}
		
		//Load/generate queued regions one per frame...
		updateGenerator();

		if(regionLoaded(regionName)==false)
			Time.timeScale=0.0f;
		else//Keep updating the players current region hasnt loaded yet..
			Time.timeScale=1.0f;
	}

	void Start ()
	{
		Debug.Log ("STARTE!!!!");
		viewerObject = GameObject.Find("First Person Controller");

		regionName=getRegionName(viewRX,viewRY,viewRZ);
		
		updateCache();
		while(loadingQueue.Count>0)
			updateGenerator();
	}
}