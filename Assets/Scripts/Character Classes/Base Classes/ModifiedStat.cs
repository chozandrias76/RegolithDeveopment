/// <summary>
/// ModifiedStat.cs
/// Sept 9, 2010
/// Peter Laliberte
/// 
/// This is the base class for all stats that can be modified by attributes
/// This class is ment to be inherited from
/// 
/// This class can not be attached to anything
/// </summary>

using System.Collections.Generic;				//Generic was added so we can use the List<>

public class ModifiedStat : BaseStat {
	private List<ModifyingAttribute> _mods;		//A list of Attributes that modify this stat
	public int _modValue;						//The amount added to the baseValue from the modifiers
	
	/// <summary>
	/// Initializes a new instance of the <see cref="ModifiedStat"/> class.
	/// </summary>
	public ModifiedStat() {
//		UnityEngine.Debug.Log("Modified Created");
		_mods = new List<ModifyingAttribute>();
		_modValue = 0;
	}
	
	/// <summary>
	/// Add a ModifyingAttribute to our list of mods for this ModifiedStat
	/// </summary>
	/// <param name='mod'>
	/// Mod.
	/// </param>
	public void AddModifier(ModifyingAttribute mod) {
		_mods.Add(mod);
	}
	
	public void ClearModifiers() {
		_mods.Clear();
	}
	
	/// <summary>
	/// Reset _modValue to 0.
	/// Check to see if we have at least one ModifyingAttribute in our list of mods.
	/// If we do, then interate through the list and add the AdjustedBaseValue * ratio to our _modValue.
	/// </summary>
	private void CalculateModValue() {
//		UnityEngine.Debug.Log( "***ModifiedStat - CalculateModValue***" );
		_modValue = 0;
		
//		UnityEngine.Debug.Log("Number of Modifiers = " + _mods.Count);
		
//		UnityEngine.Debug.Log( _mods[0].attribute.AdjustedBaseValue);

		if(_mods.Count > 0) {
			foreach(ModifyingAttribute att in _mods) {
				_modValue += (int)(att.attribute.AdjustedBaseValue * att.ratio);
				
//				UnityEngine.Debug.Log("Attribute Name: " + att.attribute.Name + "\nRatio: " + att.ratio + "\nABV: " + att.attribute.AdjustedBaseValue);
//				UnityEngine.Debug.Log("Attribute Adjusted Base Value: " + att.attribute.AdjustedBaseValue);
//				UnityEngine.Debug.Log("Attribute Ratio: " + att.ratio);
			}

//			UnityEngine.Debug.Log("ModValue = " + _modValue);
		}
	}
	
	/// <summary>
	/// This function is overriding the AdjustedBaseValue in the BaseStat class.
	/// Calculate the AdjustedBaseValue from the BaseValue + BuffValue + _modValue
	/// </summary>
	/// <value>
	/// The adjusted base value.
	/// </value>
	public new int AdjustedBaseValue {
		get{ 
//			UnityEngine.Debug.Log( "B:" + BaseValue + " U:" + BuffValue + " M:" + _modValue);
			return BaseValue + BuffValue + _modValue; 
		}
	}
	
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	public void Update() {
//		UnityEngine.Debug.Log( "***ModifiedStat - Update***" );
		CalculateModValue();
	}
	
	
	//This function is for debugging out to see what attributes are used for modifying and by how much
	public string GetModifyingAttrituesString() {
		string temp = "";
		
		for(int cnt = 0; cnt < _mods.Count; cnt++) {
			temp += _mods[cnt].attribute.Name;
			temp += ": ";
			temp += _mods[cnt].attribute.AdjustedBaseValue;
			temp += " - ";
			temp += _mods[cnt].ratio;

			if(cnt < _mods.Count - 1)
				temp += "|";
		}
		
//		UnityEngine.Debug.Log( "GetModifyingAttrituesString: " + temp );
		return temp;
	}
}


/// <summary>
/// A structure that will hold an Attribute and a ratio that will be added as a modifying attribute to our ModifiedStats
/// </summary>
public struct ModifyingAttribute {
	public Attribute attribute;			//the attribute to be used as a modifier
	public float ratio;					//the percent of the attributes AdjustedBaseValue that will be applied to the ModifiedStat
	
	/// <summary>
	/// Initializes a new instance of the <see cref="ModifyingAttribute"/> struct.
	/// </summary>
	/// <param name='att'>
	/// Att. the attribute to be used
	/// </param>
	/// <param name='rat'>
	/// Rat. the ratio to use
	/// </param>
	public ModifyingAttribute(Attribute att, float rat) {
//		UnityEngine.Debug.Log("Modifying Attribute Created:" + att.Name + " -> " + rat);
		attribute = att;
		ratio = rat;
	}
}
