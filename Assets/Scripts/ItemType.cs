using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemType {

	public static List<ItemType> itemTypes;

	const int BASIC_MAX_HEALTH_EFFECT = 20;
	const float BASIC_HEALTH_EFFECT_RARITY_MOD = 1.1f;

	const int BASIC_MAX_MELEE_DAMAGE = 5;
	const float BASIC_MELEE_DAMAGE_RARITY_MOD = 1.2f;

	const int BASIC_MAX_MELEE_RANGE = 4;
	const float BASIC_MELEE_RANGE_RARITY_MOD = 1.2f;

	const int BASIC_MAX_THROW_DAMAGE = 5;
	const float BASIC_THROW_DAMAGE_RARITY_MOD = 1.4f;

	const int BASIC_MIN_ADDITIONAL_RANGED_RANGE = 3;
	const int BASIC_MAX_THROW_RANGE = CreatureType.MAX_ATTACK_RANGE;
	const float BASIC_THROW_RANGE_RARITY_MOD = 1.05f;


	public Sprite sprite;
	public Color color;

	public int healthEffect;

	public int meleeRange;
	public int meleeDamage;

	public int throwRange;
	public int throwDamage;

	public ItemType throwUsesOtherItemType;

	//// TODO: Custom Attack Cooldown

	public float rarity = 1f;

	public int itemID;


	ItemType(){

		// Store item index in list.
		itemID = itemTypes.Count;

		// Add to the list of item types.
		itemTypes.Add (this);

	}

	public static ItemType GenerateNewBasicItemType(){

		ItemType it = new ItemType ();

		// Determine the health effect.
		it.healthEffect = (int)(BASIC_MAX_HEALTH_EFFECT * (Mathf.Pow (Random.Range (0f, 1f), 3) - 0.2f));
		it.rarity *= Mathf.Pow (BASIC_HEALTH_EFFECT_RARITY_MOD, it.healthEffect);

		// Determine the melee damage.
		it.meleeDamage = (int)(BASIC_MAX_MELEE_DAMAGE * Random.Range (0f, 1f) * GC.ReMap (it.rarity, 0.8f, 2.4f, 1f, -0.3f));
		if (it.meleeDamage < 1){
			it.meleeDamage = 1;
		}
		it.rarity *= Mathf.Pow (BASIC_MELEE_DAMAGE_RARITY_MOD, it.meleeDamage);

		// Determine the melee range.
		it.meleeRange = (int)(BASIC_MAX_MELEE_RANGE * Random.Range (0f, 1f) * GC.ReMap (it.rarity, 0.8f, 2.4f, 1f, 0.1f));
		if (it.meleeRange < 1){
			it.meleeRange = 1;
		}
		it.rarity *= Mathf.Pow (BASIC_MELEE_RANGE_RARITY_MOD, it.meleeDamage-2);

		// Determine the throw damage.
		it.throwDamage = (int)( BASIC_MAX_THROW_DAMAGE * Random.Range(0f, 1f) * GC.ReMap (it.rarity, 0.75f, 2.7f, 1f, 0.1f));
		if (it.throwDamage < 1){
			it.throwDamage = 1;
		}
		it.rarity *= Mathf.Pow (BASIC_THROW_DAMAGE_RARITY_MOD, it.throwDamage - 2);

		// Determine the throw range.
		it.throwRange = (int)(GC.ReMap (Random.Range (0f, 1f) * GC.ReMap(it.rarity, 0.75f, 2.7f, 1f, 0.5f) , 0f, 1f, it.meleeRange + BASIC_MIN_ADDITIONAL_RANGED_RANGE, BASIC_MAX_THROW_RANGE));
		it.throwRange = Mathf.Clamp (it.throwRange, it.meleeRange + BASIC_MIN_ADDITIONAL_RANGED_RANGE, BASIC_MAX_THROW_RANGE);
		it.rarity *= Mathf.Pow (BASIC_THROW_RANGE_RARITY_MOD, it.throwRange - (BASIC_MAX_THROW_RANGE - it.meleeRange - BASIC_MIN_ADDITIONAL_RANGED_RANGE) / 2);

		// Rarity will be mostly above 1. Occasionally as low as 0.75. Mostly below 2.1. Occasionally as high as 2.5.

		// Return the item type.
		return it;

	}

	const int CRAFTED_MAX_HEALTH_EFFECT = 50;
	const float CRAFTED_HEALTH_EFFECT_RARITY_MOD = 1.01f;

	const int CRAFTED_MAX_MELEE_DAMAGE = 50;
	const float CRAFTED_MELEE_DAMAGE_RARITY_MOD = 1.0001f;

	const int CRAFTED_MAX_MELEE_RANGE = 5;
	const float CRAFTED_MELEE_RANGE_RARITY_MOD = 1.1f;

	const int CRAFTED_MAX_THROW_DAMAGE = 30;
	const float CRAFTED_THROW_DAMAGE_RARITY_MOD = 1.035f;

	const int CRAFTED_MAX_THROW_RANGE = CreatureType.MAX_ATTACK_RANGE + 3;
	const float CRAFTED_THROW_RANGE_RARITY_MOD = 1.05f;
	const int CRAFTED_MIN_ADDITIONAL_RANGED_RANGE = 3;

	const float BASE_PROB_ALTERNATE_AMMO_FOR_THROWS = 0.75f;


	public static ItemType GenerateNewCraftedItemType(Sprite sprite, Color color){

		// Create the empty item type object.
		ItemType it = new ItemType ();
		it.sprite = sprite;
		it.color = color;

		// Determine the health effect.
		it.healthEffect = (int)(CRAFTED_MAX_HEALTH_EFFECT * (Mathf.Pow (Random.Range (0f, 1f), 3) - 0.2f));
		it.rarity *= Mathf.Pow (CRAFTED_HEALTH_EFFECT_RARITY_MOD, it.healthEffect);

		// Determine the melee damage.
		it.meleeDamage = (int)(CRAFTED_MAX_MELEE_DAMAGE * Random.Range (0f, 1f) * GC.ReMap (it.rarity, 0.8f, 10f, 1f, -0.3f));
		if (it.meleeDamage < 1){
			it.meleeDamage = 1;
		}
		it.rarity *= Mathf.Pow (CRAFTED_MELEE_DAMAGE_RARITY_MOD, it.meleeDamage);

		// Determine the melee range.
		it.meleeRange = (int)(CRAFTED_MAX_MELEE_RANGE * Random.Range (0f, 1f) * GC.ReMap (it.rarity, 0.8f, 10f, 1f, 0.1f));
		if (it.meleeRange < 1){
			it.meleeRange = 1;
		}
		it.rarity *= Mathf.Pow (CRAFTED_MELEE_RANGE_RARITY_MOD, it.meleeDamage-2);

		// Determine the throw damage.
		it.throwDamage = (int)( CRAFTED_MAX_THROW_DAMAGE * Random.Range(0f, 1f) * GC.ReMap (it.rarity, 0.75f, 20f, 1f, 0.1f));
		if (it.throwDamage < 1){
			it.throwDamage = 1;
		}
		it.rarity *= Mathf.Pow (CRAFTED_THROW_DAMAGE_RARITY_MOD, it.throwDamage - 2);

		// Determine the throw range.
		it.throwRange = (int)(GC.ReMap (Random.Range (0f, 1f) * GC.ReMap(it.rarity, 0.75f, 30f, 1f, 0.5f) , 0f, 1f, it.meleeRange + CRAFTED_MIN_ADDITIONAL_RANGED_RANGE, CRAFTED_MAX_THROW_RANGE));
		it.throwRange = Mathf.Clamp (it.throwRange, it.meleeRange + CRAFTED_MIN_ADDITIONAL_RANGED_RANGE, CRAFTED_MAX_THROW_RANGE);
		it.rarity *= Mathf.Pow (CRAFTED_THROW_RANGE_RARITY_MOD, it.throwRange - (CRAFTED_MAX_THROW_RANGE - it.meleeRange - CRAFTED_MIN_ADDITIONAL_RANGED_RANGE) / 2);


		// Determine whether "throwing" uses an alternate ammo and hence is more like firing.
		if (GC.Chance(BASE_PROB_ALTERNATE_AMMO_FOR_THROWS * GC.ReMap(it.throwRange, it.meleeRange + CRAFTED_MIN_ADDITIONAL_RANGED_RANGE, CRAFTED_MAX_THROW_RANGE, 0.5f, 1.5f))){

			//TODO How to decide what to use as ammo? Should be cheap, and even cheaper if this item has a high rarity.

		}

		it.rarity /= 3;
		//Debug.Log ( "Health Effect: " + it.healthEffect + ", Melee Damage: " + it.meleeDamage + ", Melee Range: " + it.meleeRange + ", Throw Damage: " + it.throwDamage + ", Throw Range: " + it.throwRange + ", Rarity: " + it.rarity);

		return it;

	}

}
