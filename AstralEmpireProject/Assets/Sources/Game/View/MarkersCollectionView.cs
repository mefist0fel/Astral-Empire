using System.Collections.Generic;
using UnityEngine;

public sealed class MarkersCollectionView : MonoBehaviour {
    [SerializeField]
    private GameObject moveMarkerPrefab; // Set from editor

    private List<GameObject> markersCache = new List<GameObject>();

	private void Start () {}

    public void Show(List<Vector3> list = null) {
        int count = list == null ? 0 : list.Count;
        if (count > markersCache.Count)
            GenerateMarkers(count - markersCache.Count);
        for (int i = 0; i < markersCache.Count; i++) {
            bool needShow = i < count;
            markersCache[i].SetActive(needShow);
            if (needShow)
                markersCache[i].transform.localPosition = list[i];
        }
    }

    public void Hide() {
        Show();
    }

    private void GenerateMarkers(int count) {
        for (int i = 0; i < count; i++) {
            var marker = Instantiate(moveMarkerPrefab, transform);
            markersCache.Add(marker);
        }
    }
}
