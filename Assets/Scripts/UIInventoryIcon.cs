using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIInventoryIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	public ItemType itemType;

	public void OnPointerEnter(PointerEventData eventData){
		UIController.inst.mouseHoverItemType = itemType;
	}

	public void OnPointerExit(PointerEventData eventData){
		UIController.inst.mouseHoverItemType = null;
	}

}
