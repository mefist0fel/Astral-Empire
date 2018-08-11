using UnityEngine;

public class LookAtCamera : MonoBehaviour {

	[SerializeField] public Transform targetCameraTransform = null;
	Transform objectTransform;

	void Start () {
		if (targetCameraTransform == null) {
            targetCameraTransform = FindActiveCamera();
        }
        objectTransform = transform;
		Update ();
	}

    private Transform FindActiveCamera() {
        if (Camera.main != null) {
            return Camera.main.transform;
        }
        foreach (var camera in Camera.allCameras) {
            if (camera.isActiveAndEnabled && !camera.orthographic) {
                return camera.transform;
            }
        }
        return null;
    }

    void Update () {
		if (targetCameraTransform != null) {
			objectTransform.rotation = targetCameraTransform.rotation;
		}
	}
}
