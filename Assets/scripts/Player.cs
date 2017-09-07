using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

	public float moveSpeed;
	public float turnSpeed;
	public GameManager gameManager;
	public GameObject playerCube;

	private Vector3 centerScreen;
	private GameObject camera;
	private List <GameManager.directions> blockedDirections;

	void Start () {
		centerScreen = new Vector3 (Screen.width / 2f, Screen.height / 2f, 0);
		camera = GameObject.Find ("Main Camera");
		blockedDirections = new List <GameManager.directions> ();
	}

	void Update () {
		FacingMouse ();
		Movement ();
	}

	private void FacingMouse(){

		Quaternion targetQuat = Quaternion.FromToRotation (Vector3.up, Input.mousePosition - centerScreen);
		Quaternion targetRot = Quaternion.Lerp(transform.rotation, targetQuat, turnSpeed*Time.deltaTime);
		transform.rotation = targetRot;
		camera.transform.position = transform.position + camera.GetComponent<CameraScript>().GetPosOffset();
		camera.transform.rotation = camera.GetComponent<CameraScript>().GetRotOffset();

		/*
		Quaternion targetQuat = Quaternion.FromToRotation (Vector3.up, Input.mousePosition - centerScreen);
		transform.rotation = Quaternion.RotateTowards (transform.rotation, targetQuat, turnSpeed*Time.deltaTime);
		camera.transform.rotation = Quaternion.identity;
		*/
	}

	private void Movement(){
		Vector3 velocity = new Vector3 (Input.GetAxis ("Horizontal") * moveSpeed * Time.deltaTime, Input.GetAxis ("Vertical") * moveSpeed * Time.deltaTime, 0);

		if (blockedDirections.Contains (GameManager.directions.up) && velocity.y > 0)
			velocity.y *= 0;
		if (blockedDirections.Contains (GameManager.directions.down) && velocity.y < 0)
			velocity.y *= 0;
		if (blockedDirections.Contains (GameManager.directions.right) && velocity.x > 0)
			velocity.x *= 0;
		if (blockedDirections.Contains (GameManager.directions.left) && velocity.x < 0)
			velocity.x *= 0;

		transform.position += velocity;
	}

	public void ToggleBlocking(GameManager.directions toBlock){
		
		if (blockedDirections.Contains (toBlock)) {
			blockedDirections.Remove (toBlock);

		} else {
			blockedDirections.Add (toBlock);
		}
	}
}
