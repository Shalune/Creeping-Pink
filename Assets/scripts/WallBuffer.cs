using UnityEngine;
using System.Collections;

public class WallBuffer : MonoBehaviour {

	public Transform parentWall;
	public Transform wall;

	void Awake () {
		BoxCollider thisBox = GetComponent<BoxCollider> ();

		Vector3 newSize = thisBox.size;
		if (wall.lossyScale.x > wall.lossyScale.y) {
			newSize.x = 0.2f / wall.lossyScale.x + 1;
		} else {
			newSize.y = 0.2f / wall.lossyScale.y + 1;
		}
		thisBox.size = newSize;

	}
}
