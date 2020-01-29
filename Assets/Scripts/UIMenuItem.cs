using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIMenuItem : MonoBehaviour {

	public UIMainMenu uiMainMenu;

	public Text nameText;
	public Text seedText;

	public int seed;

	public void PlayGame(){

		// Apply the seed.
		GC.rulesSeed = seed;

		// Load the next scene.
		SceneManager.LoadScene (1);

	}

	public void DeleteSlot(){

		// Get our index first.
		int index = transform.GetSiblingIndex ();

		// Remove from the main menu.
		uiMainMenu.seeds.RemoveAt (index);
		uiMainMenu.names.RemoveAt (index);

		// Save changes.
		uiMainMenu.SaveToPlayerPrefs();

		// Delete this game object.
		Destroy(gameObject);

	}

}
