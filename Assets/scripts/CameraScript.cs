using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

	public GameManager gameManager;

	private Vector3 posOffset = Vector3.zero;
	private Quaternion rotOffset = Quaternion.identity;

	private bool creepyMode = false;
	private Vector3 savedPos;
	private Quaternion savedRot;

	private Vector3 creepyPosOffset;
	private float xyMaxOffset = 1f;
	private float zMaxOffset = 2f;
	private float creepyLerpSpeed = 0.1f;
	private Quaternion creepyRotOffset;

	private BasicTimer creepyShakeWhenTimer;
	private float creepyShakeAfter = 4f;
	private BasicTimer creepyShakePeriodTimer;
	private float creepyShakePeriodInterval = 0.2f;
	private BasicTimer creepyShakeTimer;
	private float creepyShakeInterval;
	private float creepyShakeDistanceMin = 0.05f;
	private float creepyShakeDistanceMax = 0.1f;
	private bool shakeOn = false;
	private float shakeSpeed;
	private float shakeSpeedVariance;
	private Vector3 shakeOffset = Vector3.zero;

	void Awake(){
		posOffset.z = transform.position.z - transform.parent.position.z;


		creepyShakeInterval = creepyShakePeriodInterval / 3;

		creepyShakeWhenTimer = BasicTimer.SetupTimer (gameObject, creepyShakeAfter, false, 0f, true);
		creepyShakePeriodTimer = BasicTimer.SetupTimer (gameObject, creepyShakePeriodInterval);
		creepyShakeTimer = BasicTimer.SetupTimer (gameObject, creepyShakeInterval);

		shakeSpeed = creepyShakeInterval / 2;
		shakeSpeedVariance = shakeSpeed;
	}

	void Update () {
		if (creepyMode) {
			CreepyEffects ();
		}
		CheckCreepy ();
	}

	private void CreepyEffects(){
		switch (GameManager.creepyMode) {
		case GameManager._creepyMode.buildUp:
			CreepyShift ();
			break;
		case GameManager._creepyMode.creepy:
			CreepyShift ();
			CreepyShake ();
			break;
		}
	}

	private void CreepyShift(){
		posOffset = Vector3.Lerp (posOffset, creepyPosOffset, Time.deltaTime * creepyLerpSpeed);
		rotOffset = Quaternion.Lerp (transform.rotation, creepyRotOffset, Time.deltaTime * creepyLerpSpeed);
	}

	private void CreepyShake(){

		bool startShake = false;
		creepyShakeWhenTimer.Advance (Time.deltaTime, out startShake);
		if (startShake) {
			shakeOn = true;
		}

		if (shakeOn) {
			bool endShake = false;
			creepyShakePeriodTimer.Advance (Time.deltaTime, out endShake);
			if (endShake) {
				shakeOn = false;
				posOffset -= shakeOffset;
				shakeOffset = Vector3.zero;
				return;
			}

			bool adjustShake = false;
			creepyShakeTimer.Advance (Time.deltaTime, out adjustShake);
			if (adjustShake) {
				// reset individual shake
				posOffset -= shakeOffset;
				float x = posOffset.x + Random.Range(creepyShakeDistanceMin, creepyShakeDistanceMax) * Mathf.Pow(-1f, (int)Random.Range(0, 5.99f));
				float y = posOffset.y + Random.Range(creepyShakeDistanceMin, creepyShakeDistanceMax) * Mathf.Pow(-1f, (int)Random.Range(0, 5.99f));
				shakeOffset = new Vector3 (x, y, 0);
				Debug.Log ("shakeOFfset generated " + shakeOffset + "x,y " + x + " " + y);
			}

			// shake move
			posOffset = Vector3.Lerp (posOffset, shakeOffset, Time.deltaTime * shakeSpeed);
		}
	}

	private void CheckCreepy(){
		
		if (creepyMode && GameManager.creepyMode == GameManager._creepyMode.none) {
			creepyMode = false;
			creepyShakeWhenTimer.Reset ();
			creepyShakeTimer.Reset ();
			creepyShakePeriodTimer.Reset ();

		} else if (!creepyMode && (int)GameManager.creepyMode > 0) {
			creepyMode = true;
			GenerateCreepyOffsets ();
			savedPos = posOffset;
			savedRot = rotOffset;
		}
	}

	private void GenerateCreepyOffsets(){
		float x = Random.Range (-xyMaxOffset, xyMaxOffset);
		float y = Random.Range (-xyMaxOffset, xyMaxOffset);
		float z = Random.Range (zMaxOffset, zMaxOffset*0.5f);
		creepyPosOffset = new Vector3 (x, y, z) + posOffset;

		int sign = (int)Mathf.Pow (-1, (int)Random.Range (0, 10));
		float randomAngle = Random.Range (5, 7) * sign;
		creepyRotOffset = Quaternion.AngleAxis(randomAngle, Vector3.forward);
	}

	public Vector3 GetPosOffset() { return posOffset; }
	public Quaternion GetRotOffset() { return rotOffset; }
}
