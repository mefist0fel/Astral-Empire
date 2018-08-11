using UnityEngine;
using System.Collections;

public class Timer : MonoBehaviour {

	static Timer instance = null;

	public delegate void ResultAction ();

	public delegate void AnimateAction (float t);

	float[] timers = new float[10];
	float[] animationTime = new float[10];
	AnimateAction[] animateAction = new AnimateAction[10];
	ResultAction[]  endAction     = new ResultAction[10];

	public static int Add(float time, ResultAction resultAction) {
		return Add(0, time, null, resultAction);
	}

	public static int Add(float fullTime, AnimateAction animateAction, ResultAction resultAction = null) {
		return Add(0, fullTime, animateAction, resultAction);
	}

	public static int Add(float startTime, float fullTime, AnimateAction animateAction, ResultAction resultAction = null) {
		if (instance == null) {
			Timer timer = GameObject.FindObjectOfType<Timer>();
			if (timer != null) {
				instance = timer;
			} else {
				GameObject timerObject = new GameObject("GlobalTimer");
				DontDestroyOnLoad(timerObject);
				instance = timerObject.AddComponent<Timer>();
			}
		}
		return instance.AddTimer(startTime, fullTime, animateAction, resultAction);
	}

	public static void Stop(int timerId) {
		if (instance != null) {
			if (timerId >= 0 && timerId < instance.timers.Length && instance.timers[timerId] > 0) {
				instance.timers[timerId] 		 = 0;
				instance.animateAction[timerId] = null;
				instance.endAction[timerId]     = null;
			}
		}
	}

	int AddTimer (float startTime, float actionTime, AnimateAction animAction, ResultAction resultAction) {
		for (int i = 0; i < timers.Length; i++) {
			if (timers[i] <= 0 && endAction[i] == null && animateAction[i] == null) {
				timers[i] 		 = actionTime - startTime;
				animationTime[i] = actionTime;
				animateAction[i] = animAction;
				endAction[i]     = resultAction;
				return i;
			}
		}
		// need more timers - add to Arrays
		int tempNum = timers.Length;
		// increase arr size
		float[] newTimers = new float[tempNum + 5];
		float[] newAnimationTime = new float[tempNum + 5];
		AnimateAction[] newAnimateAction = new AnimateAction[tempNum + 5];
		ResultAction[]  newEndAction     = new ResultAction[tempNum + 5];
		// replace old values in new Array
		for (int i = 0; i < timers.Length; i++) {
			newTimers[i]        = timers[i];
			newAnimationTime[i] = animationTime[i];
			newAnimateAction[i] = animateAction[i];
			newEndAction[i]     = endAction[i];
		}
		timers        = newTimers;
		animationTime = newAnimationTime;
		animateAction = newAnimateAction;
		endAction     = newEndAction;
		// Add new timer
		timers[tempNum] 	   = actionTime - startTime;
		animationTime[tempNum] = actionTime;
		animateAction[tempNum] = animAction;
		endAction[tempNum]     = resultAction;
		return tempNum;
	}

	void Update () {
		for (int i = 0; i < timers.Length; i++) {
			if (timers[i] > 0) {
                timers[i] -= Time.deltaTime;
                if (timers[i] <= 0) {
					timers[i] = 0;
				}
				if (timers[i] <= animationTime[i]) {
					if (animateAction[i] != null) {
						animateAction[i](1f - (timers[i] / animationTime[i]));
					}
				}
				if (timers[i] <= 0) {
					if (endAction[i] != null) {
						endAction[i]();
					}
					endAction[i] = null;
					animateAction[i] = null;
				}
			}
		}
	}
}
