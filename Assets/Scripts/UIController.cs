using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour {

	public static UIController inst;

	public GameObject inventoryIconPrefab;

	public GameObject craftingRecipeVisualsPrefab;
	public GameObject craftingRecipeInputVisualPrefab;

	Text openInventoryText;
	GameObject craftingGameObject;

	Transform inventoryTransform;

	public GameObject emptySelecttionSprite;

	Transform[] itemTypeTransformByID;
	Text[] itemTypeStackTextByID;

	UIRecipe[] uiRecipes;

	public ItemType mouseHoverItemType = null;

	Text scoreText;
	Text healthText;
	GameObject deathGO;

	void Start () {

		// Make this instance of the class publicly available.
		inst = this;

	}

	void Update () {

		// If the mouse is over the ui.
		if (MouseOverUI()){

			if (mouseHoverItemType != null){

				// If the user left clicks and is hovering over an item.
				if (Input.GetMouseButtonDown (0)) {

					// If it is the users turn...
					if (GC.inst.waitingForPlayer) {
						GC.inst.map.player.EquipUnequipItem (mouseHoverItemType);
					}

				}

				// If the user right clicks.
				else if (Input.GetMouseButtonDown (1)){

					// If it is the users turn...
					if (GC.inst.waitingForPlayer){
						GC.inst.map.player.EatItem (mouseHoverItemType);
					}

				}
			
			}

		}

	}

	public void SetUp(){

		// Try placing one of every item in the ui.
		inventoryTransform = transform.GetChild (0);

		// Create the list of transforms.
		itemTypeTransformByID = new Transform[ItemType.itemTypes.Count];
		itemTypeStackTextByID = new Text[ItemType.itemTypes.Count];

		for (int i = 0; i < ItemType.itemTypes.Count; i++){
			
			GameObject newGO = Instantiate (inventoryIconPrefab);
			Image im = newGO.GetComponent <Image> ();

			im.sprite = ItemType.itemTypes[i].sprite;
			im.color = ItemType.itemTypes[i].color;

			newGO.transform.SetParent (inventoryTransform);

			itemTypeTransformByID [i] = newGO.transform;
			itemTypeStackTextByID [i] = newGO.GetComponentInChildren<Text> ();

			newGO.GetComponent<UIInventoryIcon> ().itemType = ItemType.itemTypes [i];

			newGO.SetActive (false);
		}

		// Generate all of the crafting recipes.
		uiRecipes = new UIRecipe[CraftingRecipe.craftingRecipes.Count];
		int recipeCount = 0;
		foreach (CraftingRecipe recipe in CraftingRecipe.craftingRecipes) {

			// Instantiate a crafting recipe, parenting it to the right object.
			GameObject newGO = Instantiate (craftingRecipeVisualsPrefab, transform.GetChild (2));

			// Add icons to the inputs.
			foreach (ItemStack inputStack in recipe.inputStacks) {

				// Instantiate an input icon, parenting it to the input section of the recipe's ui.
				GameObject inGO = Instantiate (craftingRecipeInputVisualPrefab, newGO.transform.GetChild (0));

				// Set the sprite and sprite colour.
				Image im = inGO.GetComponent<Image> ();
				im.sprite = inputStack.itemType.sprite;
				im.color = inputStack.itemType.color;

				// Set the text correctly.
				inGO.GetComponentInChildren<Text> ().text = inputStack.stackSize.ToString ();

			}

			// Set up the output visual.
			GameObject outGO = newGO.transform.GetChild (2).gameObject;
			Image outIm = outGO.GetComponent<Image>();
			outIm.sprite = recipe.outputStack.itemType.sprite;
			outIm.color = recipe.outputStack.itemType.color;
			outGO.GetComponentInChildren<Text> ().text = recipe.outputStack.stackSize.ToString ();

			// Apply the recipe to the game object.
			UIRecipe uir = newGO.GetComponent<UIRecipe> ();
			uir.recipe = recipe;

			// Store the game object for later, and set as inactive.
			uiRecipes [recipeCount] = uir;
			newGO.SetActive (false);
			recipeCount += 1;

		}

		// Store the open inventory button text, and game object for crafting.
		openInventoryText = transform.GetChild (1).GetComponentInChildren<Text> ();
		craftingGameObject = transform.GetChild (2).gameObject;

		// Store the score text and health text.
		scoreText = transform.GetChild (4).gameObject.GetComponentInChildren<Text> ();
		healthText= transform.GetChild (5).gameObject.GetComponentInChildren<Text> ();
		deathGO = transform.GetChild (6).gameObject;

	}

	public void UpdateVisibleRecipes(){

		// Loop through each recipe.
		foreach (UIRecipe uir in uiRecipes) {

			// Set whether the corresponding UI element is active based on whether the player has the input ingredients.
			uir.gameObject.SetActive (GC.inst.map.player.CanCraft(uir.recipe));

		}

	}

	public void NotifyInventoryChange(ItemType itemType, int newAmount){
		
		// Change the text for that item type.
		itemTypeStackTextByID [itemType.itemID].text = newAmount.ToString ();

		// If currently there is no visible icon for the item, then put the item to the far right on the inventory bar.
		Transform t = itemTypeTransformByID [itemType.itemID];
		if (t.gameObject.activeInHierarchy == false){
			t.SetAsLastSibling ();
		}

		// If the new amount is zero set the item as invisible. Otherwise as visible.
		t.gameObject.SetActive (newAmount != 0);

		// Notify ourselves to update the recipes.
		UpdateVisibleRecipes ();

	}

	public void NotifySelectedItemChange(ItemType newSelected, ItemType oldSelected){

		// If the old one was null, then set the empty selection sprite as not visible.
		if (oldSelected == null){
			emptySelecttionSprite.SetActive (false);
		}

		// Otherwise move the old selected to be at the far right.
		else {
			itemTypeTransformByID [oldSelected.itemID].SetAsLastSibling ();
		}

		// If the new one is null, then set the empty selection sprite as visible.
		if (newSelected == null){
			emptySelecttionSprite.SetActive (true);
		}

		// Otherwise move the new selected to be at the far left.
		else {
			itemTypeTransformByID [newSelected.itemID].SetAsFirstSibling ();
		}

	}

	public static bool MouseOverUI(){
		return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject ();
	}

	public void ToggleCraftingOpen(){

		// See whether it is open or not.
		bool open = craftingGameObject.activeInHierarchy;

		// Toggle active.
		craftingGameObject.SetActive (!open);

		// Change the text of the open button.
		if (open){
			openInventoryText.text = "^ CRAFTING ^";
		} else {
			openInventoryText.text = "v CRAFTING v";
		}
	}

	public void Restart(){
		SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
	}

	public void MainMenu(){
		SceneManager.LoadScene (0);
	}

	public void UpdateScoreUI(){
		scoreText.text = "Time: " + GC.inst.turnsSurvived + "\nHunt Score: " + GC.inst.huntScore.ToString("F1");
	}

	public void UpdateHealthDisplay(){
		healthText.text = GC.inst.map.player.health + " / " + GC.inst.map.player.maxHealth;
	}

	public void DisplayDeathScreen(){
		deathGO.SetActive (true);
	}

}
