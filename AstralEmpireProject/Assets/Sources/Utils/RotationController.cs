using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

// Orbital camera rotator. Move its angles by mouse and keyboard. For demonstration scenes.
public class RotationController : MonoBehaviour
{
	Vector2 startMousePosition;
	bool isMouseDown = false;
	[SerializeField]int mouseButtonNumber = 0;
	[SerializeField]float rotationSpeed = 0.4f;

	void Update () {
		bool isHoverOnInterface = false;
		if (EventSystem.current != null)
		{
			isHoverOnInterface = EventSystem.current.IsPointerOverGameObject();
		}
		if (Input.GetMouseButtonDown (mouseButtonNumber) && !isHoverOnInterface) {
			isMouseDown = true;
			startMousePosition = Input.mousePosition;
		}
		if (Input.GetMouseButtonUp (mouseButtonNumber)) {
			isMouseDown = false;
		}
		if (isMouseDown) {
			var pos = Input.mousePosition;
			transform.rotation = Quaternion.Euler((startMousePosition.y - pos.y) * rotationSpeed, (startMousePosition.x - pos.x) * - rotationSpeed, 0) * transform.rotation;
			startMousePosition = pos;
		}
	}
}
