using UnityEngine;
using System.Collections;

public class PlayerCollision : MonoBehaviour {

	public GameManager gameManager;
	public Player player;

	private int numDirections = 4;
	private GameObject[] blockedDirections;

	void Start(){
		blockedDirections = new GameObject[numDirections];
		for (int i = 0; i < numDirections; i++) {
			blockedDirections [i] = null;
		}
	}

	private void CheckWallSideHit(Collider other){
		int layerCache = other.gameObject.layer;
		other.gameObject.layer = GameManager.hitLayer;

		Vector3 difference = other.transform.position - transform.position;
		RaycastHit ray = new RaycastHit ();
		Physics.Raycast (transform.position, difference, out ray, 10f, GameManager.hitLayerMask);

		if (ray.normal.normalized == Vector3.down) {
			// bottom
			blockedDirections[(int)GameManager.directions.up] = other.gameObject;
			player.ToggleBlocking (GameManager.directions.up);

		} else if (ray.normal.normalized == Vector3.up) {
			// top
			blockedDirections[(int)GameManager.directions.down] = other.gameObject;
			player.ToggleBlocking (GameManager.directions.down);

		} else if (ray.normal.normalized == Vector3.left) {
			// left
			blockedDirections[(int)GameManager.directions.right] = other.gameObject;
			player.ToggleBlocking (GameManager.directions.right);

		} else if (ray.normal.normalized == Vector3.right) {
			// right
			blockedDirections[(int)GameManager.directions.left] = other.gameObject;
			player.ToggleBlocking (GameManager.directions.left);

		} else {
			Debug.Log ("Error: attempting to find side of wall that player hit returned no match.");
		}

		other.gameObject.layer = layerCache;

		/*
		float xOffset = other.transform.lossyScale.x /2f;
		float yOffset = other.transform.lossyScale.y /2f;

		Debug.Log ("hitpoint = " + ray.point + " and offsets = " + xOffset + ", " + yOffset);



		if (System.Math.Round(ray.point.y, 1) == System.Math.Round(other.transform.position.y - yOffset, 1)) {
			// bottom
			blockedDirections[(int)GameManager.directions.up] = other.gameObject;
			player.ToggleBlocking (GameManager.directions.up);

		} else if (System.Math.Round(ray.point.y, 1) == System.Math.Round(other.transform.position.y + yOffset, 1)) {
			// top
			blockedDirections[(int)GameManager.directions.down] = other.gameObject;
			player.ToggleBlocking (GameManager.directions.down);

		} else if (System.Math.Round(ray.point.x, 1) == System.Math.Round(other.transform.position.x - xOffset, 1)) {
			// left
			blockedDirections[(int)GameManager.directions.right] = other.gameObject;
			player.ToggleBlocking (GameManager.directions.right);

		} else if (System.Math.Round(ray.point.x, 1) == System.Math.Round(other.transform.position.x + xOffset, 1)) {
			// right
			blockedDirections[(int)GameManager.directions.left] = other.gameObject;
			player.ToggleBlocking (GameManager.directions.left);

		} else {
			Debug.Log ("Error: attempting to find side of wall that player hit returned no match.");
		}
		*/
	}

	private void LeavingWall(Collider other){

		for (int i = 0; i < numDirections; i++) {
			if (blockedDirections [i] == other.gameObject) {
				player.ToggleBlocking ((GameManager.directions)i);
				blockedDirections [i] = null;
			}
		}
	}

	void OnTriggerEnter(Collider other){
		if(other.gameObject.CompareTag("Pink")){
			Debug.Log ("player died");
			//gameManager.PlayerDied ();
		}

		if (other.gameObject.CompareTag ("Wall Buffer")) {
			CheckWallSideHit (other);
		}
	}

	void OnTriggerExit(Collider other){
		if (other.gameObject.CompareTag ("Wall Buffer")) {
			LeavingWall (other);
		}
	}
}
