using UnityEngine;
using System.Collections;

public class Directions2D : MonoBehaviour {

	public enum _directions {up, down, right, left, numDirections};

	public static Directions2D._directions FindDirection(Transform dirFrom, Transform dirTo){
		float x = dirTo.position.x - dirFrom.position.x;
		float y = dirTo.position.y - dirFrom.position.y;

		if (Mathf.Abs (x) == 0 && Mathf.Abs (y) == 0) {
			Debug.Log ("Directions2D.FindDirection was passed 2 transforms at the same location");
			return Directions2D._directions.up;

		} else if (Mathf.Abs (x) > Mathf.Abs (y)) {
			if (x > 0) {
				return Directions2D._directions.right;
			} else {
				return Directions2D._directions.left;
			}
		} else {
			if (y > 0) {
				return Directions2D._directions.up;
			} else {
				return Directions2D._directions.down;
			}
		}
	}

	public static Vector3 DirectionUnitVector(Directions2D._directions direction, bool positive = false){
		switch (direction) {
		case _directions.up:
			return Vector3.up;
		case _directions.down:
			if (positive)
				return Vector3.up;
			return Vector3.down;
		case _directions.right:
			return Vector3.right;
		case _directions.left:
			if (positive)
				return Vector3.right;
			return Vector3.left;
		}
		return Vector3.zero;
	}

	/// <summary>
	/// Get value from vector for dimension corresponding to direction.
	/// </summary>
	/// <returns>Component of vector: y if direction up/down, x if right/left, else 0.</returns>
	/// <param name="direction">Direction.</param>
	/// <param name="rawVector">Raw vector.</param>
	public static float FlattenToDirection(Directions2D._directions direction, Vector3 rawVector){
		if (direction == Directions2D._directions.numDirections) {
			Debug.Log ("Warning: Directions2D.FlattenToDirection was passed \"numDirections\" as reference direction.");
			return 0f;
		}
		if ((int)direction < (int)Directions2D._directions.numDirections / 2) {
			return rawVector.y;
		} else {
			return rawVector.x;
		}
	}

	//ODOT make private?
	public static void MatchSign(Directions2D._directions direction, ref float input){
		input = Mathf.Abs (input);
		if ((int)direction % 2 == 1) {
			input *= -1;
		}
	}

	public static Vector3 DirectionalVector(Directions2D._directions direction, Vector3 rawVector){

		Vector3 newVector = rawVector;
		if (direction == Directions2D._directions.numDirections) {
			Debug.Log ("Warning: Directions2D.DirectionalVector was passed \"numDirections\" as reference direction.");
			newVector = Vector3.zero;
		}
			
		if ((int)direction < (int)Directions2D._directions.numDirections / 2) {
			newVector.x = 0f;
			Directions2D.MatchSign (direction, ref newVector.y);
		} else {
			newVector.y = 0f;
			Directions2D.MatchSign (direction, ref newVector.x);
		}

		return newVector;
	}

	public static Directions2D._directions OppositeAxis(Directions2D._directions initialDirection){
		if (initialDirection == Directions2D._directions.numDirections) {
			Debug.Log("Warning: Directions2D.OppositeAxis was passed \"numDirections\" as a direction.");
			return Directions2D._directions.numDirections;
		}
		return (Directions2D._directions)((int)(initialDirection + 2) % (int)Directions2D._directions.numDirections);
	}

	public static Directions2D._directions OppositeDirection(Directions2D._directions initialDirection){
		if (initialDirection == Directions2D._directions.up || initialDirection == Directions2D._directions.right) {
			return (Directions2D._directions)((int)initialDirection + 1);
		} else if(initialDirection == Directions2D._directions.down || initialDirection == Directions2D._directions.left){
			return (Directions2D._directions)((int)initialDirection - 1);
		} else {
			Debug.Log("Warning: Directions2D.OppositeDirection was passed \"numDirections\" as a direction.");
			return Directions2D._directions.numDirections;
		}
	}

	public static float DistancePerp (Vector3 point0, Vector3 point1, Directions2D._directions direction){
		
		if (direction == Directions2D._directions.numDirections) {
			Debug.Log ("Warning: Directions2D.DistancePerp was passed \"numDirections\" as reference direction.");
			return 0f;
		}

		Vector3 distance = point0 - point1;

		if ((int)direction < (int)Directions2D._directions.numDirections / 2) {
			return Mathf.Abs (distance.x);
		} else {
			return Mathf.Abs (distance.y);
		}
	}

	public static float DistancePara (Vector3 point0, Vector3 point1, Directions2D._directions direction){

		if (direction == Directions2D._directions.numDirections) {
			Debug.Log ("Warning: Directions2D.DistancePerp was passed \"numDirections\" as reference direction.");
			return 0f;
		}

		Vector3 distance = point0 - point1;

		if ((int)direction < (int)Directions2D._directions.numDirections / 2) {
			return Mathf.Abs (distance.y);
		} else {
			return Mathf.Abs (distance.x);
		}
	}
}
