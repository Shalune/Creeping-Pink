using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public enum directions {up, down, right, left};
	public enum _creepyMode {none, buildUp, creepy, size};

	public static _creepyMode creepyMode = _creepyMode.none;
	public static int numGrowths = 0;
	public static Transform player;
	public static GameObject playerHitBox;
	public static int wallLayerMask;
	public static int wallBufferLayerMask;
	public static int hitLayerMask;
	public static int hitLayer;


	private float creepyTimer = 0f;
	public static float[] creepyThresholds = {0f, 1f, 6.8f};
	private List <Pink> activeCreepyTimers;

	public int maxGrowths;

	void Awake(){

		wallLayerMask = LayerMask.GetMask ("Walls");
		wallBufferLayerMask = LayerMask.GetMask ("Wall Buffers");
		hitLayerMask = LayerMask.GetMask ("Hit Detection Temp");
		hitLayer = gameObject.layer;
		player = GameObject.FindGameObjectWithTag ("Player").transform;
		playerHitBox = GameObject.Find ("player cube");

		activeCreepyTimers = new List <Pink> ();
	}

	void Update(){
		CreepyManagement ();
		CheckCreepyLevels ();
	}

	private void CreepyManagement(){
		if (activeCreepyTimers.Count != 0) {
			creepyTimer += Time.deltaTime;
		} else {
			creepyTimer = 0f;
		}
	}

	private void CheckCreepyLevels(){
		if (creepyMode != _creepyMode.none && creepyTimer == 0) {
			creepyMode = _creepyMode.none;
		} else {
			// check if creepy timer passed threshold to advance mode
			for (int i = (int)creepyMode; i < (int)_creepyMode.size; i++) {
				if (creepyTimer >= creepyThresholds [i]) {
					creepyMode = (_creepyMode)i;
				}
			}
		}
	}

	public void PlayerDied(){
		SceneManager.LoadScene ("test");
	}

	public void ToggleCreepyOff(Pink p){
		if (activeCreepyTimers.Contains (p)) {
			activeCreepyTimers.Remove (p);
			Debug.Log ("removed");
		}
	}

	public void ToggleCreepyOn(Pink p){
		if (!activeCreepyTimers.Contains (p)) {
			activeCreepyTimers.Add (p);
			Debug.Log ("added");
		}
	}
}
