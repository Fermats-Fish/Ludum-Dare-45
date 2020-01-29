using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory {

	int selectedItemTypeID = -1;

	int[] itemCounts;

	public Inventory(int nItems){
		itemCounts = new int[nItems];
	}

	public ItemType GetSelectedItemType(){
		if (selectedItemTypeID >= 0){
			return ItemType.itemTypes [selectedItemTypeID];
		} else {
			return null;
		}
	}

	public void SetSelectedItemType(ItemType toSelect){

		if (toSelect == null || GetAmountOf(toSelect) == 0){
			UIController.inst.NotifySelectedItemChange (null, GetSelectedItemType());
			selectedItemTypeID = -1;
		} else {
			UIController.inst.NotifySelectedItemChange (toSelect, GetSelectedItemType());
			selectedItemTypeID = toSelect.itemID;
		}

	}

	public int GetAmountOf(ItemType itemType){
		return itemCounts [itemType.itemID];
	}

	public void AddStack(ItemStack itemStack){
		itemCounts [itemStack.itemType.itemID] += itemStack.stackSize;
		NotifyInvChange (itemStack);
	}

	public void RemoveStack(ItemStack itemStack){
		if (GetAmountOf(itemStack.itemType) < itemStack.stackSize){
			Debug.LogError ("Trying to remove more of an item than we have.");
		}
		itemCounts [itemStack.itemType.itemID] -= itemStack.stackSize;
		if (itemCounts [itemStack.itemType.itemID] == 0 && itemStack.itemType == GetSelectedItemType()){
			SetSelectedItemType (null);
		}
		NotifyInvChange (itemStack);
	}

	protected void NotifyInvChange(ItemStack itemStack){
		UIController.inst.NotifyInventoryChange (itemStack.itemType, GetAmountOf(itemStack.itemType));
	}

}
