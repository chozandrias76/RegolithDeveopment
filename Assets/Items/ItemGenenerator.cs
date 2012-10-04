using UnityEngine;

public static class ItemGenerator {
	public const int BASE_MELEE_RANGE = 1;
	public const int BASE_RANGED_RANGE = 5;
	
	public static Item CreateItem() {
		//decide what type of item to make
		int rand = Random.Range( 0, (int)ItemType.COUNT );
		
		Item item = CreateItem( (ItemType)rand );

		//return the new Item
		return item;
	}

	
	public static Item CreateItem( ItemType t ) {
		Item item = new Item();
		
		//call the method to create that base item type
		switch( t ) {
		case ItemType.MeleeWeapon:
			item = CreateMeleeWeapon();
			break;
		case ItemType.MeleeETool:
			item = CreateEquipableTool();
			break;
		case ItemType.RangedWeapon:
			item = CreateMeleeWeapon();
			break;
		case ItemType.Armor:
			item = CreateArmor();
			break;
		}		

//		private string _name;

		item.Value = Random.Range(1, 101);
		
		item.Rarity = RarityTypes.Common;
		
		item.MaxDurability = Random.Range(50, 61);
		item.CurDurability = item.MaxDurability;
		
		return item;
	}
	
	
	private static ETool CreateEquipableTool() {
		ETool meleeETool = new ETool();
		
		string[] eToolNames = new string[] {
											"Shovel",
											"Pickaxe",
											"ShovelAxe",
											"Hoe",
											"Shears",
											"Hammer",
											"Rock Hammer",
											"Wood Axe",
											"Treetap",
											"Wrench",
											"Pipe Wrench",
											"Wire Cutter",
											"Painter",
											"Multimeter",
											"Construction Foam Sprayer",
											"Mining Drill",
											"Chain Saw",
											"Electric Wrench",
											"Electric Treetap",
											"Jackhammer",
											"Electric Hoe",
											"Weedwacker",
											"Smith Hammer",
											"Smith Tongs",
											"Food Tongs",
											"Chisel",
											"Fuller",
											"Scissors",
											"Pestle",
											"Dye Kit"
											};

		
		//fill in all of the values for that item type
		meleeETool.Name = eToolNames[Random.Range(0, eToolNames.Length)];
		//assign the max damage of the weapon
		for(int curTool = 0; curTool < eToolNames.Length; curTool++)
		{
			if(eToolNames[curTool] == "Shovel")
			{
				meleeETool.TypeOfSwing = SwingType.Dig;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			//if(eToolNames[curTool] == ("Rock Hammer" || "Jackhammer"))meleeETool.TypeOfSwing = SwingType.CrushStone;
			if(eToolNames[curTool] == ("Rock Hammer"))
			{
				meleeETool.TypeOfSwing = SwingType.CrushStone;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			if(eToolNames[curTool] == ("Jackhammer"))
			{
				meleeETool.TypeOfSwing = SwingType.CrushStone;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			//if(eToolNames[curTool] == ("Hoe" || "Electric Hoe"))meleeETool.TypeOfSwing = SwingType.Plow;
			if(eToolNames[curTool] == ("Hoe"))
			{
				meleeETool.TypeOfSwing = SwingType.Plow;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			if(eToolNames[curTool] == ("Electric Hoe"))
			{
				meleeETool.TypeOfSwing = SwingType.Plow;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			//if(eToolNames[curTool] == ("Hammer" || "Smith Hammer"))meleeETool.TypeOfSwing = SwingType.Hammer;
			if(eToolNames[curTool] == ("Hammer"))
			{
				meleeETool.TypeOfSwing = SwingType.Hammer;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			if(eToolNames[curTool] == ("Smith Hammer"))
			{
				meleeETool.TypeOfSwing = SwingType.Form;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			//if(eToolNames[curTool] == ("Wood Axe" || "Chain Saw"))meleeETool.TypeOfSwing = SwingType.Saw;
			if(eToolNames[curTool] == ("Wood Axe"))
			{
				meleeETool.TypeOfSwing = SwingType.Saw;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			if(eToolNames[curTool] == ("Chain Saw"))
			{
				meleeETool.TypeOfSwing = SwingType.Saw;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			//if(eToolNames[curTool] == ("Treetap" || "Electric Treetap"))meleeETool.TypeOfSwing = SwingType.Tap;
			if(eToolNames[curTool] == ("Treetap"))
			{
				meleeETool.TypeOfSwing = SwingType.Tap;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			if(eToolNames[curTool] == ("Electric Treetap"))
			{
				meleeETool.TypeOfSwing = SwingType.Tap;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			//if(eToolNames[curTool] == ("Wrench" || "Electric Wrench"))meleeETool.TypeOfSwing = SwingType.Wrench;
			if(eToolNames[curTool] == ("Wrench"))
			{
				meleeETool.TypeOfSwing = SwingType.Wrench;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
				
			}
			if(eToolNames[curTool] == ("Electric Wrench"))
			{
				meleeETool.TypeOfSwing = SwingType.Wrench;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
				
			}
			if(eToolNames[curTool] == "Pipe Wrench")
			{
				meleeETool.TypeOfSwing = SwingType.WrenchPipe;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			if(eToolNames[curTool] == "Wire Cutter")
			{
				meleeETool.TypeOfSwing = SwingType.SnipWire;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			if(eToolNames[curTool] == "Painter")
			{
				meleeETool.TypeOfSwing = SwingType.Dye;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			if(eToolNames[curTool] == "Multimeter")
			{
				meleeETool.TypeOfSwing = SwingType.ESense;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			if(eToolNames[curTool] == "Construction Foam Sprayer")
			{
				meleeETool.TypeOfSwing = SwingType.Attach;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			if(eToolNames[curTool] == "Weedwacker")
			{
				meleeETool.TypeOfSwing = SwingType.TrimPlant;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			if(eToolNames[curTool] == "Smith Tongs")
			{
				meleeETool.TypeOfSwing = SwingType.GrabDangerous;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			if(eToolNames[curTool] == "Food Tongs")
			{
				meleeETool.TypeOfSwing = SwingType.GrabDelicious;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			if(eToolNames[curTool] == "Chisel")
			{
				meleeETool.TypeOfSwing = SwingType.RemoveMaterial;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			if(eToolNames[curTool] == "Fuller")
			{
				meleeETool.TypeOfSwing = SwingType.Form;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			if(eToolNames[curTool] == "Scissors")
			{
				meleeETool.TypeOfSwing = SwingType.CutCloth;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			if(eToolNames[curTool] == "Pestle")
			{
				meleeETool.TypeOfSwing = SwingType.CrushPlant;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
			}
			//if(eToolNames[curTool] == ("Pickaxe" || "Mining Drill"))meleeETool.TypeOfSwing = SwingType.Break;
			if(eToolNames[curTool] == ("Pickaxe"))
			{
				meleeETool.TypeOfSwing = SwingType.Break;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 0;
				
			}
			if(eToolNames[curTool] == ("Mining Drill"))
			{
				meleeETool.TypeOfSwing = SwingType.Break;
				meleeETool.ProcessSpeed = meleeETool.ProcessSpeed + 10;
				
			}
			
		}
				/*
				if(meleeETool.TypeOfMaterial == "Wood") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "SandStone") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "Limestone") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "Granite") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "Obsidian") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "Emerald") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "Aquamarine") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "Ruby") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "Saphire") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "Aluminium") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "Tin") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "Copper") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "Lead") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "Silver") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "Gold") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "Iron") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "Titanium") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "Nickel") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "Tungsten") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "Uranium") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "Thorium") meleeETool.ProcessSpeed = 1;
				if(meleeETool.TypeOfMaterial == "Diamond") meleeETool.ProcessSpeed = 1;
				*/
			
		//assign the max range of this weapon
		meleeETool.MaxRange = BASE_MELEE_RANGE;
		
		//assign the icon for the weapon
		meleeETool.Icon = Resources.Load(GameSetting2.MELEE_ETOOL_ICON_PATH + meleeETool.Name) as Texture2D;
		
		//return the melee weapon
		return meleeETool;
	}
	
	private static Weapon CreateMeleeWeapon() {
		Weapon meleeWeapon = new Weapon();
		
		string[] weaponNames = new string[] {
											"Sword",
											"Morningstar",
											"Silifi",
											"Scimitar",
											"Hachet",
											"Axe",
											"Hammer",
											"Fork",
											"Pick",
											"Weak Sword",
											"Bastard Sword",
											"Torch"
											};

		
		//fill in all of the values for that item type
		meleeWeapon.Name = weaponNames[Random.Range(0, weaponNames.Length)];

		//assign the max damage of the weapon
		meleeWeapon.MaxDamage = Random.Range(5, 11);
		meleeWeapon.DamageVariance = Random.Range(.2f, .76f);
		meleeWeapon.TypeOfDamage = DamageType.Slash;
		
		//assign the max range of this weapon
		meleeWeapon.MaxRange = BASE_MELEE_RANGE;
		
		//assign the icon for the weapon
		meleeWeapon.Icon = Resources.Load(GameSetting2.MELEE_WEAPON_ICON_PATH + meleeWeapon.Name) as Texture2D;
		
		//return the melee weapon
		return meleeWeapon;
	}
	
	private static Weapon CreateRangedWeapon() {
		Weapon weap = new Weapon();

		return weap	;
	}
	
	private static Armor CreateArmor() {
		//decide what type of armor to make
		int temp = Random.Range( 0, 2 );
		
		Armor armor = new Armor();
		
		switch( temp ) {
		case 0:
			armor = CreateShield();
			break;
		case 1:
			armor = CreateHat();
			break;
		}

		//return the armor created
		return armor;
	}
	
	private static Armor CreateShield() {
		Armor armor = new Armor();
		
		string[] shieldNames = new string[] {
											"Small Shield",
											"Large Shield"
											};
		
		//fill in all of the values for that item type
		armor.Name = shieldNames[Random.Range(0, shieldNames.Length)];

		//assign properties for the shield
		armor.ArmorLevel = Random.Range(10, 50);
		
		//assign the icon for the weapon
		armor.Icon = Resources.Load(GameSetting2.SHIELD_ICON_PATH + armor.Name) as Texture2D;
		
		//assign the eqipment slot where this can be assigned
		armor.Slot = EquipmentSlot.OffHand;
		
		//return the melee weapon
		return armor;
	}
	
	private static Armor CreateHat() {
		Armor armor = new Armor();
		
		string[] hatNames = new string[] {
			"Bandana",
			"Squire Cap",
			"Heaume",
			"Pirate Hat",
			"Robin Hood",
			"Sailor"
											};
		
		//fill in all of the values for that item type
		armor.Name = hatNames[Random.Range(0, hatNames.Length)];

		//assign properties for the item
		armor.ArmorLevel = Random.Range(10, 50);
		
		//assign the icon for the weapon
		armor.Icon = Resources.Load(GameSetting2.HAT_ICON_PATH + armor.Name) as Texture2D;
		
		//assign the eqipment slot where this can be assigned
		armor.Slot = EquipmentSlot.Head;

		//return the melee weapon
		return armor;
	}
}

public enum ItemType {
	MeleeWeapon,
	MeleeETool,
	RangedWeapon,
	Armor,
//	Potion,
//	Scroll,
	COUNT
}