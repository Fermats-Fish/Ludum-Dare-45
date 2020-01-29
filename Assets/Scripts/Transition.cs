using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition {

	Vector3 startPos;
	Vector3 endPos;
	GameObject go;
	bool destroyOnFinish;

	int turnsToComplete;
	int turnsFinished = 0;

	public Transition(GameObject go, Vector3 startPos, Vector3 endPos, int turnsToComplete){
		this.go = go;
		this.startPos = startPos;
		this.endPos = endPos;
		this.turnsToComplete = turnsToComplete;
		destroyOnFinish = false;
	}

	public Transition(GameObject go, Vector3 endPos, int turnsToComplete){
		this.go = go;
		this.startPos = go.transform.position;
		this.endPos = endPos;
		this.turnsToComplete = turnsToComplete;
		destroyOnFinish = false;
	}

	public Transition(Sprite sprite, Quaternion angle, Vector3 startPos, Vector3 endPos, int turnsToComplete)
		: this (GameObject.Instantiate (GC.inst.terrainPrefab, startPos, angle), startPos, endPos, turnsToComplete){
		go.GetComponent<SpriteRenderer> ().sprite = sprite;
		destroyOnFinish = true;
	}

	public Transition(Sprite sprite, Vector3 startPos, Vector3 endPos, int turnsToComplete)
		: this (sprite, Quaternion.LookRotation (endPos - startPos), startPos, endPos, turnsToComplete){

	}

	public Transition(Sprite sprite, Color color, Quaternion angle, Vector3 startPos, Vector3 endPos, int turnsToComplete)
		: this (GameObject.Instantiate (GC.inst.terrainPrefab, startPos, angle), startPos, endPos, turnsToComplete){
		go.GetComponent<SpriteRenderer> ().sprite = sprite;
		go.GetComponent<SpriteRenderer> ().color = color;
		destroyOnFinish = true;
	}

	public Transition(Sprite sprite, Color color, Vector3 startPos, Vector3 endPos, int turnsToComplete)
		: this(sprite, color, Quaternion.identity, startPos, endPos, turnsToComplete){

	}

	/// <summary>
	/// Sets the percentage that the animation should have progressed by.
	/// </summary>
	/// <returns><c>true</c>, if the animation finished, <c>false</c> otherwise.</returns>
	/// <param name="p">P.</param>
	public bool SetPercent(float p){

		if (go == null){
			return true;
		}

		go.transform.position = Vector3.Lerp (startPos, endPos, turnsFinished / (float) turnsToComplete + p / (float) turnsToComplete);

		if (p >= 1f){
			turnsFinished += 1;
			if (turnsFinished >= turnsToComplete) {
				if (destroyOnFinish) {
					GameObject.Destroy (go);
				}
				return true;
			}
		}

		return false;

	}

}
