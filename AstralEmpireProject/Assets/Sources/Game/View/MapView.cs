using System;
using System.Collections;
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

    [Serializable]
    public sealed class CellPreset {
        public Map.CellType Type = Map.CellType.Land;
        public GameObject CellObject;
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

    public void Init(Map map) {
        mapShift = -CellCoordToPosition(new Coord((map.Width - 1) * 0.5f, (map.Height - 1) * 0.5f));
        for (int i = 0; i < map.Width; i++) {
            for (int j = 0; j < map.Height; j++) {
                var coord = new Coord(i, j);
                var prefab = GetRandomPrefabForType(map[coord].Type);
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
                if (map[coord].Type == Map.CellType.None)
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

    private GameObject GetRandomPrefabForType(Map.CellType type) {
        foreach (var preset in cellObjectPresets) {
            if (preset.Type == type)
                return preset.CellObject;
        }
        return null;
    }

    private void CreateCellObject(Vector3 position, GameObject prefab) {
        if (prefab == null)
            return;
        var cellObject = Instantiate(prefab, transform);
        cellObject.transform.position = position;
        cellObject.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 4) * 90f, 0);
    }
}
