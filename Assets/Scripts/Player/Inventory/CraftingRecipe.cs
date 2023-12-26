using UnityEngine;

[CreateAssetMenu(menuName = "Custom Data/Inventory/Recipe")]
public class CraftingRecipe : ScriptableObject
{
	public bool ShapelessRecipe;
	public InventoryItem[] InventoryRecipeIngredients;
	public InventoryItem[] TableRecipeIngredients;
	public InventorySlotContent RecipeResult;

	public bool isMatch(UIInventorySlot[] grid)
	{
		bool inTable = grid.Length == 9;
		bool matched = true;

		if (inTable)
		{
			if (ShapelessRecipe)
			{
				matched = ShapelessInTable(grid);
			}
			else
			{
				matched = ShapedInTable(grid);
			}
		}
		else
		{
			if (ShapelessRecipe)
			{
				matched = ShapelessInInventory(grid);
			}
			else
			{
				matched = ShapedInInventory(grid);
			}
		}

		return matched;
	}

	bool ShapedInTable(UIInventorySlot[] grid)
	{
		for (int i = 0; i < grid.Length; i++)
		{
			if (grid[i].slotContent.item != TableRecipeIngredients[i]) return false;
		}

		return true;
	}

	bool ShapelessInTable(UIInventorySlot[] grid)
	{
		bool[] ingredientFound = new bool[TableRecipeIngredients.Length];

		foreach (UIInventorySlot slot in grid)
		{
			for (int i = 0; i < TableRecipeIngredients.Length; i++)
			{
				if (!ingredientFound[i] && slot.slotContent.item == TableRecipeIngredients[i])
				{
					ingredientFound[i] = true;
					break;
				}
			}
		}

		for (int i = 0; i < ingredientFound.Length; i++)
		{
			if (!ingredientFound[i]) return false;
		}

		return true;
	}

	bool ShapedInInventory(UIInventorySlot[] grid)
	{
		Debug.Log("starting comparison with recipe");
		for (int i = 0; i < grid.Length; i++)
		{
			if (grid[i].slotContent.item != InventoryRecipeIngredients[i]) return false;
			Debug.Log("matched element at " + i);
		}

		return true;
	}

	bool ShapelessInInventory(UIInventorySlot[] grid)
	{
		bool[] ingredientFound = new bool[InventoryRecipeIngredients.Length];

		foreach (UIInventorySlot slot in grid)
		{
			for (int i = 0; i < InventoryRecipeIngredients.Length; i++)
			{
				if (!ingredientFound[i] && slot.slotContent.item == InventoryRecipeIngredients[i])
				{
					ingredientFound[i] = true;
					break;
				}
			}
		}

		for (int i = 0; i < ingredientFound.Length; i++)
		{
			if (!ingredientFound[i]) return false;
		}

		return true;
	}
}