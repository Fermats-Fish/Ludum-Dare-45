using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStack {
	
	public ItemType itemType;
	public int stackSize;

	public ItemStack(ItemType itemType, int stackSize){
		this.itemType = itemType;
		this.stackSize = stackSize;
	}

}
