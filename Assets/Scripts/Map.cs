using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map {

	const float TERRAIN_TYPE_WAVELENGTH_MODIFIER = 30f;
	const float P_CREATURE_ON_TILE = 1f/80f;
	const float P_ITEM_ON_TILE = 1f/60f;

	public const int ACTIVE_ENTITY_RADIUS_FROM_PLAYER = (int) (1.5f * CreatureType.MAX_SIGHT_RANGE) + 1;
	public const int PLAYER_GENERATE_RADIUS = ACTIVE_ENTITY_RADIUS_FROM_PLAYER + CreatureType.MAX_ATTACK_RANGE;

	DoubleList2D<Tile> tileMap = new DoubleList2D<Tile>();

	public List<MoveableEntity> moveableEntities = new List<MoveableEntity>();
	public List<MoveableEntity> addToMoveableEntities = new List<MoveableEntity> ();
	public List<MoveableEntity> removeFromMoveableEntities = new List<MoveableEntity> ();

	public Player player;

	public Map (){
		
	}

	public void PlacePlayer(){
		// Create a player object and place them in the centre of the map.
		player = new Player (GetAt(0,0));
		GetAt (0, 0).entity = player;
	}

	public void GenerateInBounds(Range r){
		for (int x = r.minX; x < r.maxX; x++) {
			for (int y = r.minY; y < r.maxY; y++) {
				GetAt (x, y);
			}
		}
	}

	/// <summary>
	/// Gets the tile at the x,y coordinates. If there is no tile there yet, then it generates that tile.
	/// </summary>
	/// <returns>The <see cref="Tile"/> at x,y.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public Tile GetAt(int x, int y){

		Tile t = tileMap.GetAt (x, y);

		if (t == null){
			return GenerateTileAt (x, y);
		}

		return t;

	}

	public Tile GetAt(Coord c){
		return GetAt(c.x, c.y);
	}

	Tile GenerateTileAt(int x, int y){

		// Get the terrain type.
		TerrainType t = GenerateTerrainTypeAt (x, y);

		// Create the tile.
		Tile tile = new Tile (x, y, t);

		// Set the tile.
		tileMap.SetAt (x, y, tile);

		// Should a creature spawn here?
		if (GC.Chance(P_CREATURE_ON_TILE)){

			// Yes. Get which one from the tile type.
			CreatureType ct = t.GetRandomCreatureType ();

			tile.entity = new Creature (ct, tile);

		}

		// Should an item spawn here?
		if (GC.Chance (P_ITEM_ON_TILE)){

			// Yes. Get which one from the tile type.
			ItemType it = t.GetRandomItemType ();

			tile.storedItem = it;

		}

		// Return the tile.
		return tile;

	}


	TerrainType GenerateTerrainTypeAt(int x, int y){

		// Keep track of the terrain which scores the highest.
		TerrainType curHighestTerrain = null;
		float highScore = Mathf.NegativeInfinity;

		foreach (TerrainType tt in TerrainType.terrainTypes) {

			// Calculate the score for this terrain type.
			float score = Mathf.PerlinNoise ( tt.perlinSeedX + (float) x / TERRAIN_TYPE_WAVELENGTH_MODIFIER, tt.perlinSeedY + (float) y / TERRAIN_TYPE_WAVELENGTH_MODIFIER );

			// To compensate for some terrain types being more likely to occur than others we should raise the score to the power of 1/rarity.
			// This is because perlin noise outputs numbers roughly in the range of 0 to 1, hence raising it to the power of 2 for example should push values down.
			score = Mathf.Pow(score, 1f / tt.rarity);

			// Check if this is the best score so far.
			if (score > highScore){
				highScore = score;
				curHighestTerrain = tt;
			}

		}

		return curHighestTerrain;

	}

}


public class Tile {

	public TerrainType terrainType;
	public Entity entity;

	public int x;
	public int y;

	public ItemType storedItem = null;

	public bool renderEntity = true;

	public Tile(int x, int y, TerrainType terrainType){
		this.x = x;
		this.y = y;
		this.terrainType = terrainType;
	}

	public bool Impassable(){
		return this.entity != null;
	}

}
