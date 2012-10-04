using UnityEngine;

public class Clothing : BuffItem {
//	private ArmorSlot _slot;		//store the slot the armor will be in
	private EquipmentSlot _slot;
	
	public Clothing() {
//		_slot = ArmorSlot.Head;
		_slot = EquipmentSlot.Head;
	}
	
	public Clothing(EquipmentSlot slot) {
		_slot = slot;
	}
	
	public EquipmentSlot Slot {
		get { return _slot; }
		set { _slot = value; }
	}
}

/*
public enum ArmorSlot {
	Head,
	Shoulders,
	Torso,
	Legs,
	Hands,
	Feet,
	Back,
	OffHand
}
*/