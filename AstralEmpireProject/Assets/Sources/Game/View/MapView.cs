﻿using System;
using System.Collections.Generic;
using Model;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class MapView : MonoBehaviour {
    [SerializeField]
    private List<CellPreset> cellObjectPresets = new List<CellPreset>();
    [SerializeField]
    private Vector2 size = Vector2.one;
    [SerializeField]
    private Vector3 mapShift = Vector3.zero;
    private Map map;

    [Serializable]
    public sealed class CellPreset {
        public CellType Type = CellType.Grass;
        public GameObject[] CellObjects;
    }

    public Vector3 CellCoordToPosition(Coord point) {
        return new Vector3(point.x * size.x + point.y * size.x * 0.5f, 0, point.y * size.y) + mapShift;
    }

    public Coord CellPositionToCoord(Vector3 position) {
        position -= mapShift;
        int y = (int)Mathf.Round(position.z / size.y);
        int x = (int)Mathf.Round(position.x / size.x - y * 0.5f);
        return new Coord(x, y);
    }

    public void Init(Map controlMap, MapGenerator generator) {
        map = controlMap;
        mapShift = -CellCoordToPosition(new Coord((generator.Width - 1) * 0.5f, (generator.Height - 1) * 0.5f));
        for (int i = 0; i < generator.Width; i++) {
            for (int j = 0; j < generator.Height; j++) {
                var coord = new Coord(i, j);
                var prefab = GetRandomPrefabForType(generator.Cells[i, j]);
                CreateCellObject(CellCoordToPosition(coord), prefab);
            }
        }
    }

    public Rect GetBorders(Map map) {
        Vector2 min = Vector2.zero;
        Vector2 max = Vector2.zero;
        for (int i = 0; i < map.Width; i++) {
            for (int j = 0; j < map.Height; j++) {
                var coord = new Coord(i, j);
                if (map[coord].Type == MoveType.None)
                    continue;
                var position = CellCoordToPosition(coord);
                min.x = Mathf.Min(min.x, position.x);
                min.y = Mathf.Min(min.y, position.z);
                max.x = Mathf.Max(max.x, position.x);
                max.y = Mathf.Max(max.y, position.z);
            }
        }
        return new Rect(min, max - min);
    }

    private GameObject GetRandomPrefabForType(CellType type) {
        foreach (var preset in cellObjectPresets) {
            if (preset.Type == type)
                return preset.CellObjects[Random.Range(0, preset.CellObjects.Length)];
        }
        return null;
    }

    private void CreateCellObject(Vector3 position, GameObject prefab) {
        if (prefab == null)
            return;
        var cellObject = Instantiate(prefab, transform);
        cellObject.transform.position = position;
        cellObject.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 6) * 60f, 0);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos() {
        if (map == null)
            return;
        var distances = map.Navigation.GetDistanceMap();
        for (int i = 0; i < map.Width; i++) {
            for (int j = 0; j < map.Height; j++) {
                if (distances[i, j] < int.MaxValue) {
                    UnityEditor.Handles.Label(CellCoordToPosition(new Coord(i, j)), distances[i, j].ToString());
                }
            }
        }
    }
#endif
}
