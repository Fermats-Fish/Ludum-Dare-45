using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MoveableEntity{

	const int PLAYER_PICKUP_RANGE = 2;

	public Inventory inv;

	public Player (Tile t) : base(t){
		GC.inst.map.GenerateInBounds ( new Range(t.x - Map.PLAYER_GENERATE_RADIUS, t.x + Map.PLAYER_GENERATE_RADIUS, t.y - Map.PLAYER_GENERATE_RADIUS, t.y + Map.PLAYER_GENERATE_RADIUS) );
		inv = new Inventory (ItemType.itemTypes.Count);
	}

	public override void SelectNewAction(){

		// Tell the game controller to pause everything until the user selects an input.
		GC.inst.waitingForPlayer = true;

	}

	public void TryCraftRecipe(CraftingRecipe recipe){

		// Check we have all the required items.
		if (! CanCraft(recipe)){
			return;
		}

		GC.inst.waitingForPlayer = false;
		actionTimer = 1;

		// Remove all the input resources.
		foreach (ItemStack inputStack in recipe.inputStacks) {
			inv.RemoveStack (inputStack);
		}

		// Add the output resource.
		inv.AddStack (recipe.outputStack);

	}

	public bool CanCraft(CraftingRecipe recipe){
		foreach (ItemStack inputStack in recipe.inputStacks) {
			if (inv.GetAmountOf(inputStack.itemType) < inputStack.stackSize){
				return false;
			}
		}
		return true;
	}

	public void EquipUnequipItem(ItemType itemType){

		// If already equipped, unequip.
		if (inv.GetSelectedItemType() == itemType){
			inv.SetSelectedItemType (null);
		} else {
			inv.SetSelectedItemType (itemType);
		}

		actionTimer = 1;
		GC.inst.waitingForPlayer = false;
	}

	public void EatItem(ItemType it){
		if (inv.GetAmountOf(it) > 0){
			inv.RemoveStack (new ItemStack (it, 1));
		}

		actionTimer = 1;
		GC.inst.waitingForPlayer = false;

		TakeDamage (-it.healthEffect);

	}

	public void PickUpItemOnTile(Tile pickupTile){

		// First check that the tile is in range.
		if ( ! InPickupRange(pickupTile) ){
			return;
		}

		// Tell the game controller to unpause once we are finished.
		GC.inst.waitingForPlayer = false;

		// Add one of the tile's stored item to our inventory.
		ItemType pickUpIT = pickupTile.storedItem;
		inv.AddStack (new ItemStack (pickUpIT, 1));

		// Set the tile's stored item to null.
		pickupTile.storedItem = null;

		// Notify the game controller to remove the tile visual for this item.
		GC.inst.NotifyTileItemChanged(pickupTile);

		actionTimer = 1;

	}

	public bool InPickupRange(Tile pickupTile){
		return (pickupTile.x - t.x) * (pickupTile.x - t.x) + (pickupTile.y - t.y) * (pickupTile.y - t.y) < PLAYER_PICKUP_RANGE * PLAYER_PICKUP_RANGE;
	}

	public override int GetAttackDamage (int sqrDist) {
		ItemType it = inv.GetSelectedItemType ();
		if (it == null) {
			return base.GetAttackDamage (sqrDist);
		} else {
			if (sqrDist <= it.meleeRange * it.meleeRange){
				return it.meleeDamage;
			} else {
				return it.throwDamage;
			}
		}
	}

	public override int GetAttackRange () {
		if (inv.GetSelectedItemType () != null) {
			return inv.GetSelectedItemType ().throwRange;
		} else {
			return base.GetAttackRange ();
		}
	}

	public override int GetAttackCoolDown () {
		return base.GetAttackCoolDown (); //////////////////////TODO: LATER HAVE CUSTOM TOOL ATTACK COOLDOWNS.
	}

	public bool TryMoveBy(Coord del){

		// Try to move as specified, and tell the game controller to release control.
		GC.inst.waitingForPlayer = false;
		return TryMoveTo (new Coord (t.x + del.x, t.y + del.y));
	}

	public void TryMoveUp(){
		if (TryMoveBy (new Coord (0, 1))){
			GC.inst.map.GenerateInBounds (new Range ( t.x - Map.PLAYER_GENERATE_RADIUS, t.x + Map.PLAYER_GENERATE_RADIUS, t.y + Map.PLAYER_GENERATE_RADIUS + 1, t.y + Map.PLAYER_GENERATE_RADIUS + 1 ));
		}
	}

	public void TryMoveRight(){
		if (TryMoveBy (new Coord (1, 0))){
			GC.inst.map.GenerateInBounds (new Range ( t.x + Map.PLAYER_GENERATE_RADIUS + 1, t.x + Map.PLAYER_GENERATE_RADIUS + 1, t.y - Map.PLAYER_GENERATE_RADIUS, t.y + Map.PLAYER_GENERATE_RADIUS ));
		}
	}

	public void TryMoveDown(){
		if (TryMoveBy (new Coord (0, -1))){
			GC.inst.map.GenerateInBounds (new Range ( t.x - Map.PLAYER_GENERATE_RADIUS, t.x + Map.PLAYER_GENERATE_RADIUS, t.y - Map.PLAYER_GENERATE_RADIUS - 1, t.y - Map.PLAYER_GENERATE_RADIUS - 1 ));
		}
	}

	public void TryMoveLeft(){
		if (TryMoveBy (new Coord (-1, 0))){
			GC.inst.map.GenerateInBounds (new Range ( t.x - Map.PLAYER_GENERATE_RADIUS - 1, t.x - Map.PLAYER_GENERATE_RADIUS - 1, t.y - Map.PLAYER_GENERATE_RADIUS, t.y + Map.PLAYER_GENERATE_RADIUS ));
		}
	}

	public void Pass(){
		GC.inst.waitingForPlayer = false;
	}

	public override bool TryAttack(Entity e){

		// Release control.
		GC.inst.waitingForPlayer = false;

		// Store whether the attack is successful.
		bool outcome = base.TryAttack (e);

		// If it is and this was a ranged attack, we need to decrease the amount of the specified resource, and then place that resource on the tile with the target.
		ItemType sel = inv.GetSelectedItemType ();
		if (outcome && sel != null && SquareDistance (e) > sel.meleeRange * sel.meleeRange){
			inv.RemoveStack (new ItemStack (sel, 1));

			// Only place the attack object on the tile that target is on if the target doesn't die. This is because if the target dies then we want its drop to go on the tile.
			if (e.dead == false){
				
				// Change the itemtype under the tile we are on to be our drop type.
				e.t.storedItem = sel;

				// Notify the GC of the change.
				GC.inst.NotifyTileItemChanged (e.t);
			}

		}

		// If we killed an entity and it was a creature then increase our hunt score.
		if (e.dead && (e is Creature)) {
			GC.inst.huntScore += ((Creature)e).creatureType.diff;
			UIController.inst.UpdateScoreUI ();
		}

		// Return the outcome.
		return outcome;
	}

	public override void OnActionCompletion () {
		base.OnActionCompletion ();
		GC.inst.RecalculateMapVisuals ();
	}

	public override Sprite GetSprite(){
		return Resources.Load<Sprite> ("player");
	}

	public override void OnDeath () {
		Camera.main.gameObject.transform.SetParent (null, true);
		base.OnDeath ();
		UIController.inst.DisplayDeathScreen ();
	}

	public override void TakeDamage(int amount){
		base.TakeDamage(amount);
		UIController.inst.UpdateHealthDisplay ();
	}

}
