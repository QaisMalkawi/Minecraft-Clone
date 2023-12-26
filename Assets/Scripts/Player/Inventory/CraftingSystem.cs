using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
	[SerializeField] UIInventorySlot[] InventoryCraftingGridSlots;
	[SerializeField] UIInventorySlot InventoryCraftingGridSlotsResult;

	[SerializeField] UIInventorySlot[] TableCraftingGridSlots;
	[SerializeField] UIInventorySlot TableCraftingGridSlotsResult;

	[SerializeField] CraftingRecipe[] CraftingRecipes;

	private void Start()
	{
		Initialize();
	}
	public void Initialize()
	{
		UpdateGrids();

		for (int i = 0; i < InventoryCraftingGridSlots.Length; i++)
		{
			InventoryCraftingGridSlots[i].slotClicked += () => {

				if (!InventoryCraftingGridSlotsResult.HasContent())
				{
					for (int r = 0; r < CraftingRecipes.Length; r++)
					{
						if (CraftingRecipes[r].isMatch(InventoryCraftingGridSlots))
						{
							InventoryCraftingGridSlotsResult.SetContent(CraftingRecipes[r].RecipeResult);
							break;
						}
					}
				}
			};
		}
		InventoryCraftingGridSlotsResult.slotClicked += () =>
		{
			for (int i = 0; i < InventoryCraftingGridSlots.Length; i++)
			{
				InventorySlotContent cont = InventoryCraftingGridSlots[i].slotContent;
				cont.amount--;
				InventoryCraftingGridSlots[i].SetContent(cont);
			}
		};


		for (int i = 0; i < TableCraftingGridSlots.Length; i++)
		{
			TableCraftingGridSlots[i].slotClicked += () => {
				if (!TableCraftingGridSlotsResult.HasContent())
				{
					for (int r = 0; r < CraftingRecipes.Length; r++)
					{
						if (CraftingRecipes[r].isMatch(TableCraftingGridSlots))
						{
							TableCraftingGridSlotsResult.SetContent(CraftingRecipes[r].RecipeResult);
							break;
						}
					}
				}
			};
		}
		TableCraftingGridSlotsResult.slotClicked += () =>
		{
			for (int i = 0; i < TableCraftingGridSlots.Length; i++)
			{
				InventorySlotContent cont = TableCraftingGridSlots[i].slotContent;
				cont.amount--;
				TableCraftingGridSlots[i].SetContent(cont);
			}
		};
	}
	public void UpdateGrids()
	{
		for (int i = 0; i < InventoryCraftingGridSlots.Length; i++)
		{
			InventoryCraftingGridSlots[i].Refresh();
		}
		for (int i = 0; i < TableCraftingGridSlots.Length; i++)
		{
			TableCraftingGridSlots[i].Refresh();
		}
	}
}