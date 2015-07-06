using UnityEngine;
using System.Collections;

public class ShootProjectile : MonoBehaviour {

	public Transform spawnPosition;
	public GameObject projectilePrefab;
	public float force = 10f;

	void Update () {
		if(Input.GetMouseButtonDown(0)) {
			Vector3 sp = Camera.main.WorldToScreenPoint(spawnPosition.position);
			Vector3 dir = (Input.mousePosition - sp).normalized;

			GameObject projectile = GameObject.Instantiate(projectilePrefab, spawnPosition.position, Quaternion.identity) as GameObject;
			projectile.GetComponent<Rigidbody2D>().velocity += (Vector2)dir*force;

		}
	}
}
