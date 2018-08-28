using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGeneratorPreview : MonoBehaviour {
    [SerializeField]
    private int widht = 51;
    [SerializeField]
    private int height = 51;
    [SerializeField]
    private Vector2 size = Vector2.one;
    [SerializeField]
    private Vector3 mapShift = Vector3.zero;
    [SerializeField]
    private int pointsInCell = 3;

    [Serializable]
    public sealed class CellPreset {
    }

    public Vector3 CellCoordToPosition(float x, float y) {
        return new Vector3(x * size.x + y * size.x * 0.5f, 0, y * size.y) + mapShift;
    }

    private void Start () {
		
	}

    private void Update () {
		
	}

    private void OnDrawGizmos() {
        Vector3 position;
        for (int i = 0; i < widht * pointsInCell - 1; i++) {
            for (int j = 0; j < height * pointsInCell - 1; j++) {
                position = CellCoordToPosition(i / (float)pointsInCell, j / (float)pointsInCell);
                var noise = Mathf.PerlinNoise(position.x, position.z);
                position.y = noise;
                Gizmos.DrawWireCube(position, Vector3.one * 0.02f);
            }
        }
    }
}
