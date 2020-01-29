using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MoveableEntity {

	const float P_NO_TARGET_IDLE = 0.5f;

	public MoveableEntity target;

	public CreatureType creatureType;

	public Creature(CreatureType creatureType, Tile t) : base(t){
		this.creatureType = creatureType;
		health = maxHealth = creatureType.health;
	}


	public override void SelectNewAction(){

		// Firstly, do we have a target?
		if (target != null){

			// Can we still see the target?
			if (target.dead || SquareDistance (target) >= creatureType.sightRange * creatureType.sightRange){

				// Not any more.
				target = null;

			}

		}

		// We still may or may not have a target. If we don't, try to find one.
		if (target == null){

			// For now just do a box search starting with a box of radius 1, with the radius increasing.
			for (int r = 0; r < creatureType.sightRange; r++) {

				// Left and right column.
				for (int y = -r; y <= r; y++){
					if (TrySetTarget(-r, y) || TrySetTarget(r, y)){
						break;
					}
				}

				if (target != null){
					break;
				}

				// Top and bottom row.
				for (int x = t.x - r + 1; x <= t.x + r - 1; x++) {
					if (TrySetTarget(x, -r) || TrySetTarget(x, r)){
						break;
					}
				}

				if (target != null){
					break;
				}

			}

		}

		// Finally act based on whether or not there is a target.
		if (target != null){

			// We have a target.
			CreatureType.Stance stance;
			if ( target is Player ){
				stance = creatureType.playerStance;
			} else if (target is Creature){
				creatureType.creatureStances.TryGetValue (((Creature)target).creatureType, out stance);
			} else {
				// This shouldn't happen, and is not coded for yet.
				Debug.LogError ("MoveableEntity is neither creature nor player.");
				stance = CreatureType.Stance.Hostile;
			}

			// If we can attack it we should, but only if hostile or semi hostile.
			if ( ( stance == CreatureType.Stance.Hostile || stance == CreatureType.Stance.SemiHostile) && attackCooldown <= 0 && SquareDistance(target) < GetAttackRange() * GetAttackRange()){
				TryAttack (target);
			} else {

				// We can't attack it so move closer. First find out our relative position.
				int relx = t.x - target.t.x;
				int rely = t.y - target.t.y;

				// Also calculate their norm.
				int nx = Mathf.Abs (relx);
				int ny = Mathf.Abs (rely);

				// Normalise the relative ones so that they are only 1 or -1.
				if (nx != 0) {
					relx /= nx;
				}
				if (ny != 0) {
					rely /= ny;
				}

				// Now randomly choose to either move in the x direction or the y direction.
				int r = Random.Range(0, nx + ny);

				bool anyEvasive = stance == CreatureType.Stance.Evasive || stance == CreatureType.Stance.SemiEvasive;

				if (r < relx){
					if (anyEvasive){
						// Try to move in the y direction first.
						TryMoveTo (t.x, t.y + rely);
					} else {
						// Try to move in the x direction first.
						TryMoveTo (t.x - relx, t.y);
					}
				} else {
					if (anyEvasive){
						// Try to move in the x direction first.
						TryMoveTo (t.x + relx, t.y);
					} else {
						// Try to move in the y direction first.
						TryMoveTo (t.x, t.y - rely);
					}
				}

				// If we didn't move, then try reversing things.
				if (actionTimer <= 0){
					if (r >= relx){
						if (anyEvasive){
							// Try to move in the y direction now.
							TryMoveTo (t.x, t.y + rely);
						} else {
							// Try to move in the x direction now.
							TryMoveTo (t.x - relx, t.y);
						}
					} else {
						if (anyEvasive){
							// Try to move in the x direction now.
							TryMoveTo (t.x + relx, t.y);
						} else {
							// Try to move in the y direction now.
							TryMoveTo (t.x, t.y - rely);
						}
					}
				}

				// If that didn't work either then we will just stay here for now and pass.

			}

		} else {

			// There is no target. Walk in a random direction or else stay still.
			if (GC.Chance(P_NO_TARGET_IDLE)){

				// Don't do anything.
				return;

			} else {

				// Move randomly.
				int r = Random.Range (0, 2);
				if (r == 0){r = -1;}
				if (Random.Range(0,2) == 0){
					TryMoveTo (t.x + r, t.y);
				} else {
					TryMoveTo (t.x, t.y + r);
				}

			}

		}

	}

	public bool TrySetTarget( int x, int y ){

		// First check there is a target there.
		Entity e = GC.inst.map.GetAt (x, y).entity;
		if (e == null){
			return false;
		}

		// Now make sure the target isn't dead.
		if (e.dead){
			return false;
		}

		// Check the the target is moveable. We don't want to go around randomly chopping down trees, or trying to attack dummy entities.
		if (! (e is MoveableEntity)){
			return false;
		}

		// Check that we are hostile or evasive to the entity.
		if (e is Player){
			if (creatureType.playerStance == CreatureType.Stance.SemiEvasive || creatureType.playerStance == CreatureType.Stance.SemiHostile){
				return false;
			}
		} else if (e is Creature){
			CreatureType.Stance stance;
			creatureType.creatureStances.TryGetValue(((Creature) e).creatureType, out stance);
			if (stance == CreatureType.Stance.SemiEvasive || stance == CreatureType.Stance.SemiHostile){
				return false;
			}
		}

		// Finally double check we can see the creature.
		if (SquareDistance(e) >= creatureType.sightRange ){
			return false;
		}

		target = (MoveableEntity) e;

		return true;

	}


	public override Sprite GetSprite(){
		return creatureType.sprite;
	}

	public override Color GetSpriteColor ()
	{
		return creatureType.color;
	}

	public override int GetSpeed(){
		return creatureType.moveSpeed;
	}

	public override int GetAttackDamage (int sqrtDist) {
		return creatureType.attackDamage;
	}

	public override int GetAttackRange () {
		return creatureType.attackRange;
	}

	public override int GetAttackCoolDown () {
		return creatureType.attackCooldown;
	}

	public override void OnDeath () {

		// Change the itemtype under the tile we are on to be our drop type.
		t.storedItem = creatureType.itemTypeDropped;

		// Notify the GC of the change.
		GC.inst.NotifyTileItemChanged (t);

		base.OnDeath ();

	}

}
