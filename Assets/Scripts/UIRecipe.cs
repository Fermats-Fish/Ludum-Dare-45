using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIRecipe : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

	Image image;
	public CraftingRecipe recipe;

	void Start(){
		image = GetComponent<Image> ();
	}

	public void OnPointerEnter(PointerEventData eventData){
		image.color = new Color (1f, 1f, 1f, 0.1f);
	}

	public void OnPointerExit(PointerEventData eventData){
		image.color = new Color (1f, 1f, 1f, 0f);
	}

	public void OnPointerClick(PointerEventData eventData){

		// If it is the players turn, tell them to try to craft the recipe.
		if (GC.inst.waitingForPlayer){

			GC.inst.map.player.TryCraftRecipe (recipe);

		}

	}
}
