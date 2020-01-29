using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UINewSeedInterface : MonoBehaviour {

	public UIMainMenu uiMainMenu;
	public GameObject newSeedButton;
	public InputField nameField;
	public InputField seedField;

	void Start(){
		
	}

	public void Activate(){
		gameObject.SetActive (true);
		newSeedButton.SetActive (false);
	}

	public void Cancel(){
		ResetGO ();
	}

	public void Confirm(){

		// Check inputs are valid.
		if (nameField.text.Length < 1){
			return;
		}
		if (seedField.text.Length < 1){
			return;
		}

		// Retrieve values.
		string name = nameField.text;
		int seed = int.Parse (seedField.text);

		// Store in main menu ui.
		uiMainMenu.names.Add (name);
		uiMainMenu.seeds.Add (seed);

		// Save to player prefs.
		uiMainMenu.SaveToPlayerPrefs ();

		// Create the menu item.
		uiMainMenu.CreateMenuSeedItem (seed, name);

		// Reset this game object.
		ResetGO ();
	}

	void ResetGO(){
		nameField.text = "";
		seedField.text = "";
		gameObject.SetActive (false);
		newSeedButton.SetActive (true);
	}

}
