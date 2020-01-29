using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableEntity : Entity {

	public Coord movingTo;

	public int actionTimer;
	public int attackCooldown = 0;

	public MoveableEntity(Tile t) : base(t) {

		GC.inst.map.addToMoveableEntities.Add (this);

	}

	public bool TryMoveTo(int x, int y){
		return TryMoveTo (new Coord (x, y));
	}

	public bool TryMoveTo(Coord moveToCoord){

		// Check that the tile we are moving to is exactly one square away from the one we are on.
		if (Mathf.Abs(t.x - moveToCoord.x) + Mathf.Abs(t.y - moveToCoord.y) != 1){
			return false;
		}

		// Check that we can move to the tile we are trying to move to.
		Tile moveToTile = GC.inst.map.GetAt(moveToCoord.x, moveToCoord.y);
		if (moveToTile.Impassable()){
			return false;
		}

		// We can move here.

		// Bags this tile by setting this tile's entity to also be this entity.
		moveToTile.entity = this;
		moveToTile.renderEntity = false;

		// Set our moving to position.
		movingTo = moveToCoord;

		// Set our action timer according to how long it will take.
		actionTimer = moveToTile.terrainType.terrainSpeed * GetSpeed () / 2;
		if (actionTimer == 0){
			actionTimer = 1;
		}

		// Tell the game controller we are moving so that it can animate us doing so.
		GC.inst.NotifyEntityMovementStart (this);

		return true;

	}

	public virtual bool TryAttack ( Entity ent ){

		// Check not null.
		if (ent == null){
			return false;
		}

		// Check range.
		if (!InRange(ent)){
			return false;
		}

		// Check attack cooldown.
		if (attackCooldown > 0){
			return false;
		}

		// Check the target isn't us...
		if (ent == this){
			return false;
		}

		// Deal damage.
		ent.TakeDamage (GetAttackDamage( SquareDistance(ent) ));

		// Set cooldown.
		attackCooldown = GetAttackCoolDown ();

		// Set action timer.
		actionTimer = 1;

		// If the other entity was a creature, then it will target this entity.
		if (ent is Creature){
			((Creature)ent).target = this;
		}

		// Notify the game controller of the attack.
		GC.inst.NotifyAttack (this, ent);

		return true;

	}

	public bool InRange (Entity e){
		return SquareDistance(e) < GetAttackRange () * GetAttackRange ();
	}

	public bool InRangeOfPlayer(){
		return SquareDistance (GC.inst.map.player) < Map.ACTIVE_ENTITY_RADIUS_FROM_PLAYER * Map.ACTIVE_ENTITY_RADIUS_FROM_PLAYER;
	}

	/// <summary>
	/// Tells the entity to move. If this is a player, then then player will just tell the GC to wait for a user input.
	/// This means that there shouldn't be any code after a call to Move() which expects that a movement has been logged.
	/// Also note that this will just get the entity to schedule a new action. It won't actually move the entity until some time has passed.
	/// </summary>
	public void TakeTurn(){

		if (dead){
			Debug.LogError ("Dead creature trying to take turn");
		}

		// Either need to be in range of the player, or for your target to be. The target option is so that a creature in range can't beat up one out of range.
		if (!(InRangeOfPlayer () || (this is Creature && ((Creature)this).target != null && ((Creature)this).target.InRangeOfPlayer()))) {
			return;
		}

		// If we are on terrain which deals damage to us, then take damage. MIGHT WANT TO CHANGE THIS TO ONLY ON ENTERING THE TILE!
		if (t.terrainType.damagePerTurn > 0){
			TakeDamage (t.terrainType.damagePerTurn);
		}

		// If the entity died from moving through the terrain, then we should end its turn here so it doesn't reassign itself to a tile.
		if (dead){
			return;
		}

		actionTimer -= 1;
		attackCooldown -= 1;

		if (actionTimer == 0){

			// We just finished an action.
			OnActionCompletion ();

		}

		if (actionTimer <= 0){

			// We need a new action...
			SelectNewAction ();

		}

	}

	public virtual void OnActionCompletion(){

		// If our current position isn't our target position...
		if (movingTo != null && (t.x != movingTo.x || t.y != movingTo.y)) {

			// Move position!

			// Set the new tile's entity as us.
			GC.inst.map.GetAt (movingTo.x, movingTo.y).entity = this;
			GC.inst.map.GetAt (movingTo.x, movingTo.y).renderEntity = true;

			// Now tell the game controller about our movements.
			GC.inst.NotifyEntityMovementEnd (this);

			// Now unset us from our current tile.
			t.entity = null;

			// Now set our tile to the new tile, and that tile to us.
			t = GC.inst.map.GetAt (movingTo.x, movingTo.y);

			// Tell ourselves we are no longer moving.
			movingTo = null;

		}

	}

	public virtual void SelectNewAction(){

		// Implementation depends on whether a creature or a player.

	}

	public virtual int GetSpeed(){

		return 2;

	}

	public override void OnDeath () {

		// If we are assigned to two tiles because we are moving to one, then unassign from the one we are moving to.
		if (movingTo != null){
			GC.inst.map.GetAt (movingTo).entity = null;
		}
		base.OnDeath ();
		GC.inst.map.removeFromMoveableEntities.Add (this);
	}

	public virtual int GetAttackRange(){
		return 3;
	}

	public virtual int GetAttackDamage(int sqrtDist){
		return 1;
	}

	public virtual int GetAttackCoolDown(){
		return 4;
	}

}
