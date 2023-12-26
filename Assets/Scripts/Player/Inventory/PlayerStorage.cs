using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerStorage : MonoBehaviour
{
	static PlayerStorage instance;
	public static PlayerStorage Instance
	{
		get
		{
			if (instance == null)
			{
				instance = FindObjectOfType<PlayerStorage>();
			}
			return instance;
		}
	}

	[Header("UI Slots")]
	[SerializeField] Transform InventorySlotsParent;
	UIInventorySlot[] InventorySlots;

	[SerializeField] Transform InventoryCraftingGridSlotsParent;
	UIInventorySlot[] InventoryCraftingGridSlots;	

	[SerializeField] Transform InventoryHotbarSlotsParent;
	UIInventorySlot[] InventoryHotbarSlots;

	[SerializeField] Transform HotbarSlotsParent;
	UIInventorySlot[] HotbarSlots;

	[SerializeField] RectTransform[] HotbarSlotsHighlightable;
	[SerializeField] RectTransform SelectedHotbarHighlight;
	[SerializeField] UIInventorySlot hoveredSlot;
	[SerializeField] UIInventorySlot slotInhand;

	[SerializeField] int hotbarSlot;

	public UIInventorySlot[] AllInventorySlots
	{
		get
		{
			List<UIInventorySlot> allSlots = new();

			for (int i = 0; i < InventoryHotbarSlots.Length; i++)
			{
				allSlots.Add(InventoryHotbarSlots[i]);
			}
			for (int i = 0; i < InventorySlots.Length; i++)
			{
				allSlots.Add(InventorySlots[i]);
			}

			return allSlots.ToArray();
		}
	}
	
	public int selectedHotbarSlot
	{
		get
		{
			return hotbarSlot;
		}
		set
		{
			int val = value;

			if (val < 0) val = 8;
			else if (val > 8) val = 0;

			hotbarSlot = val;

			SelectedHotbarHighlight.position = HotbarSlotsHighlightable[val].position;
		}
	}
	private void Start()
	{
		Initialize();
	}
	private void Update()
	{
		slotInhand.transform.position = Input.mousePosition;
		ManageInventoryControls();
	}
	void ManageInventoryControls()
	{
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			if (hoveredSlot != null)
			{
				if(hoveredSlot.outputOnly)
				{
					if(!slotInhand.HasContent())
					{
						slotInhand.SetContent(hoveredSlot.slotContent);
						hoveredSlot.SetContent(new InventorySlotContent(null, 0));
						hoveredSlot.slotClicked.Invoke();
					
						hoveredSlot.slotClicked.Invoke();
					}
					else if(slotInhand.slotContent.item == hoveredSlot.slotContent.item)
					{
						InventorySlotContent cont = hoveredSlot.slotContent;
						cont.amount += slotInhand.slotContent.amount;
						slotInhand.SetContent(cont);
						hoveredSlot.slotClicked.Invoke();
					
						hoveredSlot.slotClicked.Invoke();
					}
				}
				else
				{
					if(hoveredSlot.slotContent.item == slotInhand.slotContent.item)
					{
						hoveredSlot.slotContent.amount += slotInhand.slotContent.amount;
						slotInhand.slotContent.amount = 0;

						hoveredSlot.Refresh();
						slotInhand.Refresh();
					
					hoveredSlot.slotClicked.Invoke();
					}
					else
					{
						InventorySlotContent temp = hoveredSlot.slotContent;
						hoveredSlot.slotContent = slotInhand.slotContent;
						slotInhand.slotContent = temp;

						slotInhand.Refresh();
						hoveredSlot.Refresh();
					
					hoveredSlot.slotClicked.Invoke();
					}
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.Mouse1))
		{
			if (hoveredSlot != null)
			{
				if(hoveredSlot.slotContent.item == slotInhand.slotContent.item)
				{
					hoveredSlot.slotContent.amount++;
					slotInhand.slotContent.amount--;
					
					hoveredSlot.Refresh();
					slotInhand.Refresh();

					hoveredSlot.slotClicked.Invoke();
				}
				else if(!hoveredSlot.HasContent())
				{
					InventorySlotContent handContent = slotInhand.slotContent;
					handContent.amount = 1;
					hoveredSlot.SetContent(handContent);

					slotInhand.slotContent.amount--;
					slotInhand.Refresh();

					hoveredSlot.slotClicked.Invoke();
				}
				else if(!slotInhand.HasContent() && hoveredSlot.HasContent())
				{
					InventorySlotContent hovered = hoveredSlot.slotContent;

					int toHand = Mathf.CeilToInt((float)(hovered.amount / 2f));
					int toBoard = Mathf.FloorToInt((float)(hovered.amount / 2f));

					hovered.amount = toHand;
					slotInhand.SetContent(hovered);

					hovered.amount = toBoard;
					hoveredSlot.SetContent(hovered);
				
					hoveredSlot.slotClicked.Invoke();
				}
			}
		}
		RefreshHotbar();
	}
	public void Initialize()
	{
		InventorySlots = new UIInventorySlot[InventorySlotsParent.childCount];
		UIInventorySlot[] slots = InventorySlotsParent.GetComponentsInChildren<UIInventorySlot>();
		for (int i = 0; i < InventorySlots.Length; i++)
		{
			InventorySlots[i] = slots[i];
		}

		InventoryCraftingGridSlots = new UIInventorySlot[InventoryCraftingGridSlotsParent.childCount];
		slots = InventoryCraftingGridSlotsParent.GetComponentsInChildren<UIInventorySlot>();
		for (int i = 0; i < InventoryCraftingGridSlots.Length; i++)
		{
			InventoryCraftingGridSlots[i] = slots[i];
		}

		InventoryHotbarSlots = new UIInventorySlot[InventoryHotbarSlotsParent.childCount];
		slots = InventoryHotbarSlotsParent.GetComponentsInChildren<UIInventorySlot>();
		for (int i = 0; i < InventoryHotbarSlots.Length; i++)
		{
			InventoryHotbarSlots[i] = slots[i];
		}

		HotbarSlots = new UIInventorySlot[HotbarSlotsParent.childCount];
		slots = HotbarSlotsParent.GetComponentsInChildren<UIInventorySlot>();
		for (int i = 0; i < HotbarSlots.Length; i++)
		{
			HotbarSlots[i] = slots[i];
		}

		UpdateGrids();
	}

	public void UpdateGrids()
	{
		for (int i = 0; i < InventorySlots.Length; i++)
		{
			InventorySlots[i].Refresh();
		}
		
		for (int i = 0; i < InventoryCraftingGridSlots.Length; i++)
		{
			InventoryCraftingGridSlots[i].Refresh();
		}		
		
		for (int i = 0; i < InventoryHotbarSlots.Length; i++)
		{
			InventoryHotbarSlots[i].Refresh();
		}
		RefreshHotbar();

		slotInhand.Refresh();
	}
	void RefreshHotbar()
	{
		for (int i = 0; i < HotbarSlots.Length; i++)
		{
			HotbarSlots[i].Refresh(InventoryHotbarSlots[i].slotContent);
		}
	}

	public void SetHoveredSlot(UIInventorySlot slot)
	{
		hoveredSlot = slot;
	}
	public void UnSetHoveredSlot(UIInventorySlot slot)
	{
		if(hoveredSlot == slot)
			hoveredSlot = null;
	}

	public short GetPlacableBlock()
	{
		short slotCont = -1;
		if(InventoryHotbarSlots[hotbarSlot]!= null && HotbarSlots[hotbarSlot].HasContent())
			slotCont = HotbarSlots[hotbarSlot].slotContent.item.targetPlaceableBlock;
		return slotCont;
	}
	public void PlacedHotbarBlock()
	{
		InventorySlotContent cont = InventoryHotbarSlots[hotbarSlot].slotContent;
		cont.amount--;
		InventoryHotbarSlots[hotbarSlot].SetContent(cont);

		RefreshHotbar();
	}

	public void GiveItem(InventorySlotContent itemToGive)
	{
		UIInventorySlot[] AllSlots = AllInventorySlots;
		for (int i = 0; i < AllSlots.Length; i++)
		{
			if (AllSlots[i].slotContent.item == itemToGive.item)
			{
				InventorySlotContent cont = AllSlots[i].slotContent;
				cont.amount += itemToGive.amount;
				AllSlots[i].SetContent(cont);
				AllSlots[i].Refresh();
				Debug.Log("Item Added To:" + AllSlots[i].name);
				return;
			}
		}
		for (int i = 0; i < AllSlots.Length; i++)
		{
			if (!AllSlots[i].HasContent())
			{
				AllSlots[i].SetContent(itemToGive);
				AllSlots[i].Refresh();
				Debug.Log("Item Added To:" + AllSlots[i].name);
				return;
			}
		}

	}
}

[System.Serializable]
public struct InventorySlotContent
{
	public InventoryItem item;
	public int amount;

	public InventorySlotContent(InventoryItem item, int amount)
	{
		this.item = item;
		this.amount = amount;
	}
}
