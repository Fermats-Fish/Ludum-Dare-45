using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GC : MonoBehaviour {

	public static int rulesSeed = 0;

	public int turnsSurvived = 0;
	public float huntScore = 0f;

	const float secondsPerTurn = 0.1f;
	public bool waitingForPlayer = true;

	float timeSinceLastTurn = 0f;

	public static GC inst;

	public GameObject terrainPrefab;
	public GameObject entityPrefab;

	int NTerrainColours = 3;
	int NCreatureColours = 3;

	public Map map;

	// Apparently perlin noise gets glitchy at around 50,000.
	float MAX_PERLIN_SEED = 25000f;

	// Terrain visuals.
	DoubleList2D<TileVisual> mapTerrainVisuals = new DoubleList2D<TileVisual>();

	// Current range of the terrain visuals.
	Range terrainVisualRange = new Range (0, -1, 0, -1);

	public List<Transition> animations = new List<Transition> ();

	Sprite attackOrbSprite;

	void Start () {

		// Make this instance of the class publicly available.
		inst = this;

		// Load the attack orb sprite.
		attackOrbSprite = Resources.Load<Sprite>("attackOrb");

		// Set game rules from seed.
		SetGameRulesFromSeed (rulesSeed);

		// Generate the map from a seed.
		GenerateMapFromSeed (System.DateTime.Now.Millisecond);

		// Generate the map visuals.
		RecalculateMapVisuals ();

		// Now generate the ui visuals.
		UIController.inst.SetUp();

	}
	
	void Update () {

		// First check for any moveable entities which need adding to the map, or which need deleting because they are dead.
		if (map.addToMoveableEntities.Count > 0){
			foreach (MoveableEntity e in map.addToMoveableEntities) {
				map.moveableEntities.Add (e);
			}
			map.addToMoveableEntities = new List<MoveableEntity> ();
		}
		if (map.removeFromMoveableEntities.Count > 0){
			foreach (MoveableEntity e in map.removeFromMoveableEntities) {
				map.moveableEntities.Remove (e);
			}
			map.removeFromMoveableEntities = new List<MoveableEntity> ();
		}

		if (waitingForPlayer){

			// Mouse Controls...
			if (!UIController.MouseOverUI() && Input.GetMouseButtonDown(0)){

				// Find the mouse coords.
				Vector3 mouseScreenPos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				Coord mapPos = new Coord (Mathf.FloorToInt (mouseScreenPos.x), Mathf.FloorToInt (mouseScreenPos.y));

				// Get the tile at these coords.
				Tile t = map.GetAt (mapPos);

				// See if there is an entity we can attack at these coords...
				if (t.entity != null && t.entity.dead == false){
					map.player.TryAttack (map.GetAt (mapPos).entity);
				}

				// If not, see if there is an item we can pick up at these coords. Note that this already means we can't pick up items where there is an entity.
				else if (t.storedItem != null){
					map.player.PickUpItemOnTile (t);
				}

			}

			// Go Up
			else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)){
				map.player.TryMoveUp ();
			}

			// Go Right
			else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)){
				map.player.TryMoveRight ();
			}

			// Go Down
			else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)){
				map.player.TryMoveDown ();
			}

			// Go Left
			else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)){
				map.player.TryMoveLeft ();
			}

			// Do Nothing
			else if (Input.GetKey(KeyCode.Space)){
				map.player.Pass ();
			}


		} else {

			// Time passes.
			timeSinceLastTurn = timeSinceLastTurn + Time.deltaTime;

			// Clamp the time so that animations don't do weird things.
			if (timeSinceLastTurn > secondsPerTurn){
				timeSinceLastTurn = secondsPerTurn;
			}

			// Move animations, if they finish then remove them from the list.
			List<Transition> toRemove = new List<Transition> ();
			foreach (Transition animation in animations) {
				if (animation.SetPercent (timeSinceLastTurn / secondsPerTurn)){
					toRemove.Add (animation);
				}
			}
			foreach (Transition animation in toRemove) {
				animations.Remove (animation);
			}

			// See if enough time has passed for a new turn.
			if (timeSinceLastTurn == secondsPerTurn){
				timeSinceLastTurn = 0f;

				// Go through each entity which isn't busy currently and get them to choose an action, but only if they are alive of course.
				// The alive check is needed since an entity might die in a prior turn, in which case we can't remove it from the moveableEntities list until the end of this loop.
				foreach (MoveableEntity entity in map.moveableEntities) {
					if (!entity.dead) {
						entity.TakeTurn ();
					}
				}

				if (!map.player.dead) {
					turnsSurvived += 1;
				}

				// Update the score display.
				UIController.inst.UpdateScoreUI ();

			}

		}

	}

	private void SetGameRulesFromSeed(int seed){

		// Apply the seed.
		Random.InitState (seed);

		Debug.Log ("Generating Rules From Seed: " + seed);

		//////////////// First create the terrain types, looping through each possible sprite.
		TerrainType.terrainTypes = new List<TerrainType> ();
		foreach (var sprite in Resources.LoadAll<Sprite>("Terrain")) {

			// Loop through a set of colours for this sprite. There will be one terrain type per sprite / colour combination.
			foreach (Color color in GenerateRandomColours(NTerrainColours, 0.85f, 0.55f)) {
				TerrainType.GenerateNewTerrainType ( sprite, color );
			}

		}

		/////////////// Generate the creature types by looping through each creature sprite.
		CreatureType.creatureTypes = new List<CreatureType> ();
		foreach (Sprite sprite in Resources.LoadAll<Sprite> ("Creatures")) {

			// Loop through a set of colours for this sprite. There will be one creature type per sprite / colour combination.
			foreach (Color color in GenerateRandomColours(NCreatureColours, 0.85f, 0.9f)) {
				
				CreatureType.GenerateNewCreatureType ( sprite, color );

			}

		}

		// Now for each creature generate its stance with each other creature.
		foreach (CreatureType creatureType in CreatureType.creatureTypes) {
			creatureType.GenerateStancesWithOtherCreatures ();
		}

		/////////////// Genearte BASIC items.
		 
		ItemType.itemTypes = new List<ItemType> ();

		// First create the basic items.
		Sprite[] sprites = Resources.LoadAll<Sprite> ("Items/Basic");
		List<ItemType> basicItems = new List<ItemType> ();
		for (int i = 0; i < sprites.Length * 3; i++) {
			basicItems.Add (ItemType.GenerateNewBasicItemType ());
		}

		// Now sort the basic items by rarity, rarist first.
		basicItems.Sort (((ItemType x, ItemType y) => {
			if (x.rarity < y.rarity){
				return 1;
			}
			return -1;
		}));

		// Now sort the creatures by rarity, rarist first.
		CreatureType.creatureTypes.Sort (((CreatureType x, CreatureType y) => {
			if (x.diff < y.diff){
				return 1;
			}
			return -1;
		}));

		// Loop throug each creature.
		for (int i = 0; i < CreatureType.creatureTypes.Count; i++) {

			// If there is an ith creature, assign the ith item type as its drop type. This will give the rarist items to the most powerfull creatures.
			CreatureType.creatureTypes [i].itemTypeDropped = basicItems [i];

			// Give this item a sprite and colour which "matches" the creature's.
			string spriteName = CreatureType.creatureTypes [i].sprite.name;
			basicItems [i].sprite = Resources.Load<Sprite> ("Items/Basic/" + spriteName);
			basicItems [i].color = CreatureType.creatureTypes [i].color;
			basicItems [i].color.a = 0.8f;

		}

		// Loop through unassigned item types, however do it by sprite / colour.
		Sprite[] itemSprites = Resources.LoadAll<Sprite>("Items/Basic");
		int itemAt = CreatureType.creatureTypes.Count;
		for (int spriteAt = CreatureType.creatureTypes.Count / 3; spriteAt < itemSprites.Length; spriteAt++) {

			foreach (Color color in GenerateRandomColours(3, 0.85f, 0.9f)) {
				
				// This item will spawn naturally in a world. Pick a terrain type for it to spawn in.
				TerrainType tt = TerrainType.terrainTypes [Random.Range (0, TerrainType.terrainTypes.Count)];
				tt.itemsFoundHere.Add (basicItems [itemAt]);

				// Set the colour and texture.
				basicItems [itemAt].sprite = itemSprites [spriteAt];
				basicItems [itemAt].color = color;
				basicItems [itemAt].color.a = 0.8f;

				// Increase the item index.
				itemAt += 1;

			}

		}


		/////////////// Genearte CRAFTED items.

		CraftingRecipe.craftingRecipes = new List<CraftingRecipe> ();

		// Loop through sprites.
		foreach (Sprite sprite in Resources.LoadAll<Sprite>("Items/Crafted")) {

			// Loop through colours.
			foreach (var color in GenerateRandomColours(3, 0.85f, 0.9f)) {

				// Create a crafted item wit the sprite and colours.
				ItemType it = ItemType.GenerateNewCraftedItemType (sprite, color);

				// Generate the two crafting recipes for the item.
				CraftingRecipe.BuildRecipeFor (it, basicItems);
				CraftingRecipe.BuildRecipeFor (it, basicItems);

			}

		}

		// Reset Random Seed.
		Random.InitState (System.DateTime.Now.Millisecond);

		Debug.Log ("Rules created with "
			+ TerrainType.terrainTypes.Count       + " terrain types, "
			+ CreatureType.creatureTypes.Count     + " creature types, "
			+ ItemType.itemTypes.Count             + " item types, and "
			+ CraftingRecipe.craftingRecipes.Count + " crafting recipes.");

	}

	public void RecalculateMapVisuals(){

		// Get the player's coordinates.
		int px = map.player.t.x;
		int py = map.player.t.y;

		// Figure out the range which the camera can see.
		int ySize = (int) Camera.main.orthographicSize;

		// The range is created to have a 2 tile gap. This is to make movements easier.
		const int GAP = 2;
		Range newRange = new Range (px - (int)((ySize + 1) * Camera.main.aspect) - GAP, px + (int)((ySize + 1) * Camera.main.aspect) - 1 + GAP, py - (ySize + 1) - GAP, py + ySize + GAP);

		// Remove all tile visuals outside of this range.
		List<TileVisual> toRemove = mapTerrainVisuals.TrimToRange (newRange);
		foreach (TileVisual tv in toRemove) {
			tv.Destroy ();
		}

		// Add any new tiles which weren't in the old range.

		// Loop through all columns in the new range...
		for (int x = newRange.minX; x <= newRange.maxX; x++) {

			// If this x coordinate was not in the old range, add the whole column.
			if (x < terrainVisualRange.minX || x > terrainVisualRange.maxX){
				for (int y = newRange.minY; y <= newRange.maxY; y++) {
					GenerateMapVisualAt (x,y);
				}
			}

			// Otherwise do the top and bottom seperately, avoiding anything which is already generated.
			else {
				for (int y = newRange.minY; y < terrainVisualRange.minY; y++){
					GenerateMapVisualAt(x,y);
				}
				for (int y = terrainVisualRange.maxY + 1; y <= newRange.maxY; y++){
					GenerateMapVisualAt(x,y);
				}
			}

		}

		// Set the terrain visual range.
		terrainVisualRange = newRange;

	}

	private void GenerateMapVisualAt(int x, int y){

		// Get the tile here.
		Tile t = map.GetAt (x, y);

		// Check we don't already have a tile visual here.
		if (mapTerrainVisuals.GetAt(x,y) != null){
			Debug.LogError("Trying to generate a tile visual at " + x + ", " + y + " but there is already a tile visual at those coordinates.");
		}

		mapTerrainVisuals.SetAt (x, y, new TileVisual (t));

	}

	/// <summary>
	/// Sets up the map seed etc. Doesn't generate any terrain because that should happen automatically as the player moves around.
	/// </summary>
	/// <param name="seed">The random seed for the map.</param>
	private void GenerateMapFromSeed(int seed){

		// Apply the seed to the random number generator.
		Random.InitState (seed);

		// Now set the perlin noise seed for each terrain type. We will use a different perlin noise seed for each terrain type.
		//   We will then see for which terrain type, perlin noise is highest at a given point, to determine the terrain type at that point.
		//   Since different terrain types have different probabilities of occuring, we will balance this by raising the perlin noise to the power of 1/(spawn chance).M
		foreach (TerrainType terrainType in TerrainType.terrainTypes) {
			terrainType.perlinSeedX = Random.Range (-MAX_PERLIN_SEED, MAX_PERLIN_SEED);
			terrainType.perlinSeedY = Random.Range (-MAX_PERLIN_SEED, MAX_PERLIN_SEED);
		}

		// Generate a map.
		map = new Map();
		map.PlacePlayer ();

		// Reset Random Seed.
		Random.InitState (System.DateTime.Now.Millisecond);

	}

	/// <summary>
	/// Returns true or false depending on a probability.
	/// </summary>
	/// <param name="prob">The probability of returning true</param>.
	public static bool Chance(float prob){
		return Random.Range (0f, 1f) <= prob;
	}

	/// <summary>
	/// Generates a list of random colours which are spaced equal distances from each other on a colour wheel.
	/// </summary>
	/// <returns>The random colours.</returns>
	/// <param name="numberOfColoursToGenerate">Number of colours to generate.</param>
	/// <param name="brightness">Brightness of each colour.</param>
	/// <param name="saturation">Saturation of each colour.</param>
	static Color[] GenerateRandomColours(int numberOfColoursToGenerate, float brightness, float saturation){

		// Get a random hue for the first number.
		float randomHue = Random.Range (0f, 1f);

		// Get the hue spacing between each colour.
		float hueSpacing = 1f / ((float)numberOfColoursToGenerate);

		// Initialise the list.
		Color[] colors = new Color[numberOfColoursToGenerate];

		// Generate the colours.
		for (int i = 0; i < numberOfColoursToGenerate; i++) {

			// Calculate the hue for this colour.
			float hue = randomHue + i * hueSpacing;
			if (hue > 1f){
				hue -= 1f;
			}

			colors [i] = Color.HSVToRGB (hue, saturation, brightness);
		}

		return colors;

	}

	/// <summary>
	/// Linearly remaps a value from between oldMin and oldMax to a value between newMin and newMax.
	/// </summary>
	/// <returns>The map.</returns>
	/// <param name="value">Value.</param>
	/// <param name="oldMin">Old minimum.</param>
	/// <param name="oldMax">Old max.</param>
	/// <param name="newMin">New minimum.</param>
	/// <param name="newMax">New max.</param>
	public static float ReMap( float value, float oldMin, float oldMax, float newMin, float newMax ){
		// Map from 0 to 1.
		value = (value - oldMin) / (oldMax - oldMin);

		// Map from newMin to newMax.
		value = value * (newMax - newMin) + newMin;

		// Return the value.
		return value;
	}

	public void NotifyAttack(MoveableEntity source, Entity target){
		
		// For now, always display the attack even if off screen. This is because for example there might be an attack from off the top of the screen to off the left of the screen,
		//   where even though you can't see either entity you can still see the attack orb.

		// Set the default sprite and colour.
		Sprite s = attackOrbSprite;
		Color c = source.GetSpriteColor();

		// If the player is attacking however...
		if (source is Player){
			Player p = (Player)source;
			// See if the player has an item selected...
			ItemType it = p.inv.GetSelectedItemType ();
			if (it != null){
				// Use the sprite for this item type instead.
				s = it.sprite;
				c = it.color;
			}
		}

		animations.Add ( new Transition( s, c, new Vector3(source.t.x, source.t.y, 3f), new Vector3(target.t.x, target.t.y, 3f), 1 ) );
	}

	public void NotifyEntityMovementStart(MoveableEntity ent){
		TileVisual tv = mapTerrainVisuals.GetAt (ent.t.x, ent.t.y);
		if (tv == null || tv.entityGO == null) {
			return;
		} else {
			animations.Add (new Transition (tv.entityGO, new Vector3 (ent.movingTo.x, ent.movingTo.y), ent.actionTimer));
		}

	}
	public void NotifyEntityMovementEnd(MoveableEntity ent){

		// Get the visual for the old tile it was on and unlink the entity from it.
		TileVisual oldTV = mapTerrainVisuals.GetAt (ent.t.x, ent.t.y);
		GameObject entGO;
		if (oldTV != null){
			entGO = oldTV.entityGO;
			mapTerrainVisuals.GetAt (ent.t.x, ent.t.y).entityGO = null;
		} else {
			// There was no old tile visual. We will have to create a new one.
			entGO = TileVisual.CreateEntityVisual (map.GetAt(ent.movingTo));
		}

		// Set as the entity visual for another tile visual, but only if that tile is on screen...
		TileVisual newTV = mapTerrainVisuals.GetAt (ent.movingTo.x, ent.movingTo.y);
		if (newTV != null){
			if (newTV.entityGO != null){
				Destroy (newTV.entityGO);
			}
			newTV.entityGO = entGO;
		} else {
			// This enttity game object isn't needed.
			Destroy (entGO);
		}

	}

	public void NotifyEntityHealthChange(Entity ent){

		// See if we are visible.
		TileVisual tv = mapTerrainVisuals.GetAt (ent.t.x, ent.t.y);
		if (tv != null){

			// Now if the entity has 0 health...
			if (ent.health == 0){

				// Destroy the entity visual.
				Destroy (tv.entityGO);
				tv.entityGO = null;

			}

			// Otherwise if the entity has full health...
			else if (ent.health == ent.maxHealth){

				// Hide the entities damage bar.
				tv.entityGO.transform.GetChild (0).gameObject.SetActive (false);

			}

			// Otherwise...
			else {
				
				// Show the entities damage bar.
				if (tv.entityGO == null){
					Debug.Log (ent.t.x + ", " + ent.t.y + ", " + ent.GetSprite() + ", " + ent.dead + ", " + ent.health + "/" + ent.maxHealth);
				} else {
					GameObject bar = tv.entityGO.transform.GetChild (0).gameObject;
					bar.SetActive (true);

					// Scale it appropriately. 0.9 scale should be 100% health.
					bar.transform.localScale = new Vector3 (ReMap (ent.health, 0, ent.maxHealth, 0, 0.9f), 0.075f, 1f);
				}

			}
		}

	}

	public void NotifyTileItemChanged(Tile t){

		// Check the tile is visible.
		TileVisual tv = mapTerrainVisuals.GetAt (t.x, t.y);
		if (tv != null){

			// If the tile already had an item visual, remove it.
			if (tv.itemGO != null){
				Destroy (tv.itemGO);
				tv.itemGO = null;
			}

			// Create the new tile visual, if one is to be created.
			if (t.storedItem != null) {
				tv.itemGO = TileVisual.CreateItemVisual (t);
			}
		}

	}

}
