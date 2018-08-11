using UnityEngine;

public sealed class StressCamera : MonoBehaviour {
	const float DEFAULT_STRESS_TIME = 1f;
	public static StressCamera stressCamera;
	
	Vector3 position;
	Vector3 stressPosition = Vector3.zero;
	float stressTime = 0;
	
	void Awake () {
		stressCamera = this;
		position = transform.localPosition;
	}
	
	public static void Stress(float time = DEFAULT_STRESS_TIME) {
		if (stressCamera != null) {
			stressCamera.stressTime = time;
		}
	}
	
	void Update () {
		if (stressTime > 0) {
			stressTime -= Time.deltaTime;
			stressPosition = (stressPosition * 0.6f + Random.insideUnitSphere * stressTime * 0.1f);
			if (stressTime <= 0) {
				stressTime = 0;
				stressPosition = Vector3.zero;
			}
			transform.localPosition = position + stressPosition;
		}
	}
}
