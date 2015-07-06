using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public partial class Projectile : MonoBehaviour {

	public bool dead = false;
	public int explosionRadius = 40; // unity units * 100

	public void OnDestroy()
	{

		Vector2 explosionPos = new Vector2(transform.position.x, transform.position.y);
		Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, (float)explosionRadius/100);

		for (int i = 0; i < colliders.Length; i++)
		{
			// TODO: two calls for getcomponent is bad
			if(colliders[i].GetComponent<DestructibleSprite>())
				colliders[i].GetComponent<DestructibleSprite>().ApplyDamage(explosionPos, explosionRadius);
		}
	}

	public void FixedUpdate()
	{
		GetComponent<Rigidbody2D>().AddForce(transform.up*-1);
	}

	void OnTriggerEnter2D (Collider2D other)
	{
		if (dead) {
			return;
		}

		GameObject go = other.gameObject;
		if (go != null && go.layer == LayerMask.NameToLayer("Level"))
		{
			Destroy(gameObject);
		}
	}
}
