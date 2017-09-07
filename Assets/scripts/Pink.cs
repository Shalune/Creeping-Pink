using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * USAGE WARNINGS
 * - all parent objects must have scale of 1 or pink scaling will break
 * 
 * 
 * 
 * 
 * Tyler Main - 2016
 */


public class Pink : MonoBehaviour {

	public bool DEBUGHALL = false;

	public GameObject[] pinkTemplate;
	public GameManager gameManager;
	public Transform pinkStorage;

	public Vector3 startingScale;
	public int maxBranches = 2;
	public int maxExtraBranches = 4;
	public float maxScaling = 3f;
	public float growSpeed;
	public float branchZOffset;

	private BoxCollider pinkCollider;
	private GameObject[] branches;
	private bool atMaxSize = false;
	private bool canGrow = true;
	private bool inLightCollider = false;
	private bool currentlyLit = false;
	private int timesBeenMaxSize = 0;
	private int maxTimesToGrow = 2;
	private int normalBranches = 0;
	private int branchCount = 0;
	private float branchAt = 0.65f;
	private float branchAtVariance = 0.1f;
	private float biasToGrowTowardsPlayer = 0.7f;
	private float adjustBranchBias = 0.2f;
	private float overshootTolerance = 1.05f;
	private float wallHitDownsizeBy = 0.95f;
	private Directions2D._directions growDir;

	private bool crowded = false;
	private int touchingPinks = 0;
	private int stopWhenTouching = 4;
	private GameObject[] touchingOtherPinks;

	private bool inCreepyRange = false;
	private float creepyRange = 3.1f;
	private float creepyGrowTimer = 0f;
	private float creepyGrowAdded = 0f;
	private const float creepyGrowInterval = 7f;
	private const float creepyGrowCompensatedDuration = 8f;

	// hallway vars
	// enum hallwayMode - off, connecting, traversing, ending
	public enum _hallwayMode {off, connecting, traversing, ending};
	public _hallwayMode hallwayMode;
	private List<Collider> hallTriggersHit;
	private int hallStartTriggersHit = 0;

	void Awake() {

		maxScaling = 3f;

		pinkCollider = GetComponent<BoxCollider> ();
		transform.localScale = startingScale;
		growDir = Directions2D._directions.down;
		branches = new GameObject[maxBranches + maxExtraBranches];
		for (int i = 0; i < maxBranches + maxExtraBranches; i++) {
			branches [i] = null;
		}

		branchAt += Random.Range (-branchAtVariance, branchAtVariance);

		touchingOtherPinks = new GameObject[stopWhenTouching];
		for (int i = 0; i < stopWhenTouching; i++) {
			touchingOtherPinks[i] = null;
		}

		pinkStorage = GameObject.Find ("pink storage").transform;
		transform.SetParent (pinkStorage);

		hallwayMode = _hallwayMode.off;
		hallTriggersHit = new List<Collider> ();

		GameManager.numGrowths++;
	}

	void Update () {

		if (DEBUGHALL) {
			//Debug.Log ("max = " + maxScaling + "   mode = " + hallwayMode);
		}

		if (!currentlyLit && inLightCollider && IsInLight()) {
			currentlyLit = true;
		}

		if (currentlyLit || atMaxSize || crowded) {
			canGrow = false;
		} else {
			canGrow = true;
		}

		if (canGrow || creepyGrowAdded > 0) {
			Grow (Time.deltaTime);
		} else if (atMaxSize && timesBeenMaxSize < maxTimesToGrow) {
			//CheckSecondGrowth ();
		} else if (currentlyLit) {
			CreepyGrowth ();
		}
	}

	private bool IsInLight(){
		Vector3 leadingEdge;

		if ((float)growDir < (float)((int)Directions2D._directions.numDirections / 2)) {
			//vertical
			leadingEdge = transform.position + (Vector3.up * Mathf.Pow(-1, (float)growDir) * transform.lossyScale.y/2f);

		} else {
			// horizontal
			leadingEdge = transform.position + (Vector3.right * Mathf.Pow(-1, (float)growDir) * transform.lossyScale.x/2f);
		}

		Vector3 difference = GameManager.player.position - leadingEdge;
		RaycastHit ray = new RaycastHit();

		if (Physics.Raycast (leadingEdge, difference, out ray, Vector3.Magnitude (difference), GameManager.wallLayerMask)) {
			return false;
		}
		return true;
	}

	private void CreepyGrowth(){
		float distance = Vector3.Magnitude (pinkCollider.ClosestPointOnBounds (GameManager.player.position) - GameManager.player.position);

		if (!inCreepyRange && distance <= creepyRange) {
			gameManager.ToggleCreepyOn (this);
			inCreepyRange = true;
		} else if (inCreepyRange) {
			creepyGrowTimer += Time.deltaTime;
		
			if (creepyGrowTimer >= creepyGrowInterval && !crowded) {
				Grow (creepyGrowCompensatedDuration);
				creepyGrowTimer = 0f;
			}

			if(distance > creepyRange){
				TurnOffCreepy ();
			}
		}
	}

	private void TurnOffCreepy(){
		gameManager.ToggleCreepyOff (this);
		inCreepyRange = false;
		creepyGrowTimer = 0f;
	}

	private void Grow(float growDuration) {

		growDuration += creepyGrowAdded;

		Vector3 newScale;
		Vector3 newPos;

		if ((float)growDir < (float)((int)Directions2D._directions.numDirections/2)) {

			newScale = transform.localScale + (growSpeed * growDuration) * Vector3.up;
			newPos = FindGrowthPosition (newScale, true);


			float newScaleVsBranch = newScale.y - (maxScaling * branchAt);

			// check if half way branch
			if (newScaleVsBranch >= 0 && normalBranches < maxBranches/2f) {
				Vector3 branchPoint = newPos + Vector3.up * (Mathf.Pow(-1f, (float)growDir) * (maxScaling * branchAt - startingScale.y)/ 2f);
				Vector3 zOffset = Vector3.forward * Mathf.Pow (-1f, (int)Random.Range (0f, 1.99f)) * branchZOffset;
				Branch (branchPoint  + zOffset, newScaleVsBranch);
			}

			float newScaleVsMax = newScale.y - maxScaling;
			if (newScaleVsMax >= 0) {
				newScale.y = maxScaling;
				newPos = FindGrowthPosition (newScale, true);
				Vector3 branchPoint = newPos + Vector3.up * ((maxScaling - startingScale.y) / 2f) * Mathf.Pow (-1f, (float)growDir);
				Vector3 zOffset = Vector3.forward * Mathf.Pow(-1f, (int)Random.Range(0f, 1.99f)) * branchZOffset;
				Branch (branchPoint + zOffset, newScaleVsMax);
				atMaxSize = true;
				timesBeenMaxSize++;
			}


		} else {		// X AXIS GROWTH


			newScale = transform.localScale + (growSpeed * growDuration) * Vector3.right;
			newPos = FindGrowthPosition (newScale, false);

			// TEMP - old method - newScale.x >= maxScaling * branchAt
			float newScaleVsBranch = newScale.x - (maxScaling * branchAt);

			// check if half way branch
			if (newScaleVsBranch >= 0 && normalBranches < maxBranches/2f) {
				Vector3 branchPoint = newPos + Vector3.right * (((maxScaling * branchAt - startingScale.x)/ 2f) * Mathf.Pow (-1f, (float)growDir));
				Vector3 zOffset = Vector3.forward * Mathf.Pow (-1f, (int)Random.Range (0f, 1.99f)) * branchZOffset;
				Branch (branchPoint + zOffset, newScaleVsBranch);
			}


			// TEMP - old method - newScale.x >= maxScaling
			float newScaleVsMax = newScale.x - maxScaling;

			if (newScaleVsMax >= 0) {
				newScale.x = maxScaling;
				newPos = FindGrowthPosition (newScale, false);
				Vector3 branchPoint = newPos + Vector3.right * ((maxScaling - startingScale.x)/ 2f) * Mathf.Pow (-1f, (float)growDir);
				Vector3 zOffset = Vector3.forward * Mathf.Pow (-1f, (int)Random.Range (0f, 1.99f)) * branchZOffset;
				Branch (branchPoint + zOffset, newScaleVsMax);
				atMaxSize = true;
				timesBeenMaxSize++;
			}
		}

		transform.position = newPos;
		transform.localScale = newScale;

		creepyGrowAdded = 0f;
	}

	private Vector3 FrontBranchPoint(){
		Vector3 endPoint = transform.position + Directions2D.DirectionalVector (growDir, transform.lossyScale) / 2;
		//endPoint -= Directions2D.FlattenToDirection (growDir, startingScale) / 2;
		// ODOT, make positive
		endPoint -= Directions2D.DirectionalVector(growDir, startingScale) / 2;
		return endPoint;
	}

	private Pink Branch(Vector3 startPoint, float overshotBy = 0f, bool extraBranch = false, Directions2D._directions direction = Directions2D._directions.numDirections, _hallwayMode hall = _hallwayMode.off){
		if (normalBranches == maxBranches) {
			Debug.Log ("ERROR: attmepted to branch from pink object (" + this.name + ") when already at max branches");
			return null;
		}

		branches [branchCount] = Instantiate (pinkTemplate[0], startPoint, Quaternion.identity) as GameObject;


		Pink newBranchPink = branches [branchCount].GetComponentInChildren<Pink> ();

		if (direction == Directions2D._directions.numDirections) {
			Directions2D._directions newDir = NewBranchDirection (startPoint);
			newBranchPink.SetDirection (newDir);
		} else {
			newBranchPink.SetDirection (direction);
		}

		if (overshotBy >= 0.05f) {
			newBranchPink.AddGrowthBurst (overshotBy);
		}
		newBranchPink.hallwayMode = hall;
			
		branchCount++;
		if (!extraBranch) {
			normalBranches++;
		}

		newBranchPink.DEBUGHALL = false;

		return newBranchPink;
	}

	private void BranchBloom(Vector3 startPoint, float overshotBy = 0f, bool extraBranches = false){
		Directions2D._directions newDir = NewBranchDirection (startPoint);
		Branch (startPoint, overshotBy, extraBranches, newDir);
		Branch (startPoint, overshotBy, extraBranches, Directions2D.OppositeDirection(newDir));

		Debug.Log ("Bloom called");
	}

	private Directions2D._directions NewBranchDirection(Vector3 startPoint){

		bool grewTowardsPlayer = false;

		if ((float)growDir < (float)((int)Directions2D._directions.numDirections / 2)) {
			
			// current growth is vertical, choose horizontal
			if (GameManager.player.position.x < startPoint.x) {
				if (Random.Range (0f, 1f) < biasToGrowTowardsPlayer) {
					grewTowardsPlayer = true;
					return Directions2D._directions.left;
				} else {
					return Directions2D._directions.right;
				}
			} else {
				if (Random.Range (0f, 1f) < biasToGrowTowardsPlayer) {
					grewTowardsPlayer = true;
					return Directions2D._directions.right;

				} else {
					return Directions2D._directions.left;
				}
			}


		} else {
			// current growth is horizontal, choose vertical
			if (GameManager.player.position.y < startPoint.y) {
				if (Random.Range (0f, 1f) < biasToGrowTowardsPlayer) {
					grewTowardsPlayer = true;
					return Directions2D._directions.down;

				} else {
					return Directions2D._directions.up;
				}
			} else {
				if (Random.Range (0f, 1f) < biasToGrowTowardsPlayer) {
					grewTowardsPlayer = true;
					return Directions2D._directions.up;

				} else {
					return Directions2D._directions.down;
				}
			}
		}

		biasToGrowTowardsPlayer += adjustBranchBias * (grewTowardsPlayer ? -1 : 1);
	}

	private Vector3 FindGrowthPosition(Vector3 newScale, bool yAxis){
		if (yAxis) {
			return transform.position + Vector3.up * ((newScale.y - transform.localScale.y) / 2f) * Mathf.Pow (-1f, (float)growDir);
		} else {
			return transform.position + Vector3.right * ((newScale.x - transform.localScale.x) / 2f) * Mathf.Pow (-1f, (float)growDir);
		}
	}

	private void CheckSecondGrowth(){

		// should make permanent variable instead of checking each time
		for (int i = 0; i < branches.Length; i++) {
			if (!branches [i].GetComponent<Pink> ().IsAtMaxSize()) {
				return;
			}
		}

		atMaxSize = false;
		maxScaling *= 1.618f;

		//TEMP
		//branchCount++;
	}

	private void CheckTouchingPink(Collider other){
		bool alreadyCounted = false;
		int firstOpenSlot = -1;

		for (int i = 0; i < maxBranches; i++) {
			if (branches [i] == other.gameObject) {
				alreadyCounted = true;
			}
		}

		for (int i = 0; i < stopWhenTouching; i++) {
			if (touchingOtherPinks [i] == null) {
				firstOpenSlot = i;
				break;
			}
			if (touchingOtherPinks [i] == other.gameObject) {
				alreadyCounted = true;
			}
		}

		if (!alreadyCounted) {
			touchingOtherPinks [firstOpenSlot] = other.gameObject;
			if (firstOpenSlot == stopWhenTouching - 1 && hallwayMode != _hallwayMode.traversing) {
				crowded = true;
			}
		}
	}

	private void CheckOvershotWall(Collider other){
		float shrinkAbsolute = 0f;

		if ((float)growDir < (float)((int)Directions2D._directions.numDirections / 2)) {
			// growing on Y axis

			float distance = Mathf.Abs(transform.position.y - other.gameObject.transform.position.y);
			float targetDistance = Mathf.Abs(distance - (other.gameObject.transform.lossyScale.y * 0.5f)) * overshootTolerance;

			if (targetDistance < transform.lossyScale.y * 0.5f) {
				// ODOT - apply to X axis below
				// FIX - assumes that centre will remain in the same place
				maxScaling = (transform.lossyScale.y * 0.5f) + (targetDistance * wallHitDownsizeBy);
				float shrinkRatio = maxScaling / transform.lossyScale.y;
				shrinkAbsolute = transform.lossyScale.y - maxScaling;


				//adjust branches
				float[] branchesOffset = new float[maxBranches];

				for (int i = 0; i < maxBranches; i++) {
					if (branches [i] != null) {
						branchesOffset[i] = Mathf.Abs (branches [i].transform.position.y - transform.position.y);
					}
				}

				// apply changes to this
				transform.position += Vector3.down * shrinkAbsolute * 0.5f * Mathf.Pow(-1, (int)growDir);
				transform.localScale += Vector3.down * shrinkAbsolute;

				for (int i = 0; i < maxBranches; i++) {
					if (branches [i] != null) {
						branches[i].transform.position = transform.position + (Vector3.up * Mathf.Pow(-1, (int)growDir) * branchesOffset[i]);
					}
				}
			}

		} else {
			// growing on X axis


			float distance = Mathf.Abs(transform.position.x - other.gameObject.transform.position.x);
			float targetDistance = Mathf.Abs(distance - (other.gameObject.transform.lossyScale.x * 0.5f)) * overshootTolerance;

			if (targetDistance < transform.lossyScale.x * 0.5f) {
				Debug.Log("distance = " + distance + "  targetDistance = " + targetDistance);
				maxScaling = (transform.lossyScale.x * 0.5f) + (targetDistance * wallHitDownsizeBy);
				float shrinkRatio = maxScaling / transform.lossyScale.x;
				shrinkAbsolute = transform.lossyScale.x - maxScaling;


				//adjust branches
				float[] branchesOffset = new float[maxBranches];

				for (int i = 0; i < maxBranches; i++) {
					if (branches [i] != null) {
						branchesOffset[i] = Mathf.Abs (branches [i].transform.position.x - transform.position.x);
					}
				}

				// apply changes to this
				transform.position += Vector3.left * shrinkAbsolute * 0.5f * Mathf.Pow(-1, (int)growDir);
				transform.localScale += Vector3.left * shrinkAbsolute;

				for (int i = 0; i < maxBranches; i++) {
					if (branches [i] != null) {
						branches[i].transform.position = transform.position + (Vector3.right * Mathf.Pow(-1, (int)growDir) * branchesOffset[i]);
					}
				}
			}
		}

		if (shrinkAbsolute > 0) {
			for (int i = 0; i < maxBranches; i++) {
				if (branches [i] != null) {
					branches [i].GetComponent<Pink> ().AddGrowthBurst (shrinkAbsolute);
				}
			}
		}
	}

	private void HallwayTriggers(Collider other){
		if (DEBUGHALL) {
			//Debug.Log ("HallwayTriggers called for trigger = " + other.gameObject.tag + "   object = " + other.gameObject.name);
		}

		if (other.gameObject.CompareTag ("Hall Start Trigger") && !other.GetComponent<HallwayTrigger> ().hall.pinkHasTraversed) {

			if (DEBUGHALL) {
				Debug.Log ("hit hall start trigger");
			}

			HallwayTrigger hallTrig = other.GetComponent<HallwayTrigger> ();

			if (hallwayMode == _hallwayMode.off) {

				if (DEBUGHALL) {
					Debug.Log ("hallway mode was off when hit");
				}

				if (growDir == hallTrig.hallDirection) {
					// confirmed pink is growing in same direction as the length of the hallway


					float otherPos = Directions2D.FlattenToDirection (Directions2D.OppositeAxis(growDir), other.transform.position);
					float pairPos = Directions2D.FlattenToDirection (Directions2D.OppositeAxis (growDir), hallTrig.pairedTrigger.transform.position);
					float thisPos = Directions2D.FlattenToDirection (Directions2D.OppositeAxis (growDir), transform.position);
					float otherScale = Directions2D.FlattenToDirection (Directions2D.OppositeAxis (growDir), other.transform.lossyScale);
					float thisScale = Directions2D.FlattenToDirection (Directions2D.OppositeAxis (growDir), transform.lossyScale);

					float triggerDistance = Mathf.Abs(pairPos - otherPos);
					float midpoint = triggerDistance / 2 + otherPos;
					float allowance =  thisScale - (triggerDistance - otherScale);

					if (DEBUGHALL) {
						Debug.Log ("growdir same as hallway, triggerDistance = " + triggerDistance + "   midpoint = " + midpoint + "   allowance = " + allowance);
					}

					if (thisPos > midpoint - allowance && thisPos < midpoint + allowance) {
						// confirmed pink is centered enough that it has hit both hall triggers
						// it will clear the hallway walls and only needs to be extended
						if (DEBUGHALL) {
							Debug.Log ("start traversing");
						}
						float hallLength = Directions2D.FlattenToDirection(growDir, other.transform.position - hallTrig.oppositeTrigger.transform.position);
						maxScaling += Mathf.Abs(hallLength);
						hallwayMode = _hallwayMode.traversing;
						hallTrig.hall.pinkHasTraversed = true;

					} else {
						// pink is not centered enough and needs to branch to align
						// create connecting branch
						Pink newBranch = Branch(FrontBranchPoint(), 0f, true, hallTrig.pairDirection, _hallwayMode.connecting);
					}

				}
				else if (growDir == hallTrig.pairDirection) {
					// confirmed pink is growing in direction of paired hallway trigger
					float connectionLength = Directions2D.FlattenToDirection(growDir, other.transform.position - hallTrig.pairedTrigger.transform.position);
					maxScaling += Mathf.Abs (connectionLength);
					hallwayMode = _hallwayMode.connecting;
				}

			} else if (hallwayMode == _hallwayMode.connecting && hallStartTriggersHit % 2 == 1) {
				// create traversing branch
				Pink newBranch = Branch(FrontBranchPoint(), 0f, true, hallTrig.hallDirection, _hallwayMode.traversing);
			
				float hallLength = Directions2D.FlattenToDirection(hallTrig.hallDirection, other.transform.position - hallTrig.oppositeTrigger.transform.position);
				newBranch.maxScaling += Mathf.Abs(hallLength);

				hallwayMode = _hallwayMode.off;
				hallTrig.hall.pinkHasTraversed = true;
			}

			hallStartTriggersHit++;

		} else if (other.gameObject.CompareTag ("Hall End Trigger") && !hallTriggersHit.Contains(other)) {
			// hit end point

			if (hallwayMode == _hallwayMode.traversing && hallTriggersHit.Count > 0) {
				// branch both directions
				if (DEBUGHALL) {
					Debug.Log ("hit hall end trigger while traversing");
				}

				BranchBloom (FrontBranchPoint (), 0f, true);
				hallwayMode = _hallwayMode.off;
			}
			hallTriggersHit.Add (other);
		}
	}

	public void TriggerEnter(Collider other){
		if (other.gameObject.CompareTag ("Torch Collider")) {
			inLightCollider = true;
		} else if (other.gameObject.CompareTag ("Wall")) {
			crowded = true;
			CheckOvershotWall (other);
		}

		if (other.gameObject.CompareTag ("Pink")) {
			if (!crowded) {
				CheckTouchingPink (other);
			}
		}
			
		HallwayTriggers (other);
	}

	void OnTriggerEnter(Collider other){
		// ODOT - move above method back here?
		TriggerEnter (other);
	}

	void OnTriggerExit(Collider other){
		if (other.gameObject.CompareTag ("Torch Collider")) {
			currentlyLit = false;
			inLightCollider = false;
			TurnOffCreepy ();
		}
	}

	public void SetDirection(Directions2D._directions newDir){
		growDir = newDir;
	}

	public void AddGrowthBurst(float amount){
		creepyGrowAdded += amount;
	}

	public bool IsAtMaxSize(){
		return atMaxSize;
	}
}
