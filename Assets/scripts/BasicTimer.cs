﻿using UnityEngine;
using System.Collections;

public class BasicTimer : MonoBehaviour {

	private bool passiveTimer = false;
	private bool paused = false;
	private float counter = 0f;
	private float cycleInterval;
	private int timesCycled = 0;

	void Update () {
		if (passiveTimer) {
			TimerTick (Time.deltaTime);
		}
	}

	private void TimerTick(float advanceTimeBy){
		if (!paused) {
			counter += advanceTimeBy;
			int cycles = (int)Mathf.Floor (counter / cycleInterval);
			if (cycles > 0) {
				counter = 0f;
				timesCycled += cycles;
			}
		}
	}

	public static BasicTimer SetupTimer(GameObject attachTo, float cycleTime, bool setPassive = false, float startTimerAt = 0f, bool startCycled = false){
		if (cycleTime > 0) {
			BasicTimer newTimer = attachTo.AddComponent<BasicTimer> () as BasicTimer;
			newTimer.cycleInterval = cycleTime;
			newTimer.passiveTimer = setPassive;
			newTimer.counter = startTimerAt;
			if (startCycled) {
				newTimer.timesCycled = 1;
			}

			return newTimer;
		} else if (cycleTime == 0) {
			Debug.Log ("Error: invalid input. BasicTimer was passed 0 for cycle time.");
		} else {
			Debug.Log ("Error: invalid input. BasicTimer was passed a negative value for cycle time.");
		}
		return null;
	}

	public void Advance(float advanceTimeBy){
		TimerTick (advanceTimeBy);
	}

	public void Advance(float advanceTimeBy, out bool atLeastOneCycleComplete){
		TimerTick (advanceTimeBy);
		if (timesCycled > 0) {
			atLeastOneCycleComplete = true;
			timesCycled = 0;
		} else {
			atLeastOneCycleComplete = false;
		}
	}

	public void Advance(float advanceTimeBy, out int cyclesComplete){
		TimerTick (advanceTimeBy);
		cyclesComplete = timesCycled;
		timesCycled = 0;
	}

	public void Advance(float advanceTimeBy, out float exactCyclesComplete){
		TimerTick (advanceTimeBy);
		exactCyclesComplete = timesCycled + (counter / cycleInterval);
		timesCycled = 0;
	}

	public bool HasCycled(){
		if (timesCycled > 0) {
			timesCycled = 0;
			return true;
		}
		return false;
	}

	public int NewCyclesCompleted(){
		int result = timesCycled;
		timesCycled = 0;
		return result;
	}

	public float NewCyclesCompletedExact(){
		float result = timesCycled + (counter / cycleInterval);
		timesCycled = 0;
		return result;
	}

	public void Pause(){
		paused = true;
	}

	public void UnPause(){
		paused = false;
	}

	public void TogglePause(){
		paused = !paused;
	}

	public void Reset(){
		counter = 0f;
	}

	public void Set(float setTo){
		counter = setTo;
	}

	public float CurrentCount(){
		return counter;
	}
}
