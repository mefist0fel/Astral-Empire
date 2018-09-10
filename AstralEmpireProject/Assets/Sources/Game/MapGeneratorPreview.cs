using System;
using UnityEngine;

public class MapGeneratorPreview : MonoBehaviour {
    public enum NoiseType {
        PerlinNoise,
        QuadDistance
    }
    [SerializeField]
    private int width = 51;
    [SerializeField]
    private int height = 51;
    [SerializeField]
    private Vector2 size = Vector2.one;
    [SerializeField]
    private Vector3 mapShift = Vector3.zero;
   // [SerializeField]
   // private int pointsInCell = 3;
    [SerializeField]
    private bool hideNegativeValues = false;
    [SerializeField]
    private NoiseParams[] noiseParams = new NoiseParams[] {
        new NoiseParams()
    };

    [Serializable]
    public sealed class NoiseParams {
        public NoiseType Type = NoiseType.PerlinNoise;
        public bool Enabled = true;
        public float Scale = 1;
        public float Offset = 0;
        public float Height = 1;
        public Vector2 Shift = Vector2.zero;

        public float GetNoise(float x, float y) {
            if (!Enabled)
                return 0;
            switch (Type) {
                default:
                case NoiseType.PerlinNoise:
                    return Mathf.PerlinNoise(x / Scale + Shift.x, y / Scale + Shift.y) * Height + Offset;
                case NoiseType.QuadDistance:
                    var normalizedDistance = (new Vector2(x, y) + Shift).magnitude / Scale;
                    return normalizedDistance * normalizedDistance * normalizedDistance * normalizedDistance * Height + Offset;
            }
        }
    }

    public Vector3 CellCoordToPosition(float x, float y) {
        return new Vector3(x * size.x + y * size.x * 0.5f, 0, y * size.y) + mapShift;
    }

    private void Start () {
		
	}

    private void Update () {

    }

    private float CalculateNoise(NoiseParams[] noiseParams, Vector3 position) {
        if (noiseParams == null)
            return 0;
        var height = 0f;
        foreach (var param in noiseParams) {
            if (param == null)
                continue;
            height += param.GetNoise(position.x, position.z);
        }
        return height;
    }

    private void OnDrawGizmos() {
        Vector3 position;
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (i + j - 1 <= width * 0.5f || i + j + 1 >= width + height - width * 0.5f - 2) {
                    continue;
                }
                position = CellCoordToPosition(i, j);
                var noise = CalculateNoise(noiseParams, position);
                if (hideNegativeValues && noise < 0)
                    continue;
                position.y = noise;
                Gizmos.DrawWireCube(position, Vector3.one * 0.1f);
            }
        }
    }
}
