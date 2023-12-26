using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Inventory Item", menuName ="Custom Data/Inventory/Inventory Item")]
public class InventoryItem : ScriptableObject
{
	public string Description;
	public bool stackable;
	public int stackSize;
	public Sprite Icon;
	public short targetPlaceableBlock;
}
