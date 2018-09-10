using System;
using UnityEngine;

public class CameraController : MonoBehaviour {
    private static CameraController instance = null;

    private const float moveSpeed = 0.4f; // camera move - unit per second

    [SerializeField]
    private float minDistance = 3f;
    [SerializeField]
    private float maxDistance = 30f;
    [SerializeField]
    private Rect borders = new Rect(new Vector2(0, 0), new Vector2(50, 50));

    [SerializeField]
    private Vector3 cameraShift = new Vector3(0, 0, -12f); // camera vertical/horisontal shift and distance from pivot
    [SerializeField]
    private Vector3 needCameraShift = new Vector3(0, 0, -12f); // need camera vertical/horisontal shift and distance from pivot
    [SerializeField]
    private Vector3 needPosition = Vector3.zero;

    private Quaternion needRotation = Quaternion.Euler(45, 0, 0);
    [SerializeField]
    private Camera controlCamera = null;

    Transform selfTransform = null;

    public static Camera ControlCamera {
        get { return (instance != null) ? instance.controlCamera : null; }
    }

    public static Vector3 Shift {
        get { return (instance != null) ? instance.cameraShift : Vector3.zero; }
    }

    public static Vector3 Position {
        get { return (instance != null) ? instance.needPosition : Vector3.zero; }
    }

    public static void SetBorders(Rect newBorders) {
        if (instance != null) {
            instance.borders = newBorders;
        }
    }

    public static void SetDistanceBorders(float minDistance, float maxDistance) {
        if (instance != null) {
            instance.minDistance = minDistance;
            instance.maxDistance = maxDistance;
        }
    }

    private void Awake() {
        if (instance != null) {
            Destroy(gameObject);
            Debug.Log("Two camera controllers in scene. Removing");
            return;
        }
        instance = this;
        selfTransform = transform;
        if (controlCamera == null) {
            controlCamera = GetComponentInChildren<Camera>();
        }
        DontDestroyOnLoad(gameObject);
    }

    public static void SetPosition(Vector3 position, bool useAnimation = false) {
        if (instance != null) {
            instance.needPosition = position;
            if (!useAnimation) {
                instance.selfTransform.localPosition = position;
            }
        }
    }

    public static void SetAngles(Vector3 angles, bool useAnimation = false) {
        if (instance != null) {
            instance.needRotation = Quaternion.Euler(angles);
            if (!useAnimation) {
                instance.selfTransform.localEulerAngles = angles;
            }
        }
    }

    public static void SetShift(Vector3 shift, bool useAnimation = false) {
        if (instance != null) {
            instance.needCameraShift = shift;
            if (!useAnimation) {
                instance.cameraShift = shift;
            }
        }
    }

    public static void Move(Vector3 moveVector) {
        float cameraAngle = instance.transform.eulerAngles.y;
        instance.needPosition += Quaternion.Euler(0, cameraAngle, 0) * moveVector * moveSpeed;
    }

    public static void MoveRaycast(Vector3 prevScreenPosition, Vector3 currentScreenPosition) {
        Vector3 prevMapPosition = instance.Raycast(prevScreenPosition);
        Vector3 currentMapPosition = instance.Raycast(currentScreenPosition);
        instance.needPosition += prevMapPosition - currentMapPosition;
    }

    public static void Zoom(float scale = 1f) {
        instance.needCameraShift.z = instance.needCameraShift.z * scale;
        instance.needCameraShift.z = -Mathf.Max(-instance.needCameraShift.z, instance.minDistance);
        instance.needCameraShift.z = -Mathf.Min(-instance.needCameraShift.z, instance.maxDistance);
    }

    public static void ZoomIn(float speed = 20f) {
        instance.needCameraShift.z -= instance.needCameraShift.z * speed * Time.deltaTime;
        instance.needCameraShift.z = -Mathf.Max(-instance.needCameraShift.z, instance.minDistance);
    }

    public static void ZoomOut(float speed = 20f) {
        instance.needCameraShift.z += instance.needCameraShift.z * speed * Time.deltaTime;
        instance.needCameraShift.z = -Mathf.Min(-instance.needCameraShift.z, instance.maxDistance);
    }

    public Vector3 Raycast(Vector2 screenPoint) {
        if (ControlCamera == null)
            return Vector3.zero;
        var ray = ControlCamera.ScreenPointToRay(screenPoint);
        Plane floorPlane = new Plane(Vector3.up, Vector3.zero);
        float distance = 0;
        if (floorPlane.Raycast(ray, out distance)) {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    private void Update() {
        if (controlCamera != null) {
            cameraShift = Utils.Lerp(cameraShift, needCameraShift, (0.15f * 30f) * Time.deltaTime);
            controlCamera.transform.localPosition = cameraShift;
        }
        CheckBorders();
        selfTransform.localPosition = Utils.Lerp(selfTransform.localPosition, needPosition, (0.15f * 50f) * Time.deltaTime);
        selfTransform.localRotation = Quaternion.Lerp(selfTransform.localRotation, needRotation, (0.15f * 50f) * Time.deltaTime);
    }

    private void CheckBorders() {
        if (needPosition.x > borders.xMax)
            needPosition.x = borders.xMax;
        if (needPosition.z > borders.yMax)
            needPosition.z = borders.yMax;
        if (needPosition.x < borders.xMin)
            needPosition.x = borders.xMin;
        if (needPosition.z < borders.yMin)
            needPosition.z = borders.yMin;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos() {
        // Camera move borders
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(borders.xMax, 0, borders.yMax), new Vector3(borders.xMax, 0, borders.yMin));
        Gizmos.DrawLine(new Vector3(borders.xMax, 0, borders.yMin), new Vector3(borders.xMin, 0, borders.yMin));
        Gizmos.DrawLine(new Vector3(borders.xMin, 0, borders.yMin), new Vector3(borders.xMin, 0, borders.yMax));
        Gizmos.DrawLine(new Vector3(borders.xMin, 0, borders.yMax), new Vector3(borders.xMax, 0, borders.yMax));
    }
#endif
}
