using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public sealed class BoxMeshFilter: MonoBehaviour {
	[System.Serializable]
	public class Settings {
        [SerializeField]
        public Vector3 Size = Vector3.one;

        public override int GetHashCode () {
			return Size.GetHashCode();
		}
	}
	[SerializeField]
    private Settings settings = new Settings();
	[SerializeField]
    private Vector3 shift = Vector3.zero;

	[SerializeField]
    private MeshFilter meshFilter = null;

	private Mesh controlMesh = null;

    private void FindComponents() {
		if (meshFilter == null)
			meshFilter = GetComponent<MeshFilter> ();
    }

    private void Start() {
        FindComponents();
        RegenerateMesh();
    }

    [ContextMenu("Regenerate mesh")]
    private void RegenerateMesh () {
		controlMesh = GenerateMesh ();
		meshFilter.mesh = controlMesh;
	}

    private Mesh GenerateMesh () {
		return GenerateBoxMesh (settings, shift);
	}

    public static Mesh GenerateBoxMesh(Settings settings, Vector3 shift) {
        var mesh = new MeshGenerator("Box");
        var size = settings.Size * 0.5f;
        mesh.AddQuad(
            new Vector3(-size.x, +size.y, +size.z) + shift,
            new Vector3(+size.x, +size.y, +size.z) + shift,
            new Vector3(+size.x, +size.y, -size.z) + shift,
            new Vector3(-size.x, +size.y, -size.z) + shift,
            Vector3.up);
        mesh.AddQuad(
            new Vector3(+size.x, -size.y, +size.z) + shift,
            new Vector3(-size.x, -size.y, +size.z) + shift,
            new Vector3(-size.x, -size.y, -size.z) + shift,
            new Vector3(+size.x, -size.y, -size.z) + shift,
            Vector3.down);
        mesh.AddQuad(
            new Vector3(+size.x, +size.y, -size.z) + shift,
            new Vector3(+size.x, +size.y, +size.z) + shift,
            new Vector3(+size.x, -size.y, +size.z) + shift,
            new Vector3(+size.x, -size.y, -size.z) + shift,
            Vector3.right);
        mesh.AddQuad(
            new Vector3(-size.x, +size.y, +size.z) + shift,
            new Vector3(-size.x, +size.y, -size.z) + shift,
            new Vector3(-size.x, -size.y, -size.z) + shift,
            new Vector3(-size.x, -size.y, +size.z) + shift,
            Vector3.down);
        mesh.AddQuad(
            new Vector3(+size.x, -size.y, +size.z) + shift,
            new Vector3(+size.x, +size.y, +size.z) + shift,
            new Vector3(-size.x, +size.y, +size.z) + shift,
            new Vector3(-size.x, -size.y, +size.z) + shift,
            Vector3.forward);
        mesh.AddQuad(
            new Vector3(+size.x, +size.y, -size.z) + shift,
            new Vector3(+size.x, -size.y, -size.z) + shift,
            new Vector3(-size.x, -size.y, -size.z) + shift,
            new Vector3(-size.x, +size.y, -size.z) + shift,
            Vector3.back);
        return mesh.Generate();
	}

	#if UNITY_EDITOR
	private int settingsHash = 0;
	private int shiftHash = 0;
    private void OnDrawGizmos() {
		FindComponents ();
		if (settingsHash != settings.GetHashCode ()) {
			settingsHash = settings.GetHashCode ();
			RegenerateMesh ();
		}
		if (shiftHash != shift.GetHashCode ()) {
			shiftHash = shift.GetHashCode ();
			RegenerateMesh ();
		}
	}
	#endif
}
