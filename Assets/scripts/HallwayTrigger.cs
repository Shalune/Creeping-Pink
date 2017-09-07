using UnityEngine;
using System.Collections;

public class HallwayTrigger : MonoBehaviour {

	public Hallway hall;
	public GameObject pairedTrigger;
	public GameObject oppositeTrigger;

	public Directions2D._directions pairDirection;
	public Directions2D._directions hallDirection;

	void Awake(){
		pairDirection = Directions2D.FindDirection (gameObject.transform, pairedTrigger.transform);
		hallDirection = Directions2D.FindDirection (gameObject.transform, oppositeTrigger.transform);
	}
}