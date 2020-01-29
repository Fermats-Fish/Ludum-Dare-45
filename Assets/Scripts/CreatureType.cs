using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureType {

	// Each creature has an AI type for every other creature which says how it interacts with that creature.
	// Hostile means it always runs up to the creature and attacks once in range.
	// SemiHostile means it will act hostile to the creature if the creature attacks it.
	// SemiEvasive means it will evade the creature if attacked by it.
	// Evasive means it will always evade the creature.
	public enum Stance {Hostile, SemiHostile, SemiEvasive, Evasive}

	public static List<CreatureType> creatureTypes;

	const float P_ABNORMAL_SPEED = 0.2f;
	const float P_FAST_GIVEN_ABONORMAL_SPEED = 0.2f;
	const float DIFF_MOD_IF_SLOW = 0.25f;
	const float DIFF_MOD_IF_FAST = 4f;

	public const int MAX_ATTACK_RANGE = 12;
	const int MIN_ATTACK_RANGE = 2;

	const int MIN_GAP_BETWEEN_SIGHT_AND_ATTACK = 5;
	public const int MAX_SIGHT_RANGE = 20; // Must be strictly more than MAX_ATTACK_RANGE + MIN_GAP_BETWEEN_SIGHT_AND_ATTACK.

	const int MIN_ATTACK_DAMAGE = 3;
	const int MAX_ATTACK_DAMAGE = 8;

	const int MIN_HEALTH = 5;
	const int MAX_HEALTH = 60;

	const int INIT_ATTACK_COOLDOWN_MAX = 2;
	const int MIN_ATTACK_COOLDOWN_ADDITION = 2;
	const int MAX_ATTACK_COOLDOWN_ADDITION = 6;

	const float BASE_PROB_HOSTILE_OR_SEMI_PLAYER = 0.5f;
	const float BASE_PROB_SEMI_IF_HOSTILE_PLAYER = 0.2f;
	const float BASE_PROB_SEMI_IF_EVASIVE_PLAYER = 0.8f;

	// A random number between plus or minus this will be added to the relative difficulty of the two creatures when calculating whether the creature is hostile.
	const float RANDOMNESS_FOR_HOSTILE_OR_EVASIVE = 1f;
	const float BASE_PROB_SEMI = 0.8f;


	public Sprite sprite;
	public Color color;

	public int moveSpeed;
	public int attackRange;
	public int sightRange;
	public int attackDamage;
	public int attackCooldown; // Number of turns for which the attack will be unoperable.
	public int health;

	// Whether this creature is hostile, semihostile, etc to the player.
	public Stance playerStance;

	// For each other creature, this creatures stance on that creature.
	public Dictionary<CreatureType, Stance> creatureStances;

	// An estimated difficulty for this creature. The game will try to keep this around 1, either by making the creature rare, or through other means.
	public float diff = 1f;

	// The item type that this creature drops.
	public ItemType itemTypeDropped;

	/// <summary>
	/// Generates a new <see cref="CreatureType"/> and adds it to the public list of creature types.
	/// </summary>
	/// <param name="spriteName">The name of the sprite for this terrain type.</param>
	CreatureType (Sprite sprite, Color color){

		// Set the sprite and colour for this creature type.
		this.sprite = sprite;
		this.color = color;

		// Decide on the move speed of the creature.
		if (GC.Chance(P_ABNORMAL_SPEED)){
			if (GC.Chance(P_FAST_GIVEN_ABONORMAL_SPEED)){
				moveSpeed = 4;
				diff *= DIFF_MOD_IF_FAST;
			} else {
				moveSpeed = 1;
				diff *= DIFF_MOD_IF_SLOW;
			}
		} else {
			moveSpeed = 2;
		}

		// Decide on the attack range of the creature. Decrease the maximum if difficulty is high, then readjust difficulty.
		attackRange = Random.Range (MIN_ATTACK_RANGE, Mathf.Min (1 + MAX_ATTACK_RANGE, 1 + (int)(MAX_ATTACK_RANGE / diff)));
		diff *= GC.ReMap (attackRange, MIN_ATTACK_RANGE, MAX_ATTACK_RANGE, 0.5f, 1.5f);

		// Decide on the sight range of the creature. Should be at least a little bit more than the attack range.
		sightRange = Random.Range (attackRange + MIN_GAP_BETWEEN_SIGHT_AND_ATTACK, Mathf.Min(1 + MAX_SIGHT_RANGE, 1 + (int)(MAX_SIGHT_RANGE / diff)));
		diff *= GC.ReMap (sightRange, attackRange + MIN_GAP_BETWEEN_SIGHT_AND_ATTACK, MAX_SIGHT_RANGE, 0.9f, 1.1f);

		// Calculate attack damage.
		attackDamage = Random.Range (Mathf.Max (MIN_ATTACK_DAMAGE, (int)(MIN_ATTACK_DAMAGE / diff)), Mathf.Min(1 + MAX_ATTACK_DAMAGE, 1 + (int)(MAX_ATTACK_DAMAGE / diff)));
		diff *= GC.ReMap (attackDamage, MIN_ATTACK_DAMAGE, MAX_ATTACK_DAMAGE, 0.5f, 2f);

		// Calculate the attack cooldown.
		attackCooldown = Random.Range (0, 1+INIT_ATTACK_COOLDOWN_MAX);
		attackCooldown += (int)GC.ReMap ( diff, 0.5f, 2f, MIN_ATTACK_COOLDOWN_ADDITION, MAX_ATTACK_COOLDOWN_ADDITION );
		attackCooldown = Mathf.Clamp (attackCooldown, MIN_ATTACK_COOLDOWN_ADDITION, MAX_ATTACK_COOLDOWN_ADDITION + INIT_ATTACK_COOLDOWN_MAX);
		diff *= GC.ReMap (attackCooldown, MIN_ATTACK_COOLDOWN_ADDITION, MAX_ATTACK_COOLDOWN_ADDITION + INIT_ATTACK_COOLDOWN_MAX, 2f, 0.5f);

		// Calculate health.
		float fhealth = Random.Range (0f, 1f);
		fhealth = fhealth * fhealth;
		fhealth /= diff;
		health = (int) GC.ReMap (fhealth, 0f, 2f, MIN_HEALTH, 1+MAX_HEALTH);
		health = Mathf.Clamp (health, MIN_HEALTH, MAX_HEALTH);
		diff *= GC.ReMap (health, MIN_HEALTH, MAX_HEALTH, 0.5f, 3f);

        /*
		Debug.Log ("Seed: " + moveSpeed.ToString() + ", Range: " + attackRange.ToString() + ", Sight: " + sightRange.ToString()
			+ ", Damage: " + attackDamage.ToString() + ", Cooldown: " + attackCooldown.ToString() + ", Health: " + health.ToString()
			+ ", Diff: " +  diff.ToString());
        */

		// Decide how this creature interacts with the player. More likely to be hostile if strong.
		if (GC.Chance( BASE_PROB_HOSTILE_OR_SEMI_PLAYER * GC.ReMap(diff, 0.5f, 2f, 0.75f, 1.5f) )){

			// HOSTILE

			// Decide if semi hostile. More likely to be semi hostile if strong or weak.
			if (GC.Chance( BASE_PROB_SEMI_IF_HOSTILE_PLAYER * GC.ReMap( Mathf.Abs(diff - 1.25f), 0f, 0.75f, 1.5f, 0.5f ) ) ){

				// SEMI HOSTILE.
				playerStance = Stance.SemiHostile;

			} else {

				// HOSTILE.
				playerStance = Stance.Hostile;

			}

		} else {

			// EVASIVE

			// Decide if semi evasive or evasive. More likely to be semi evasive if strong.
			if (GC.Chance (BASE_PROB_SEMI_IF_EVASIVE_PLAYER * GC.ReMap (diff, 0.5f, 2f, 1.1f, 0.9f))){

				// SEMI EVASIVE
				playerStance = Stance.SemiEvasive;

			} else {

				// SEMI HOSTILE
				playerStance = Stance.SemiHostile;

			}

		}

		// How this creature interacts with each other creature will be calculated after all creatures have been calculated.

		// We shall now calculate this creatures base spawn weight.
		float baseSpawnProb = GC.ReMap(diff, 0.5f, 2f, 1f, 0f);
		baseSpawnProb = Mathf.Clamp (baseSpawnProb, 0f, 1f);

		// We shall now calculate its spawn weight for each biome.
		foreach (TerrainType terrainType in TerrainType.terrainTypes) {
			terrainType.spawnWeightPerCreature.Add (this, baseSpawnProb * Mathf.Pow(Random.Range(0f, 1f), 3));
		}

		// Add this creature to the list of creature types.
		creatureTypes.Add (this);

	}


	public void GenerateStancesWithOtherCreatures(){

		if (creatureStances != null){
			Debug.LogError ("Creature Stances Already Generated!");
			return;
		}

		creatureStances = new Dictionary<CreatureType, Stance> ();

		// Loop through each other creature.
		foreach( CreatureType creatureType in creatureTypes ){

			Stance stance;

			// Calculate the relative difficulty of this creature to ours. Rough max of about 2.5 means it is way stronger than ours. Rough min of about 2.5 means it is way weaker.
			// Note these "maximums" will be exceeded by maybe 1 or 2 creatures each game, and were calculated by trial and error, nothing rigorous.
			float relDiff = creatureType.diff - diff;

			// If relDiff is largeish then we will be at least semi-hostile to it.
			// If relDiff is smallish then we will be at least semi-neutral to it.
			// If relDiff is large in magnitude then we will be less likely to be semi.

			if ( Random.Range(-RANDOMNESS_FOR_HOSTILE_OR_EVASIVE,RANDOMNESS_FOR_HOSTILE_OR_EVASIVE) + relDiff > 0 ){

				// HOSTILE

				if ( GC.Chance(BASE_PROB_SEMI * GC.ReMap( Mathf.Abs(relDiff), 0f, 2.5f, 1.2f, 0.8f ) ) ){

					// SEMI HOSTILE
					stance = Stance.SemiHostile;

				} else {

					// HOSTILE
					stance = Stance.Hostile;

				}

			} else {

				// EVASIVE
				if ( GC.Chance(BASE_PROB_SEMI * GC.ReMap( Mathf.Abs(relDiff), 0f, 2.5f, 1.2f, 0.8f ) ) ){

					// SEMI EVASIVE
					stance = Stance.SemiEvasive;

				} else {

					// EVASIVE
					stance = Stance.Evasive;

				}

			}

			creatureStances.Add (creatureType, stance);

		}

	}


	/// <summary>
	/// Generates a new <see cref="CreatureType"/> and adds it to the public list of creature types.
	/// </summary>
	/// <param name="spriteName">The name of the sprite for this terrain type.</param>
	public static void GenerateNewCreatureType(Sprite sprite, Color color){
		new CreatureType (sprite, color);
	}

}
