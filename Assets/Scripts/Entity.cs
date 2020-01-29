using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity {

	public int health = 100;
	public int maxHealth = 100;

	public bool dead = false;

	public Tile t;

	public Entity(Tile t){
		this.t = t;
	}

	public virtual void TakeDamage(int amount){

		if (dead){
			Debug.LogError ("Trying to attack a dead entity");
		}

		health -= amount;
		if (health <= 0){
			health = 0;
			OnDeath ();
		}
		if (health >= maxHealth){
			health = maxHealth;
		}

		GC.inst.NotifyEntityHealthChange (this);
	}

	public virtual Sprite GetSprite(){
		return null;
	}

	public virtual Color GetSpriteColor(){
		return Color.white;
	}

	public virtual void OnDeath(){
		t.entity = null;
		dead = true;
	}

	public int SquareDistance(Entity other){
		return (other.t.x - t.x) * (other.t.x - t.x) + (other.t.y - t.y) * (other.t.y - t.y);
	}

}
