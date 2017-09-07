using UnityEngine;
using System.Collections;

public class TroubleShootingJitters : MonoBehaviour {

	public Transform cube;
	public Transform camera;

	private float tolerance = 0.2f;


	void Update () {
		if (xCoordsOff () || yCoordsOff ()) {
			Debug.Log ("player jumping detected");
			Debug.Log ("base player coord = " + transform.position);
			Debug.Log ("player cube coord = " + cube.position);
			Debug.Log ("main camera coord = " + camera.position);
		}
	}

	private bool xCoordsOff(){
		if (transform.position.x != cube.position.x || transform.position.x != camera.position.x || cube.position.x != camera.position.x) {
			return true;
		}
		return false;
	}

	private bool yCoordsOff(){
		if (transform.position.y != cube.position.y || transform.position.y != camera.position.y || cube.position.y != camera.position.y) {
			return true;
		}
		return false;
	}
}
