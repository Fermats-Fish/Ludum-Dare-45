using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleList2D<T> where T : class {

	// Have a double list of double lists.
	DoubleList<DoubleList<T>> listOfLists;

	public DoubleList2D (){
		// Create a double list which contains columns of double lists, and where the default value of an item in this list is an empty double list.
		listOfLists = new DoubleList<DoubleList<T>> ( () => {return new DoubleList<T>(); } );
	}

	// Set the item at a specific position.
	public void SetAt (int x, int y, T value){

		// Extend to include the desired coordinates (will only do so if required).
		ExtendTo (x, y);

		// Set the block at the coordinates.
		listOfLists.GetAt (x).SetAt (y, value);

	}

	public void SetAt(Coord c, T value){
		SetAt (c.x, c.y, value);
	}

	// Set the item at a specific position.
	public T GetAt (int x, int y){
		return listOfLists.GetAt (x).GetAt (y);
	}

	public T GetAt (Coord c){
		return GetAt (c.x, c.y);
	}

	// Returns the minimum x coordinate that the map currently reaches to.
	public int GetMinX(){
		return listOfLists.GetMinIndex ();
	}

	// Returns the maximum x coordinate that the map currently reaches to.
	public int GetMaxX(){
		return listOfLists.GetMaxIndex ();
	}

	// Returns the minimum y coordinate that the map currently reaches to.
	public int GetMinY(){
		return listOfLists.GetAt (0).GetMinIndex ();
	}

	// Returns the maximum y coordinate that the map currently reaches to.
	public int GetMaxY(){
		return listOfLists.GetAt (0).GetMaxIndex ();
	}

	// Returns the minimum x coordinate of nonempty items in the map.
	public int GetMinOccX(){

		// Loop down columns until we get to a non-empty location.
		for (int col = GetMinX(); col <= GetMaxX(); col++) {
			for (int row = GetMinY(); row <= GetMaxY(); row++){
				if (!(GetAt (col, row) == default(T))) {
					return col;
				}
			}
		}

		return 0;

	}

	// Returns the maximum x coordinate of nonempty items in the map.
	public int GetMaxOccX(){

		// Loop down columns until we get to a non-empty location.
		for (int col = GetMaxX(); col >= GetMinX(); col--) {
			for (int row = GetMinY(); row <= GetMaxY(); row++){
				if (!(GetAt (col, row) == default(T))) {
					return col;
				}
			}
		}

		return 0;

	}

	// Returns the minimum y coordinate of nonempty items in the map.
	public int GetMinOccY(){

		// Loop down columns until we get to a non-empty location.
		for (int row = GetMinY(); row <= GetMaxY(); row++){
			for (int col = GetMinX(); col <= GetMaxX(); col++) {
				if (!(GetAt (col, row) == default(T))) {
					return row;
				}
			}
		}

		return 0;

	}

	// Returns the maximum y coordinate of nonempty items in the map.
	public int GetMaxOccY(){

		// Loop down columns until we get to a non-empty location.
		for (int row = GetMaxY(); row >= GetMinY(); row--){
			for (int col = GetMinX(); col <= GetMaxX(); col++) {
				if (!(GetAt (col, row) == default(T))) {
					return row;
				}
			}
		}

		return 0;

	}

	// Extend the tile map so that it includes the specified coordinate.
	// Should only be run internally if wanting to place a block at the specified coord.
	private void ExtendTo(int x, int y){

		// Add rows as needed.
		if (y > GetMinY() || y < GetMinY()){
			for (int i = GetMinX(); i <= GetMaxX(); i++){
				listOfLists.GetAt (i).ExtendTo (y);
			}
		}

		// Add columns as needed.
		if (x < GetMinX () || x > GetMaxX ()) {

			// startX is the index of the first new column to be added.
			int startX = 0;

			// endX is one more than the index of the last new colum to be added.
			int endX = 0;

			// Get current min and max y.
			int curMinY = GetMinY ();
			int curMaxY = GetMaxY ();

			// Figure out the range of new columns which will be added.
			if (x < GetMinX ()) {
				startX = x;
				endX = GetMinX ();
			} else if (x > GetMaxX ()) {
				startX = GetMaxX () + 1;
				endX = x + 1;
			}

			// Add extra columns as needed. Note that these extra columns will contain a different number of rows to the rest of the map which is bad.
			listOfLists.ExtendTo (x);

			// Make these new columns have the same range as the other columns.
			for (int i = startX; i < endX; i++) {
				listOfLists.GetAt (i).ExtendTo (curMinY);
				listOfLists.GetAt (i).ExtendTo (curMaxY);
			}

		}

	}

	public List<T> TrimToRange(Range r){
		return TrimToRange (r.minX, r.maxX, r.minY, r.maxY);
	}

	// Trims the items in the list to be in a specific range, and returns anything deleted from this list.
	public List<T> TrimToRange(int minX, int maxX, int minY, int maxY){

		// First get the current occurance x bounds of the map.
		int curMinX = GetMinOccX ();
		int curMaxX = GetMaxOccX ();
		int curMinY = GetMinOccY ();
		int curMaxY = GetMaxOccY ();

		// Initialise the list of things being removed.
		List<T> removedItems = new List<T> ();

		// Start by removing columns of items.
		while (curMinX < minX){

			// Add the nonempty contents in the curMinX column, and remove them from the list.
			for (int y = curMinY; y <= curMaxY; y++) {
				T item = GetAt (curMinX, y);
				if (!(item == default(T))) {
					removedItems.Add (item);
					SetAt (curMinX, y, default(T));
				}
			}

			// Increase the curMinX value by 1.
			curMinX += 1;

		}
		while (curMaxX > maxX){

			// Add the nonempty contents in the curMaxX column, and remove them from the list.
			for (int y = curMinY; y <= curMaxY; y++) {
				T item = GetAt (curMaxX, y);
				if (!(item == default(T))) {
					removedItems.Add (item);
					SetAt (curMaxX, y, default(T));
				}
			}

			// Decrease the curMaxX value by 1.
			curMaxX -= 1;

		}
		// Now do the same as above but for rows of items.
		while (curMinY < minY){

			// Add the nonempty contents in the curMinY row, and remove them from the list.
			for (int x = minX; x <= maxX; x++) {
				T item = GetAt (x, curMinY);
				if (!(item == default(T))) {
					removedItems.Add (item);
					SetAt (x, curMinY, default(T));
				}
			}

			// Increase the curMinY value by 1.
			curMinY += 1;

		}
		while (curMaxY > maxY){

			// Add the nonempty contents in the curMaxY row, and remove them from the list.
			for (int x = minX; x <= maxX; x++) {
				T item = GetAt (x, curMaxY);
				if (!(item == default(T))) {
					removedItems.Add (item);
					SetAt (x, curMaxY, default(T));
				}
			}

			// Decrease the curMaxY value by 1.
			curMaxY -= 1;

		}

		return removedItems;
	}

}


public class Coord {

	public int x;
	public int y;

	public Coord(int x, int y){
		this.x = x;
		this.y = y;
	}

	public Vector3 ToVector3(){
		return new Vector3 ((float) this.x, (float) this.y, 0f);
	}

	public override string ToString ()
	{
		return "(" + x.ToString() + ", " + y.ToString() + ")";
	}

	public static bool operator == (Coord c1, Coord c2){
		if (Object.ReferenceEquals(c1, null) || Object.ReferenceEquals(c2, null)){
			if (Object.ReferenceEquals(c1, null) && Object.ReferenceEquals(c2, null)){
				return true;
			}
			return false;
		}
		return c1.x == c2.x && c1.y == c2.y;
	}

	public static bool operator != (Coord c1, Coord c2){
		return !(c1 == c2);
	}

	public override bool Equals(object c2){
		if ( c2.GetType() != typeof(Coord) ){
			return false;
		} else {
			return this == c2 as Coord;
		}
	}

	public override int GetHashCode(){
		return x * 0x0010000 + y;
	}

}


public class Range {

	public int minX;
	public int maxX;
	public int minY;
	public int maxY;

	public Range( int minX, int maxX, int minY, int maxY ){
		this.minX = minX;
		this.maxX = maxX;
		this.minY = minY;
		this.maxY = maxY;
	}

}


public class DoubleList<T> {

	protected List<T> negatives = new List<T> ();
	protected List<T> nonNegatives = new List<T> ();

	// The function which generates a new empty value to be placed in the map.
	public delegate T GenerateNewEmpty();
	GenerateNewEmpty generateNewEmpty = () => {return default(T); };

	public DoubleList () {
		ExtendTo(0);
	}

	public DoubleList (GenerateNewEmpty generator){
		generateNewEmpty = generator;
		ExtendTo (0);
	}

	public T GetAt(int i){
		ExtendTo (i);
		if (i >= 0){
			return nonNegatives [i];
		} else {
			return negatives [-i - 1];
		}
	}

	public void SetAt(int i, T value){
		ExtendTo (i);
		if (i >= 0){
			nonNegatives [i] = value;
		} else {
			negatives [-i - 1] = value;
		}
	}

	public void ExtendTo(int i){

		while (i > GetMaxIndex()){
			nonNegatives.Add (generateNewEmpty());
		}

		while (i < GetMinIndex()){
			negatives.Add (generateNewEmpty());
		}

	}

	public int GetMaxIndex(){
		return nonNegatives.Count - 1;
	}

	public int GetMinIndex(){
		return -negatives.Count;
	}

}