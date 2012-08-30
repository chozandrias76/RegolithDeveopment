using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("VoxCraft/Player/PC Stats")]
public class PC : BaseCharacter {
	private List<Item> inventory = new List<Item>();
	public List<Item> Inventory {
		get { return inventory; }
		set { inventory = value; }
	}

	public bool initialized = false;

	private static PC instance = null;
	public static PC Instance {
		get {
			if ( instance == null ) {
//				Debug.Log( "***PC - Instance***" );
				GameObject go = Instantiate( Resources.Load(
				                             GameSetting2.MALE_MODEL_PATH + GameSetting2.maleModels[ GameSetting2.LoadCharacterModelIndex() ] ),
				                           	 GameSetting2.LoadPlayerPosition(),
				                             Quaternion.identity ) as GameObject;
				
				PC temp = go.GetComponent<PC>();
				
				if( temp == null ){}
				Debug.LogError( "Player Prefab does not contain an PC script. Please add and configure." );
				
				instance = go.GetComponent<PC>();
				
				go.name = "PC";
				go.tag = "Player";
			}
			
			//return instance;
			return null;
		}
	}
	
	public void Initialize() {
//		Debug.Log( "***PC - Initialize***" );
		if( !initialized )
			LoadCharacter();
	}
	
	#region Unity functions
	public new void Awake() {
//		Debug.Log( "***PC - Awake***" );

		base.Awake();

		instance = this;
	}
	
//	public override void Awake() {
//		base.Awake();
//		
//		/**************************
//		Check out tutorial #140 and #141 to see how we got this weaponMount
//		**************************/
//		Transform weaponMount = transform.Find("base/spine/spine_up/right_arm/right_foret_arm/right_hand/weaponSlot");
//		
//		if(weaponMount == null) {
//			Debug.LogWarning("We could not find the weapon mount");
//			return;
//		}
//		
//		int count = weaponMount.GetChildCount();
//		
//		_weaponMesh = new GameObject[count];
//		
//		for(int cnt = 0; cnt < count; cnt++) {
//			_weaponMesh[cnt] = weaponMount.GetChild(cnt).gameObject;
//		}
//
//		HideWeaponMeshes();
//	}
//	
	//we do not want to be sending messages out each frame. We will be moving this out when we get back in to combat
	void Update() {
		Messenger<int, int>.Broadcast("player health update", 80, 100, MessengerMode.DONT_REQUIRE_LISTENER);
	}
	#endregion


	

	public Item EquipedShield {
		get { return _equipment[(int)EquipmentSlot.OffHand]; }
		set {
			_equipment[(int)EquipmentSlot.OffHand] = value;
			
			if( offHandMount.transform.childCount > 0 )
				Destroy( offHandMount.transform.GetChild( 0 ).gameObject );
				      
			if( _equipment[(int)EquipmentSlot.OffHand] != null) {
				GameObject mesh = Instantiate( Resources.Load( GameSetting2.SHIELD_MESH_PATH + _equipment[(int)EquipmentSlot.OffHand].Name ), offHandMount.transform.position, offHandMount.transform.rotation ) as GameObject;
				mesh.transform.parent = offHandMount.transform;
			}
		}
	}
	
	public Item EquipedHeadGear {
		get { return _equipment[(int)EquipmentSlot.Head]; }
		set {
			_equipment[(int)EquipmentSlot.Head] = value;
			
			if( helmetMount.transform.childCount > 0 )
				Destroy( helmetMount.transform.GetChild( 0 ).gameObject );
				      
			if( _equipment[(int)EquipmentSlot.Head] != null) {
				GameObject mesh = Instantiate( Resources.Load( GameSetting2.HAT_MESH_PATH + _equipment[(int)EquipmentSlot.Head].Name ), helmetMount.transform.position, helmetMount.transform.rotation ) as GameObject;
				mesh.transform.parent = helmetMount.transform;
				
				//scale
				//mesh.transform.localScale = hairMount.transform.GetChild(0).localScale;
				
				//hide player hair
				//hairMount.transform.GetChild(0).gameObject.active = false;
			}
		}
	}
	
	public void LoadCharacter() {
		//GameSetting2.LoadAttributes();
		ClearModifiers();
		//GameSetting2.LoadVitals();
		//GameSetting2.LoadSkills();
		
		//LoadHair();
		//LoadSkinColor();
		
		LoadScale();

		initialized = true;
	}
	
	public void LoadScale() {
		Vector2 scale = GameSetting2.LoadCharacterScale();
		
		transform.localScale = new Vector3(
		                                   transform.localScale.x * scale.x,
		                                   transform.localScale.y * scale.y,
		                                   transform.localScale.z * scale.x
		                                   );
	}
	
	/*public void LoadHair() {
		LoadHairMesh();
		LoadHairColor();
	}
	
	public void LoadSkinColor() {
		characterMaterialMesh.renderer.materials[ (int)CharacterMaterialIndex.Face ].mainTexture = Resources.Load( GameSetting2.HEAD_TEXTURE_PATH + "head_" + GameSetting2.LoadHeadIndex() + "_" + GameSetting2.LoadSkinColor() + ".human") as Texture;
	}

	public void LoadHairMesh() {
		if( hairMount.transform.childCount > 0 )
			Object.Destroy( hairMount.transform.GetChild(0).gameObject );

		GameObject hairStyle;
		
		int hairMeshIndex = GameSetting2.LoadHairMesh();

		int hairSet = hairMeshIndex / 5 + 1;
		int hairIndex = hairMeshIndex % 5 + 1;
		
		hairStyle = Object.Instantiate( Resources.Load(
		                                               GameSetting2.HUMAN_MALE_HAIR_MESH_PATH + "Hair" + " " + hairSet + "_" + hairIndex ),
		                               				   hairMount.transform.position,
		                                               hairMount.transform.rotation
		                                              ) as GameObject;

		hairStyle.transform.parent = PC.Instance.hairMount.transform;
		
		LoadHairColor();		

		MeshOffset mo = hairStyle.GetComponent<MeshOffset>();
		if( mo == null )
			return;
		
		hairStyle.transform.localPosition = mo.positionOffset;
		hairStyle.transform.localRotation = Quaternion.Euler( mo.rotationOffset );
		hairStyle.transform.localScale = mo.scaleOffset;
	}
	
	public void LoadHairColor() {
		Texture temp = Resources.Load( GameSetting2.HUMAN_MALE_HAIR_COLOR_PATH + ((HairColorNames)GameSetting2.LoadHairColor()).ToString()) as Texture;
		
		hairMount.transform.GetChild(0).renderer.material.mainTexture = temp;
	}*/
	
	public void LoadHelmet() {
	}

	public void LoadShoulderPads() {
	}

	public void LoadTorsoArmor() {
	}
	
	public void LoadGloves() {
	}

	public void LoadLegArmor() {
	}

	public void LoadBoots() {
	}

	public void LoadBackItem() {
	}
}
