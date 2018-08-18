using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public sealed class TubeMeshFilter : MonoBehaviour {
	[System.Serializable]
	public class Settings {
        [SerializeField]
        public int edgeCount = 6;
        [SerializeField]
        public float outerRadius = 1f;
        [SerializeField]
        public float innerRadius = 0.5f;
        [SerializeField]
        public float startAngle = 0;
        [SerializeField]
        public float depth = 1;
        [SerializeField]
        public bool smoothNormals = false;

        public override int GetHashCode () {
			return startAngle.GetHashCode() + edgeCount.GetHashCode() + outerRadius.GetHashCode() + innerRadius.GetHashCode() + depth.GetHashCode() + smoothNormals.GetHashCode();
		}
	}
	[SerializeField] Settings settings = new Settings();
	[SerializeField] Vector3 shift = Vector3.zero;

	[SerializeField] MeshFilter meshFilter = null;

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
		return GeneratePrysmMesh (settings, shift);
	}

    public static Mesh GeneratePrysmMesh(Settings settings, Vector3 shift) {
        var mesh = new MeshGenerator("Prysm");
        int segments = settings.edgeCount;
        float startAngle = settings.startAngle / 180f * Mathf.PI;
        Vector3[] sideNormals = new Vector3[segments + 1];
        Vector3[] topOuterPoints = new Vector3[segments + 1];
        Vector3[] topInnerPoints = new Vector3[segments + 1];
        Vector3[] bottomOuterPoints = new Vector3[segments + 1];
        Vector3[] bottomInnerPoints = new Vector3[segments + 1];
        for (int i = 0; i < topOuterPoints.Length; i++) {
            float angleInRad = 2f * Mathf.PI / (float)segments * (float)i + startAngle;
            float angleOfNormalInRad;
            if (settings.smoothNormals) {
                angleOfNormalInRad = 2f * Mathf.PI / (float)segments * (float)(i) + startAngle;
            } else {
                angleOfNormalInRad = 2f * Mathf.PI / (float)segments * (float)(i + 0.5f) + startAngle;
            }
            topOuterPoints[i] = new Vector3(Mathf.Sin(angleInRad), 0, Mathf.Cos(angleInRad)) * settings.outerRadius + shift;
            topInnerPoints[i] = new Vector3(Mathf.Sin(angleInRad), 0, Mathf.Cos(angleInRad)) * settings.innerRadius + shift;
            bottomOuterPoints[i] = topOuterPoints[i] + new Vector3(0, -settings.depth);
            bottomInnerPoints[i] = topInnerPoints[i] + new Vector3(0, -settings.depth);
            sideNormals[i] = new Vector3(Mathf.Sin(angleOfNormalInRad), 0, Mathf.Cos(angleOfNormalInRad));
        }
        for (int i = 0; i < segments; i++) {
            mesh.AddQuad(topInnerPoints[i + 1], topInnerPoints[i], topOuterPoints[i], topOuterPoints[i + 1]);
        }
        if (settings.depth != 0) {
            // Outer sides
            for (int i = 0; i < segments; i++) {
                if (settings.smoothNormals) {
                    mesh.AddQuad(
                        topOuterPoints[i + 1],
                        topOuterPoints[i],
                        bottomOuterPoints[i],
                        bottomOuterPoints[i + 1],
                        sideNormals[i + 1],
                        sideNormals[i],
                        sideNormals[i],
                        sideNormals[i + 1]
                    );
                } else {
                    mesh.AddQuad(
                        topOuterPoints[i + 1],
                        topOuterPoints[i],
                        bottomOuterPoints[i],
                        bottomOuterPoints[i + 1],
                        sideNormals[i]
                    );
                }
            }
            // Inner sides
            for (int i = 0; i < segments; i++) {
                if (settings.smoothNormals) {
                    mesh.AddQuad(
                        topInnerPoints[i],
                        topInnerPoints[i + 1],
                        bottomInnerPoints[i + 1],
                        bottomInnerPoints[i],
                        -sideNormals[i],
                        -sideNormals[i + 1],
                        -sideNormals[i + 1],
                        -sideNormals[i]
                    );
                } else {
                    mesh.AddQuad(
                        topInnerPoints[i],
                        topInnerPoints[i + 1],
                        bottomInnerPoints[i + 1],
                        bottomInnerPoints[i],
                        -sideNormals[i]
                    );
                }
            }
        }
		for (int i = 0; i < segments; i++) {
            mesh.AddQuad(bottomInnerPoints[i], bottomInnerPoints[i + 1], bottomOuterPoints[i + 1], bottomOuterPoints[i]);
        }
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
