/// <summary>
/// ETool.cs
/// 8.25.2012
/// Colin Swenson-Healey
/// 
/// Our Equippable Tool class!
/// 
/// This script does not get attached to anything
/// </summary>
using UnityEngine;

[AddComponentMenu("VoxCraft/Item/ETool")]
public class ETool : BuffItem {
	private float _processSpd;
	private SwingType _swingType;
	private MaterialType _materialType;
	private float _maxRange;
	
	public ETool() {
		_processSpd = 0.0f;
		_swingType = SwingType.Dig;
		_materialType = MaterialType.Wood;
		_maxRange = 0;
	}
	
	public ETool(int pSpd, MaterialType mt, float mRange, SwingType st) {
		_processSpd = pSpd;
		_maxRange = mRange;
		_swingType = st;
		_materialType = mt;
	}
	
	
	public float MaxRange {
		get { return _maxRange; }
		set { _maxRange = value; }
	}
	
	public SwingType TypeOfSwing {
		get { return _swingType; }
		set { _swingType = value; }
	}
	public MaterialType TypeOfMaterial{
		get { return _materialType; }
		set { _materialType = value; }
	}
	public float ProcessSpeed {
		get { return _processSpd; }
		set { _processSpd = value; }
	}
	
	public override string ToolTip() {
		return Name + "\n" +
				"Value " + Value + "\n" +
				"Durability " + CurDurability + "/" + MaxDurability + "\n" +
				"Material " + Mat + "\n" +
				"Process Speed " + ProcessSpeed;
	}
}

public enum SwingType {
	Dig,
	Break,
	CrushStone,
	CrushPlant,
	Plow,
	Ignite,
	Wrench,
	WrenchPipe,
	SnipWire,
	TrimPlant,
	TrimAnimal,
	Hammer,
	Tap,
	Paint,
	ESense,
	Saw,
	Attach,
	Switch,
	Form,
	CutCloth,
	RemoveMaterial,
	GrabDangerous,
	GrabDelicious,
	Dye,
	
}

public enum MaterialType {
	Wood,
	SandStone,
	Limestone,
	Granite,
	Obsidian,
	Emerald,
	Aquamarine,
	Ruby,
	Saphire,
	Aluminium,
	Tin,
	Copper,
	Lead,
	Silver,
	Gold,
	Iron,
	Titanium,
	Nickel,
	Tungsten,
	Uranium,
	Thorium,
	Diamond
}
