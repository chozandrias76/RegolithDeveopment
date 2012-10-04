using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class World
{
	PangoWorld			root;
	static int 			MAX_VERTS = 65000;
	static int 			MAX_TRIS = 666000;
	int 				curVert = 0;
	int 				curIndex = 0;
	static int 			chunkDim = 20;
	float 				noiseRes = 0.03f;
	float 				matNoiseRes = 0.1f;
	int 				planarSweep = 3;
	int 				depthSweep = 3;
	
	int					viewRX;
	int					viewRY;
	int					viewRZ;
	public string 		regionName;
	
	static System.Random 	rand=new System.Random();
	Vector3[] 			vertices = new Vector3[MAX_VERTS];//(chunkDim+1)*(chunkDim+1)];
	Vector3[] 			normals = new Vector3[MAX_VERTS];//(chunkDim+1)*(chunkDim+1)];
	Vector2[] 			uvs = new Vector2[MAX_VERTS];//[(chunkDim+1)*(chunkDim+1)];
	int[] 				indices = new int[MAX_TRIS];//[chunkDim*chunkDim * 6];
	
	
	static int cacheRowSize=(chunkDim+3);
	static int ox=(cacheRowSize*cacheRowSize);
	static int oy=cacheRowSize;
	static int oz=1;
	MCubes.GRIDCELL cell = new MCubes.GRIDCELL ();
	
	static int	chunkVertexCount = (chunkDim+3)*(chunkDim+3)*(chunkDim+3);
	//GridVertex[]	chunkVertices=new GridVertex[chunkVertexCount];
	
	Dictionary<string,Region> 	loadedRegions = new Dictionary<string, Region>();
	public List<Region>			loadingQueue = new List<World.Region>();
	public List<Region>			geometryQueue = new List<World.Region>();
	
	Dictionary<Region,bool>		visibleMap = new Dictionary<Region, bool>();
	
	public void loadWorld(){
		
		return;
		FileStream stream = new FileStream("world/regions.bin", FileMode.Open);
		BinaryReader br = new BinaryReader(stream);
		
		int rct=br.Read ();
		for(int i=0;i<rct;i++){
			int rx=br.Read();
			int ry=br.Read();
			int rz=br.Read();
			int dlen=br.Read();
			Region r=getRegion(rx,ry,rz);
			r.data=new World.GridVertex[dlen];
			for(int t=0;t<dlen;t++){
				r.data[t].density=br.Read();
				r.data[t].material.x=br.Read();
				r.data[t].material.y=br.Read();
			}
			r.modified=true;
		}
        br.Close();
	}
	
	public void saveWorld(){
		return;
		//Regions - datas
		//Directory.CreateDirectory("world");
		FileStream stream = new FileStream("world/regions.bin", FileMode.Create);
		BinaryWriter bw = new BinaryWriter(stream);
		int rgnCount=0;
		foreach( Region r in loadedRegions.Values ){
			if(r.modified){// && r.data!=null){
				rgnCount++;
			}
		}
		bw.Write(rgnCount);
		foreach( Region r in loadedRegions.Values ){
			if(r.modified){// && r.data!=null){
				bw.Write(r.coord[0]);
				bw.Write(r.coord[1]);
				bw.Write(r.coord[2]);
				bw.Write(r.data.Length);
				for(int t=0;t<r.data.Length;t++){
					bw.Write(r.data[t].density);
					bw.Write(r.data[t].material.x);
					bw.Write(r.data[t].material.y);
				}
			}
		}
        //writer.Write("hello");
        //writer.Write(5);
        bw.Close();
		//File inFile=File.Open("myfile.bin",FileMode.Create);
		
		//File.WriteAllText("myfile.txt","mytext");
	}
	
	
	public struct GridVertex
	{
		public float 	density;
		public Vector2	material;
		public Vector3	position;
		public Vector3 	normal;	
	}
	
	public class Modification
	{
		public Vector3 	position;
		public float  	strength;
		public float 	radius;
		public World.ToolFunction		tool;
	}
	
	public class Region
	{
		public string 				key;
		public GameObject			obj;
		public GridVertex[]			data;
		public int[]				coord;
		public Vector3				origin;
		public bool					inLoadQueue;
		public bool					inGeomQueue;
		public bool					isVisible;
		public bool					modified;
		public List<Modification>	modifications=new List<Modification>();
		public List<Unit>			units=new List<Unit>();
		public Region(string _key){
			key=_key;
		}
	};
	
	public static float frand(){
		return (float)rand.NextDouble();
	}
	
	public static float fmax(float a,float b){
		return a>b?a:b;
	}
	
	public static float fmin(float a,float b){
		return a<b?a:b;
	}
	static float sin (float val)
	{
		return (float)Math.Sin ((double)val);
	}

	static float cos (float val)
	{
		return (float)Math.Cos ((double)val);
	}
	
	void capVolumeDomain(int rx,int ry,int rz,ref MCubes.GRIDCELL cell){
		if(rx==0)cell.val[0]=cell.val[3]=cell.val[4]=cell.val[7]=0.0;
		else if(rx==chunkDim-1)cell.val[1]=cell.val[2]=cell.val[5]=cell.val[6]=0.0;
		if(ry==0)cell.val[0]=cell.val[1]=cell.val[4]=cell.val[5]=0.0;
		else if(ry==chunkDim-1)cell.val[2]=cell.val[3]=cell.val[6]=cell.val[7]=0.0;
		if(rz==0)cell.val[0]=cell.val[1]=cell.val[2]=cell.val[3]=0.0;
		else if(rz==chunkDim-1)cell.val[4]=cell.val[5]=cell.val[6]=cell.val[7]=0.0;
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
	
	float sphereDensity(float nx,float ny,float nz,float radius){
		return (1.0f-fmin(1,Mathf.Sqrt((nx*nx)+(ny*ny)+(nz*nz))/radius));
	}

	float columnDensity(float nx,float ny,float radius){
		return (1.0f-fmin(1,Mathf.Sqrt((nx*nx)+(ny*ny))/radius));
	}
	
	float evaluateFieldFirstGeneration(ref float nx,ref float ny,ref float nz)
	{
		float densityVariance=1.0f;//((float)SimplexNoise.noise (nx*densityVarianceScale, ny*densityVarianceScale, nz*densityVarianceScale)-0.5f)*-0.5f;
		float nv = (float)SimplexNoise.noise (nx, ny, nz)*densityVariance;
		float nv2 = (float)SimplexNoise.noise (nx*6.1f, ny*6.1f, nz*6.1f);
		//gv = ny;
		nv+=(ny*-1.4f);	//Add ground by subtracting the Y value from the density... the higher this value, the flatter the terrain
		//Add noise
		nv=nv+(nv2*0.1f); //Add some bumpier noise
		nv=fmin(fmax(nv,0.0f),1.0f); ///Clamp density to 0-1 range..
		
		ny+=1.0f; //Offset the Y to make bowl shape appear lower
		return nv * sphereDensity(nx,ny,nz,16.0f);	//Add in bowl shaped field to make bowl planetoid...
	}
	
	float fnclamp(float val){
		return val<0.0f?0.0f:val>1.0f?1.0f:val;
	}
	
	float featureTest(float minx,float miny,float minz,float maxx,float maxy,float maxz,float nv,float nx,float ny,float nz){	
		if(nx>minx&&ny>miny&&nz>minz&&nx<maxx&&ny<maxy&&nz<maxz){
			nx=((nx-minx)/(maxx-minx));
			ny=((ny-miny)/(maxy-miny));
			nz=((nz-minz)/(maxz-minz));
			nv+=nx-0.5f;
			nv+=ny-0.5f;
			nv+=nz-0.5f;
			
			float pny=-0.7f;
			float pnx=-0.4f;
			float pnz=-0.1f;

	//		nv+=(nx*pnx)+(ny*pny)+(nz*pnz);
		}
		return nv;
	}
	
	float evaluateFieldTowerFormation(ref float nx,ref float ny,ref float nz)
	{
		float densityVariance=1.0f;//((float)SimplexNoise.noise (nx*densityVarianceScale, ny*densityVarianceScale, nz*densityVarianceScale)-0.5f)*-0.5f;
		float nv = (float)SimplexNoise.noise (nx, ny, nz)*densityVariance;
		float nv2 = (float)SimplexNoise.noise (nx*6.1f, ny*6.1f, nz*6.1f);
		//gv = ny;
		nv+=(ny*-1.4f);	//Add ground by subtracting the Y value from the density... the higher this value, the flatter the terrain
		//Add noise
		nv=nv+(nv2*0.1f); //Add some bumpier noise
		nv=fnclamp(nv);
		
		ny+=1.0f; //Offset the Y to make bowl shape appear lower
		float sphere=fnclamp(sphereDensity(nx,ny,nz,16.0f));
		float column=fnclamp(columnDensity(nx+0.5f,nz+0.5f,0.2f));
		nv+=column;
		//column += (nv2*0.2f);//Add noise to the column
		
		nv *= sphere;//Add in bowl shaped field to make bowl planetoid...
		
		float fox=-0.5f;
		float foy= 1.0f;
		float foz=-0.5f;
		float rad=0.5f;
		nv=featureTest (fox-rad,foy-rad,foz-rad,fox+rad,foy+rad,foz+rad,nv,nx,ny,nz);
		return fmin(fmax(nv,0.0f),1.0f); ///Clamp density to 0-1 range..
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
	
	float snoise(float x,float y,float z){
		return (float)SimplexNoise.noise(x,y,z);
	}
	
	void initializeField(int x, int y, int z,ref GridVertex[]	chunkVertices){	
		int vtop=0;
		//Build the procedural field for this chunk...
		for (int rx=-1; rx<=chunkDim+1; rx++)
			for (int ry=-1; ry<=chunkDim+1; ry++)
				for (int rz=-1; rz<=chunkDim+1; rz++)
				{
					chunkVertices[vtop].position.Set(rx, ry, rz);
				
					float nx = (x + rx) * noiseRes;
					float ny = (y + ry) * noiseRes;
					float nz = (z + rz) * noiseRes;
					
				//	float d= 
					chunkVertices[vtop].density = evaluateFieldTowerFormation(ref nx,ref ny,ref nz);

					float n2sz=matNoiseRes;
					chunkVertices[vtop].material.Set (//0.7f,
						(snoise (nx*n2sz, ny*n2sz, 0.0f)*0.7f)+
						(snoise (ny*3.0f, nx*3.0f, 0.0f)*0.3f)
						//(snoise (nx*n2sz, ny*n2sz, 0.0f)*0.7f)+
						//(snoise (ny*(n2sz*2), nx*(n2sz*2), 0.0f)*0.3f)
						,
						snoise (ny*n2sz, nx*n2sz, 0.0f));
					
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
	
	
	static float fabs(float f){return f<0.0f?-1.0f*f:f;}
	
	public delegate float ToolFunction(ref GridVertex g,Vector3	dlt,float editRadius,float er2,float targDelta);
	
	public static float 	cubeTool(ref GridVertex g,Vector3	dlt,float editRadius,float er2,float targDelta){
		//Cube tool...
		float d=g.density;
		if(//	g.material.x>0.4f&&
			fabs(dlt.x)<editRadius&&
			fabs(dlt.y)<editRadius&&
			fabs(dlt.z)<editRadius){
			d=d+targDelta;
			d = dclamp(d+targDelta);
			g.density=d;
		}
		return d;
	}
	
	public static float 	sphereTool(ref GridVertex g,Vector3	dlt,float editRadius,float er2,float targDelta){
		//Sphere tool...
		float d=g.density;
		if(//g.material.x>0.4f&&
			dlt.sqrMagnitude<er2){
			// add/sub a sphere from the  density field
			//if(dlt.y<0.2f)	//Comment this in to get flat top sphere drawing
			d=dclamp(d+((1.0f-(dlt.magnitude/editRadius))*targDelta));	//Effect this element...
			g.density=d;
		}
		return d;
	}

	float modifyRegion(Region rgn,Vector3 origin,float editRadius,float targDelta,ToolFunction tool){
		int cbase=0;
		//ox+oy+oz;
		//cbase*=2;
		float densityChange=0.0f;
		float er2 = editRadius*editRadius;
		for (int rx=0; rx<=chunkDim+2; rx++){
			int cplane=cbase;
			for (int ry=0; ry<=chunkDim+2; ry++){
				int cidx=cplane;
				for (int rz=0; rz<=chunkDim+2; rz++) {
					
					Vector3 pos=rgn.data[cidx].position+rgn.origin;
					Vector3 dlt=pos-origin;
					float d=rgn.data[cidx].density;
					densityChange+=tool(ref rgn.data[cidx],dlt,editRadius,er2,targDelta)-d;
					
					//if(d>0.5f)
					//	rgn.data[cidx].material.x=1.5f;
					
					cidx+=oz;
				}
				cplane+=oy;
			}
			cbase+=ox;
		}
		return densityChange;
	}

	
	void generateGeometry (ref Region	chunk,ref Mesh mesh)
	{
		curVert = 0;
		curIndex = 0;
		int 			cbase=ox+oy+oz;
		GridVertex[]	chunkVertices=chunk.data;
		Vector3			originWorld=chunk.origin;
		bool 			firstTime=chunk.modified?false:true;
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
						cell.t[ax]=gv.material;
						cell.val[ax]=gv.density;
					}
					int triBase=curIndex;
					MCubes.Polygonise (cell, 0.5, ref vertices, ref normals, ref uvs, ref indices, ref curVert, ref curIndex);
					
					if(firstTime){
						if(triBase<curIndex){	//If a triangle is being emitted, check its normal.. if its suitably flat, do random chance of object spawn...
							GridVertex gv=chunkVertices[cidx+cacheNeighbors[0]];
							if(gv.normal.y>0.919f){
								//Flatish surface... potentially emit something..
								if(gv.normal.y<0.92f){
									if(originWorld.sqrMagnitude>(50*50) && (gv.material[0]>-0.4f)){
										GameObject go=(GameObject)GameObject.Instantiate(Resources.Load("Objs/TurretBase"));//CreatePrimitive (PrimitiveType.Sphere);
										go.transform.position=gv.position+originWorld;
										Vector3 wid=new Vector3(gv.normal.x,0.0f,gv.normal.z);
										wid.Normalize();
										go.transform.rotation.SetLookRotation(Vector3.Cross(wid,gv.normal).normalized,gv.normal);
										
										go.GetComponent<Turret>().enabled=true;
									}									
								}
							}
	
						}
					}				
					cidx+=oz;
				}
				cplane+=oy;
			}
			cbase+=ox;
		}
		if(firstTime)
			chunk.modified=true;
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
	
	
	void	buildRegionData(GameObject archetype,ref Region chunk){
		int[] ccoord=chunk.coord;
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
	}
	
	void generateMesh(Region chunk, GameObject archetype)
	{
		if(chunk.obj==null){
			chunk.obj = new GameObject(chunk.key);
			chunk.obj.transform.position = chunk.origin = new Vector3 (chunk.coord[0] * chunkDim, chunk.coord[1] * chunkDim, chunk.coord[2] * chunkDim);			
			chunk.obj.AddComponent<MeshRenderer>();
			chunk.obj.AddComponent<MeshFilter>();
			chunk.obj.AddComponent<MeshCollider>();
			//chunk.obj.tag = "Finish";
			chunk.obj.layer = LayerMask.NameToLayer ("Terrain");
		}

		generateNormalField(ref chunk.data);
		Mesh mesh = new Mesh();
		generateGeometry(ref chunk,ref mesh);
		chunk.obj.GetComponent<MeshFilter> ().mesh=mesh;
		chunk.obj.GetComponent<MeshCollider> ().sharedMesh = mesh;
		//chunk.AddComponent(chunk.GetComponent<"Material">().);
		//Material mat = new Material (Shader.Find ("Diffuse"));
		chunk.obj.GetComponent<MeshRenderer> ().sharedMaterial = archetype.GetComponent<MeshRenderer> ().sharedMaterial;
	}
	
//	void	buildRegionMesh(GameObject archetype,ref Region chunk)
//	{s
//		generateMesh(chunk,archetype,ref chunk.data);
//	}
	static RaycastHit	rayHit=new RaycastHit();
	
	public static GameObject rayCast(Vector3 rfrom,Vector3 rto){
		return  rayCast(rfrom,rto,ref rayHit);
	}
	
	public static GameObject rayCast(Vector3 rfrom,Vector3 rto,ref RaycastHit hit){
		Vector3 raynormal=rto-rfrom;
		float dlt=raynormal.magnitude;
		raynormal/=dlt;
		if(Physics.Raycast(rfrom,raynormal,out hit,dlt,~0)==true){
			return hit.collider.gameObject;
		}
		return null;
	}
	
	public string getRegionName(int x,int y,int z){
		return "ck" + x + ":" + y + ":" + z;
	}
	
	public Region getRegion (int[] coord)
	{
		return getRegion (coord[0],coord[1],coord[2]);
	}
	
	public Region getRegion (int x, int y, int z)
	{
		GameObject archetype=root.gameObject;
		//Gets the chunk located at chunk coordinat x,y,z
		string ckName=getRegionName(x,y,z);
		Region chunk;
		if(loadedRegions.ContainsKey(ckName)){
			chunk=loadedRegions[ckName];
			if(chunk.data==null){
				if(chunk.inLoadQueue==false){
					loadingQueue.Add (chunk);
					chunk.inLoadQueue=true;
				}
			}else if(chunk.obj==null){				
				addRegionToGeometryQueue(chunk);
			}
		}
		else{
			chunk = new Region(ckName);
			chunk.coord=new int[]{x,y,z};
			chunk.inLoadQueue=true;
			loadingQueue.Add (chunk);
			loadedRegions[ckName]=chunk;
		}
		return chunk;
	}
	
	
	public bool isRegionLoaded(string name){
		return loadedRegions.ContainsKey(name);
	}
	
	//This updates the geometry generation and region generation/loading task pools...
	public void updateGenerator(){
		int	maxLoadsPerFrame=1;
		for(int t=0;t<maxLoadsPerFrame;t++){
			if(loadingQueue.Count>0){	//Generate/Load newly encountered regions...
				Region topObj=loadingQueue[loadingQueue.Count-1];
				loadingQueue.RemoveRange(loadingQueue.Count-1,1);			
				if(topObj.data==null){
					//Look for file here...
					buildRegionData(root.gameObject,ref topObj);
				}
				topObj.inLoadQueue=false;
				
				float sumDensity=0.0f;
				foreach(Modification mod in topObj.modifications){
					sumDensity+=modifyRegion(topObj,mod.position,mod.radius,mod.strength,mod.tool);
				}
				addRegionToGeometryQueue(topObj);
			}else
				break;
		}

		int	maxRegensPerFrame=1;
		for(int t=0;t<maxRegensPerFrame;t++){
			if(geometryQueue.Count>0){	//Prioritize geometry generation over new region generation..
				Region topObj=geometryQueue[geometryQueue.Count-1];
				geometryQueue.RemoveRange(geometryQueue.Count-1,1);
		
				
				generateMesh(topObj,root.gameObject);
				topObj.inGeomQueue=false;
			}else
				break;
		}
	}
	
	List<Region>	deadRegions=new List<Region>();
	
	//Destroys all regions in the dead region list and releases thier meshes..
	
	void destroyDeadRegions(){
		foreach(Region rg in deadRegions){
			loadingQueue.Remove(rg);
			geometryQueue.Remove(rg);
			rg.inGeomQueue=rg.inLoadQueue=false;
//			loadedRegions.Remove(rg.key);
			//if(rg.obj!=null)
			{
				//MeshCollider 	mc=rg.obj.GetComponent<MeshCollider>();
				//MeshFilter 		mf=rg.obj.GetComponent<MeshFilter>();
				//mc.sharedMesh = null;
				//Destroy(mf.mesh);
				//mf.mesh = null;
				PangoWorld.Destroy(rg.obj);
				rg.obj=null;
			}
		}
		if(deadRegions.Count>0){
			Resources.UnloadUnusedAssets();
			GC.Collect();
		}
		deadRegions.Clear();
	}

	public void	updateCache(){
		//Debug.Log ("Generating Terrain....");
		
		regionName=getRegionName(viewRX,viewRY,viewRZ);
		visibleMap.Clear();
		for (int tx=-planarSweep; tx<=planarSweep; tx++)
			for (int ty=-depthSweep; ty<=depthSweep; ty++)
				for (int tz=-planarSweep; tz<=planarSweep; tz++) {
					Region chunk=getRegion ( tx+viewRX, ty+viewRY, tz+viewRZ);
					visibleMap[chunk]=true;
					chunk.isVisible=true;
				}
		//	GetComponent<MeshFilter> ().mesh = mesh;
		//	GetComponent<MeshCollider>().sharedMesh = mesh;
		//Debug.Log("Terrain finished.");
		
		foreach(Region rg in loadedRegions.Values){
			if(visibleMap.ContainsKey(rg)==false){
				deadRegions.Add(rg);
				rg.isVisible=false;
			}
		}
		destroyDeadRegions();
	}
	
	static float dclamp(float density){
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
	
	public void getRgnCoord(Vector3 pos,ref int[] coord){
		coord[0]=(int)Math.Floor(pos.x/chunkDim);
		coord[1]=(int)Math.Floor(pos.y/chunkDim);
		coord[2]=(int)Math.Floor(pos.z/chunkDim);
	}
	int[] tmpMin = new int[3];
	int[] tmpMax = new int[3];
	
	List<Region>	regionCollisions=new List<Region>();
	
	void addRegionToGeometryQueue(Region r){
		if(r.inGeomQueue==false){
			geometryQueue.Add(r);
			r.inGeomQueue=true;
		}
	}
	
	public float	editWorld(GameObject	archetype,Vector3 point,float editRadius,float editValue,World.ToolFunction tool)
	{	
		
		bool validEdit=true;
		Modification mod=new Modification();
		mod.position=point;
		mod.radius=editRadius;
		mod.strength=editValue;
		mod.tool=tool;
		
		
		regionCollisions.Clear();
		getCollidingRegions(mod.position,mod.radius,ref regionCollisions);
		
		float sumDensity=0.0f;
		foreach(Region r in regionCollisions){
			if(r.data==null){
//				validEdit=false;
				//This may lose mineral data if edit happens when client doesnt have the region cached
				r.modifications.Add(mod);
				//addRegionToGeometryQueue(r);
				//break;
			}else{
				sumDensity+=modifyRegion(r,mod.position,mod.radius,mod.strength,mod.tool);
				addRegionToGeometryQueue(r);
			}
		}
		if(validEdit){
//			foreach(Region rgn in regionCollisions){
//				sumDensity+=modifyRegion(rgn,mod.position,mod.radius,mod.strength,mod.tool);
//				generateMesh(rgn,archetype);
				//hilightRegion(rgn,Color.blue,1.0f);
//			}
		}
		//Region chunk=getChunk (this.gameObject, viewRX, viewRY, viewRZ);
		//if(chunk!=null && chunk.data!=null){
		//	modifyChunk(ref chunk.data,ref chunk.origin);
		//	generateMesh(ref chunk,this.gameObject,ref chunk.data);
		//	Debug.Log ("Rebuilt chunk:"+chunk.key);
		//}
		return sumDensity;
	}
	
	
	void getCollidingRegions(Vector3 pmin,Vector3 pmax,ref List<Region>	colliders){
		
		getRgnCoord(pmin	,ref tmpMin);
		getRgnCoord(pmax	,ref tmpMax);
		for(int tx=tmpMin[0];tx<=tmpMax[0];tx++)
		for(int ty=tmpMin[1];ty<=tmpMax[1];ty++)
		for(int tz=tmpMin[2];tz<=tmpMax[2];tz++)
			colliders.Add(getRegion(tx,ty,tz));
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
	
	public void primeCache(){
		regionName=getRegionName(viewRX,viewRY,viewRZ);
		updateCache();
		while((loadingQueue.Count>0) || (geometryQueue.Count>0))
			updateGenerator();
	}
	
	public void update(){
		Vector3 cacheOrigin=(root.viewerObject!=null)?root.viewerObject.transform.position:Vector3.zero;
		getRgnCoord(cacheOrigin	,ref viewRgnCoord);
		
		if((viewRgnCoord[0]!=viewRX)||(viewRgnCoord[1]!=viewRY)||(viewRgnCoord[2]!=viewRZ)){
			viewRX=viewRgnCoord[0];
			viewRY=viewRgnCoord[1];
			viewRZ=viewRgnCoord[2];
			//Debug.Log ("RgnChange:"+viewRX+":"+viewRY+":"+viewRZ);
			//hilightRegion (viewerObject.transform.position,Color.red,0.5f);
			updateCache();
		}
	}
	/*
	protected static GameObject[] getAllEditorAssets()
    {
	    List<GameObject> tempObjects = new List<GameObject>();
	    DirectoryInfo directory = new DirectoryInfo(Application.dataPath);
	    FileInfo[] goFileInfo = directory.GetFiles("*.prefab", SearchOption.AllDirectories);
	    uint i = 0; 
		uint goFileInfoLength = goFileInfo.length;
	    FileInfo tempGoFileInfo; 
		String tempFilePath;
		int assetIndex;
	    GameObject tempGO;
	    for(i = 0; i < goFileInfoLength; i++)
	    {
	        tempGoFileInfo = goFileInfo[i] as FileInfo;
	        if(tempGoFileInfo == null) continue;            
	        tempFilePath = tempGoFileInfo.FullName;
	
	        assetIndex = tempFilePath.IndexOf("Assets/");
	        //assetIndex = tempFilePath.IndexOf("Assets\\");
	        if (assetIndex < 0) assetIndex = 0;         
	        tempFilePath = tempFilePath.Substring(assetIndex, tempFilePath.length - assetIndex);
	        //tempFilePath = tempFilePath.Replace('\\', '/');
	        tempGO = AssetDatabase.LoadAssetAtPath(tempFilePath, GameObject) as GameObject;
	        if(tempGO == null) continue;
	        tempObjects.push(tempGO);
	    }
	    return tempObjects.ToBuiltin(GameObject) as GameObject[];
	}
	*/
	
	public World (PangoWorld _root)
	{
//		UnityEngine.Object[]	rezzes=Resources.FindObjectsOfTypeAll(typeof( Mesh));
//		UnityEngine.Object[]	meshes=GameObject.FindObjectsOfTypeIncludingAssets(typeof( Mesh));
		
		root=_root;
	}
}
