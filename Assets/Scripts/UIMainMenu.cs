using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMainMenu : MonoBehaviour {

	public GameObject menuItemPrefab;

	public List<string> names;
	public List<int> seeds;

	void Start () {

		// Load all seed name combinations which are saved from player prefs.
		string namesString = PlayerPrefs.GetString ("names", "Default Seed\n");
		string seedsString = PlayerPrefs.GetString ("seeds", "0\n");

		// Turn the names into a list of names, and the seeds into a list of seeds.
		names = StringToStringList (namesString);
		seeds = StringListToIntList (StringToStringList (seedsString));

		// Create a menu seed item for everything in the list.
		for (int i = 0; i < seeds.Count; i++) {
			CreateMenuSeedItem (seeds [i], names [i]);
		}

	}

	public void SaveToPlayerPrefs (){

		string namesString = StringListToString (names);
		string seedsString = StringListToString (IntListToStringList (seeds));

		PlayerPrefs.SetString ("names", namesString);
		PlayerPrefs.SetString ("seeds", seedsString);

	}

	public void CreateMenuSeedItem (int seed, string name){

		// Instantiate a menu item.
		GameObject newGO = Instantiate (menuItemPrefab, transform);
		newGO.transform.SetSiblingIndex (transform.childCount-3);

		UIMenuItem seedUI = newGO.GetComponent<UIMenuItem> ();
		seedUI.nameText.text = name;
		seedUI.seedText.text = "Seed: " + seed.ToString ();
		seedUI.seed = seed;

		seedUI.uiMainMenu = this;

	}

	public static string StringListToString( List<string> l ){

		string retStr = "";

		foreach (string s in l) {

			retStr += s + "\n";

		}

		return retStr;

	}

	public static List<string> StringToStringList( string s ){

		List<string> retList = new List<string> ();

		// Get the first seperation character.
		while (s.Length >= 1) {
			int i = s.IndexOf ('\n');
			retList.Add (s.Substring (0, i));
			s = s.Substring (i + 1);
		}

		return retList;

	}

	public static List<int> StringListToIntList( List<string> l ){

		List<int> retList = new List<int> ();
		foreach (string s in l) {
			retList.Add (int.Parse (s));
		}
		return retList;

	}

	public static List<string> IntListToStringList( List<int> l ){

		List<string> retList = new List<string> ();
		foreach (int i in l) {
			retList.Add (i.ToString());
		}
		return retList;

	}

}
