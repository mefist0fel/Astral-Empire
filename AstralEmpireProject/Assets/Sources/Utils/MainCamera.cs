using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))] 

public class MainCamera : MonoBehaviour {
	
	public static Camera mainCamera;
	public static Vector2 size;
	
	int currentWidth = 0;
	int currentHeight = 0;
	
	public static void UpdateAspect() {
		if (mainCamera != null) {
			size = new Vector2(Screen.width, Screen.height) * (mainCamera.orthographicSize / Screen.height);
		}
	}

	void Awake () {
		mainCamera = this.GetComponent<Camera>();
		UpdateAspect ();
	}

	void Update() {
		if (currentWidth != Screen.width || currentHeight != Screen.height) {
			currentWidth = Screen.width;
			currentHeight = Screen.height;
			UpdateAspect();
		}
	}
}
