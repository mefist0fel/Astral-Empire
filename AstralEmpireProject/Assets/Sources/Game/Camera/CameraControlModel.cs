using UnityEngine;
using System;
using UnityEngine.EventSystems;

public sealed class CameraControlModel {

    private bool isTouchStationar = false;
    private Vector3 prevMouseMovePosition = Vector3.zero;

    private Action<Vector2> onPointerClick = null;
    private bool isPointerOverInterface {
        get {
            if (EventSystem.current == null)
                return false;
            return EventSystem.current.IsPointerOverGameObject();
        }
    }

    public CameraControlModel(Action<Vector2> onSelectionClickAction = null) {
        onPointerClick = onSelectionClickAction;
    }

    public void Process() {
        ProcessKeyboardMove();
        ControlCameraZoom();
        // move by mouse // touch
        if (Input.touchSupported) {
            ProcessTouchMove();
        } else {
            ProcessMouseMove();
        }
    }

    private void ProcessTouchMove() {
        if (Input.touchCount == 1) {
            if (Input.touches[0].phase == TouchPhase.Began && !isPointerOverInterface) {
                isTouchStationar = true;
            }
            if (Input.touches[0].phase == TouchPhase.Moved) {
                var position = Input.touches[0].position;
                var prevPosition = position - Input.touches[0].deltaPosition;
                CameraController.MoveRaycast(prevPosition, position);
                isTouchStationar = false;
            }
            if (Input.touches[0].phase == TouchPhase.Ended && isTouchStationar) {
                PointerSelect(Input.touches[0].position);
            }
        }
        if (Input.touchCount == 2) {
            float prevDistance = ((Input.touches[0].position - Input.touches[0].deltaPosition) - (Input.touches[1].position - Input.touches[1].deltaPosition)).magnitude;
            float currentDistance = (Input.touches[0].position - Input.touches[1].position).magnitude;
            CameraController.Zoom(prevDistance / currentDistance);
        }
    }

    private void PointerSelect(Vector2 position) {
        if (onPointerClick == null)
            return;
        onPointerClick(position);
    }

    private void ProcessMouseMove() {
        const int LeftMouseButton = 0;
        if (Input.GetMouseButtonDown(LeftMouseButton) && !isPointerOverInterface) {
            PointerSelect(Input.mousePosition);
        }
        const int RightMouseButton = 1;
        if (Input.GetMouseButtonDown(RightMouseButton) || isPointerOverInterface) {
            prevMouseMovePosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(RightMouseButton) && !isPointerOverInterface) {
            CameraController.MoveRaycast(prevMouseMovePosition, Input.mousePosition);
            prevMouseMovePosition = Input.mousePosition;
        }
    }

    private void ProcessKeyboardMove() {
        const float moveSpeed = 16f;
        Vector3 moveVector = Vector3.zero;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
            moveVector += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            moveVector += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            moveVector += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            moveVector += Vector3.right;
        }
        CameraController.Move(moveVector * (moveSpeed * Time.deltaTime));
    }

    private void ControlCameraZoom() { // Scroll - zooming
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll < 0) {
            CameraController.ZoomOut();
        }
        if (scroll > 0) {
            CameraController.ZoomIn();
        }
    }
}
