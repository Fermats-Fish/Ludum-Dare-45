using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainType {

	public static List<TerrainType> terrainTypes;

	public Dictionary<CreatureType, float> spawnWeightPerCreature = new Dictionary<CreatureType, float> ();

	const float P_ABNORMAL_TERRAIN_SPEED = 0.3f;
	const float ABNORMAL_TERRAIN_SPEED_RARITY_MOD = 0.5f;

	const int MAX_DAMAGE_PER_TURN = 2;
	const float P_DAMAGE_PER_TURN = 0.2f;
	const float RARITY_PER_DAMAGE_PER_TURN = 0.5f;


	public Sprite sprite;
	public Color color;

	public int terrainSpeed;
	public int damagePerTurn;
	public float rarity = 1f;


	public float perlinSeedX = 0f;
	public float perlinSeedY = 0f;

	float totalSpawnWeight = Mathf.NegativeInfinity;
	float totalItemInverseRarity = Mathf.NegativeInfinity;

	public List<ItemType> itemsFoundHere = new List<ItemType>();

	/// <summary>
	/// Creates a new <see cref="TerrainType"/> and adds it to the public list of terrain types.
	/// </summary>
	/// <param name="spriteName">The name of the sprite for this terrain type.</param>
	TerrainType (Sprite sprite, Color color){

		// Set the sprite for this terrain type.
		this.sprite = sprite;

		// Set the colour for this terrain type.
		this.color = color;

		// Determine the speed for this terrain type.
		if (GC.Chance(P_ABNORMAL_TERRAIN_SPEED)){

			// We want a non-default terrain speed, so have equal chance of double or half speed terrain.
			if (GC.Chance(0.5f)){
				terrainSpeed = 1;
			} else {
				terrainSpeed = 4;
			}

			// Increase this terrains rarity for having non-default terrain speed.
			rarity *= ABNORMAL_TERRAIN_SPEED_RARITY_MOD;
		} else {

			// We want default terrain speed.
			terrainSpeed = 2;

		}

		// Determine whether this terrain will deal damage to the player or not.
		if (GC.Chance(P_DAMAGE_PER_TURN)){

			// Calculate how much damage per turn.
			damagePerTurn = Random.Range (1, MAX_DAMAGE_PER_TURN + 1);

			// Adjust rarity accordingly.
			rarity *= Mathf.Pow (RARITY_PER_DAMAGE_PER_TURN, damagePerTurn);

		} else {

			// No damage per turn.
			damagePerTurn = 0;

		}

		// Add this to the list of terrain types.
		terrainTypes.Add (this);

	}

	public CreatureType GetRandomCreatureType (){

		// Calculate a number between 0 and the total spawn weight.
		float p = Random.Range (0f, TotalSpawnWeight());

		float sumSoFar = 0f;

		foreach (CreatureType creatureType in spawnWeightPerCreature.Keys) {

			float value;
			spawnWeightPerCreature.TryGetValue (creatureType, out value);

			sumSoFar += value;

			if (sumSoFar >= p){
				return creatureType;
			}

		}

		// Should never get to here but if a floating point number addition error means that sumSoFar doesn't quite reach the total spawn weight, then just return any creature.
		// Also log that this happened.
		Debug.Log ("GetRandomCreatureType sumSoFar didn't reach totalSpawnWeight. Selected first creature in creatureTypes.");
		return CreatureType.creatureTypes[0];

	}

	public ItemType GetRandomItemType (){

		// If no items spawn in this biome return null.
		if (itemsFoundHere.Count == 0){
			return null;
		}

		// If just one item, return it.
		if (itemsFoundHere.Count == 1){
			return itemsFoundHere [0];
		}

		// Otherwise pick an item at random weighted by the highest inverse rarity being.

		// Calculate a random number.
		float p = Random.Range (0f, TotalItemInverseRarity ());

		float sumSoFar = 0f;

		foreach (ItemType itemType in itemsFoundHere) {

			sumSoFar += 1f / itemType.rarity;

			if (sumSoFar >= p){
				return itemType;
			}

		}

		// Should never get to here but if a floating point number addition error means that sumSoFar doesn't quite reach the total inverse item weight, then just return any item type.
		// Also log that this happened.
		Debug.Log ("GetRandomItemType sumSoFar didn't reach totalItemInverseRarity. Selected first item type in itemsFoundHere.");
		return itemsFoundHere[0];

	}

	private float TotalSpawnWeight(){

		// If not set yet then calculate the spawn weight.
		if (totalSpawnWeight == Mathf.NegativeInfinity){
			
			totalSpawnWeight = 0f;

			foreach (float f in spawnWeightPerCreature.Values){
				totalSpawnWeight += f;
			}

		}

		return totalSpawnWeight;

	}

	private float TotalItemInverseRarity(){

		// If not set yet, then calculate the total item rarity first.
		if (totalItemInverseRarity == Mathf.Infinity){
			
			totalItemInverseRarity = 0f;

			foreach (ItemType it in itemsFoundHere){
				totalItemInverseRarity += 1f/it.rarity;
			}

		}

		return totalItemInverseRarity;

	}


	/// <summary>
	/// Generates a new <see cref="TerrainType"/> and adds it to the public list of terrain types.
	/// </summary>
	/// <param name="spriteName">The name of the sprite for this terrain type.</param>
	public static void GenerateNewTerrainType(Sprite sprite, Color color){
		new TerrainType (sprite, color);
	}

}
