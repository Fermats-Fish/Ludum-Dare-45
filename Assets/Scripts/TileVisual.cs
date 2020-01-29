using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileVisual  {

	const float TILE_VISUAL_SCALE = 0.625f;

	public Tile t;

	GameObject terrainGO;
	public GameObject entityGO;
	public GameObject itemGO;

	public TileVisual (Tile t){

		// Store the tile.
		this.t = t;

		// Instantiate a game object, parented to the game controller, to represent the terrain. Also place the object in the correct place.
		terrainGO = MonoBehaviour.Instantiate (GC.inst.terrainPrefab, new Vector3(t.x, t.y, 10), Quaternion.identity, GC.inst.transform);
		terrainGO.transform.name = t.x.ToString() + ", " + t.y.ToString();

		// Set the object's sprite and colour.
		SpriteRenderer sr = terrainGO.GetComponent<SpriteRenderer> ();
		sr.sprite = t.terrainType.sprite;
		sr.color = t.terrainType.color;

		// Create an entity game object if there is an entity on this tile.
		if (t.entity != null && !(t.entity.GetSprite() == null)) {
			entityGO = CreateEntityVisual (t);
			if (t.entity is Player){
				Camera.main.transform.SetParent(entityGO.transform);
			}
		}

		// Create an item game object if there is an item on this tile.
		if (t.storedItem != null){
			itemGO = CreateItemVisual (t);
		}

	}

	public void Destroy(){
		GameObject.Destroy (terrainGO);
		if (entityGO != null){
			GameObject.Destroy (entityGO);
		}
		if (itemGO != null){
			GameObject.Destroy (itemGO);
		}
	}

	public static GameObject CreateEntityVisual(Tile t){
		GameObject go = MonoBehaviour.Instantiate (GC.inst.entityPrefab, new Vector3(t.x, t.y, 0), Quaternion.identity, GC.inst.transform);

		// Set the object's sprite and colour.
		SpriteRenderer sr = go.GetComponent<SpriteRenderer> ();
		sr.sprite = t.entity.GetSprite();
		sr.color = t.entity.GetSpriteColor();

		go.name = "Creature";

		return go;
	}

	public static GameObject CreateItemVisual(Tile t){

		// Calculate how offset from the tile centre that the visual should be.
		float offset = (1f - TILE_VISUAL_SCALE) / 2f;

		// Instantiate the game object.
		GameObject go = MonoBehaviour.Instantiate (GC.inst.terrainPrefab, new Vector3 (t.x+offset, t.y+offset, 5), Quaternion.identity, GC.inst.transform);

		// Scale the game object a bit.
		go.transform.localScale = new Vector3 (TILE_VISUAL_SCALE, TILE_VISUAL_SCALE, 1f);

		// Set the object's sprite and colour.
		SpriteRenderer sr = go.GetComponent<SpriteRenderer> ();
		sr.sprite = t.storedItem.sprite;
		sr.color = t.storedItem.color;

		go.name = "Item";

		return go;
	}

}
