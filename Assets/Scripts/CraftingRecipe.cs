using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingRecipe {

	const int INIT_MAX_IN_RARITY_MINUS_OUT = 3;

	public static List<CraftingRecipe> craftingRecipes;

	public List<ItemStack> inputStacks = new List<ItemStack>();
	public ItemStack outputStack;

	public CraftingRecipe(ItemType outputType){
		
		outputStack = new ItemStack (outputType, 1);

		craftingRecipes.Add (this);

	}

	public float GetInRarity(){
		float sum = 0f;
		foreach (ItemStack stack in inputStacks) {
			sum += stack.stackSize * stack.itemType.rarity;
		}
		return sum;
	}

	public float GetOutRarity(){
		return outputStack.itemType.rarity * outputStack.stackSize;
	}

	public static CraftingRecipe BuildRecipeFor(ItemType it, List<ItemType> basicItemTypes){

		// Create a crafting recipe.
		CraftingRecipe cr = new CraftingRecipe(it);

		// First select 1-3 resources, but less often 1.
		int nInputs = Mathf.FloorToInt(Random.Range (1.5f, 4f));
		List<ItemType> inputTypes = new List<ItemType> ();

		while (inputTypes.Count < nInputs){
			// Choose a random item type.
			ItemType rand = basicItemTypes [Random.Range (0, basicItemTypes.Count)];

			// If its not in the list already, then add it.
			if (! inputTypes.Contains(rand) ){
				inputTypes.Add (rand);
			}
		}

		// For now add one of each.
		foreach (ItemType inputType in inputTypes) {
			cr.inputStacks.Add (new ItemStack (inputType, 1));
		}

		// If the input rarity is greatly larger than the output rarity, increase the number of items produced in the output.
		while (cr.GetInRarity() > cr.GetOutRarity() + INIT_MAX_IN_RARITY_MINUS_OUT){
			cr.outputStack.stackSize += 1;
		}

		// While the input rarity is lower than the output rarity, increase the size of one of the input stacks.
		while (cr.GetInRarity() < cr.GetOutRarity()){

			// Choose a random stack number.
			int rand = Random.Range (0, cr.inputStacks.Count);

			// Increase that stack's size by 1.
			cr.inputStacks [rand].stackSize += 1;

		}

		return cr;

	}

	public static CraftingRecipe BuildRecipeWhichUses(ItemType it){

		// Find a resource which is used in lots of crafting recipes...
		int[] counts = new int[ItemType.itemTypes.Count];

		// Loop through recipes...
		foreach (CraftingRecipe recipe in craftingRecipes) {
			foreach (ItemStack inputStack in recipe.inputStacks) {
				counts [inputStack.itemType.itemID] += inputStack.stackSize;
			}
		}

		// Get the item with the largest count.
		int maxSoFar = 0;
		int best = 0;
		for (int i = 0; i < counts.Length; i++) {
			if (counts[i] > maxSoFar){
				best = i;
				maxSoFar = counts [i];
			}
		}

		// Get the most used input item type.
		ItemType outIT = ItemType.itemTypes [best];

		// We want to balance the amounts. Since these will probabily both be raw materials they will mostly have a rarity between 1 and 2.1, sometimes between 0.75 and 2.5.
		// Hence if we multiply by 2 its between 2 and 4, or sometimes 1.5 and 5. Hence if we multiply each by 3 and round down, and use that as the stack size for the other item,
		//    it should be kind of balanced.
		CraftingRecipe cr = new CraftingRecipe (outIT);
		cr.inputStacks.Add (new ItemStack (it, Mathf.CeilToInt (outIT.rarity * 3)));
		cr.outputStack.stackSize = Mathf.FloorToInt (it.rarity * 3);

		// If the in rarity is not more than the out, then keep adding more to the in...
		while (cr.GetInRarity() <= cr.GetOutRarity()){
			cr.inputStacks [0].stackSize += 1;
		}

		return cr;

	}

}
