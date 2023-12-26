using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Block", menuName = "Custom Data/Blocks/Crafting Table Behaviour")]
public class CraftingTable : BlockBehaviour
{
	public override bool Break()
	{
		if(BreakTime < 0) return false;

		if(t >= BreakTime)
		{
			t = 0;
			return true;
		}
		else
		{
			t += Time.deltaTime;
			return false;
		}
	}

	public override bool Interact()
	{
		if (!HasInteraction) return false;

		Debug.Log($"Interaction Happened {name}");
		Player.player.OpenUI(UITypes.Crafting);
		PlayerStorage.Instance.UpdateGrids();
		return true;
	}
}
