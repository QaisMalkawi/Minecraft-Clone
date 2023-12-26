using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

public class UIInventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public static readonly Color emptyColor = new Color(1, 1, 1, 0);
	public static readonly Color filledColor = new Color(1, 1, 1, 1);
	public Image item;
	public TMP_Text amount;
	public InventorySlotContent slotContent;

	public bool outputOnly;
	public bool IsHovered { get; private set; }

	public Action slotClicked = ()=> { Debug.Log("Clicked"); };

	private void Awake()
	{
		GetUI();
	}
	void GetUI()
	{
		item = this.GetComponent<Image>();
		amount = this.GetComponentInChildren<TMP_Text>();
	}
	public void Refresh()
	{
		if(item == null)
		{
			GetUI();
		}
		if (slotContent.amount <= 0)
			slotContent.item = null;

		item.color = slotContent.item == null ? emptyColor : filledColor;
		amount.color = slotContent.item == null ? emptyColor : filledColor;
		item.sprite = slotContent.item?.Icon;
		amount.text = slotContent.amount.ToString();
	}
	public void Refresh(InventorySlotContent cont)
	{
		if(item == null)
		{
			GetUI();
		}

		slotContent = cont;
		Refresh();
	}
	public void OnPointerEnter(PointerEventData eventData)
	{
		PlayerStorage.Instance.SetHoveredSlot(this);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		PlayerStorage.Instance.UnSetHoveredSlot(this);
	}
	public bool HasContent()
	{
		if (slotContent.amount == 0)
			slotContent.item = null;

		if (slotContent.item == null)
			return false;

		return true;

	}
	public void SetContent(InventorySlotContent cont)
	{
		slotContent = cont;

		Refresh();
	}
}
