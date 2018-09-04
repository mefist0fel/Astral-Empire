using System;
using Model;
using UnityEngine;

public sealed class CityView : MonoBehaviour {
    private City city;

    public void Init(City controlCity, Vector3 position) {
        city = controlCity;
        transform.localPosition = position;
        SetCityColor(city.Faction.BaseColor, city.Faction.FactionColor);
        city.OnSetFaction += OnSetFaction;
    }

    private void OnDestroy() {
        if (city == null)
            return;
        city.OnSetFaction -= OnSetFaction;
    }

    private void OnSetFaction(Faction faction) {
        SetCityColor(faction.BaseColor, faction.FactionColor);
    }

    private void SetCityColor(Color baseColor, Color factionColor) {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++) {
            TextMesh mesh = renderers[i].GetComponent<TextMesh>();
            if (mesh == null) {
                renderers[i].material.color = factionColor; // TODO - optimize - save materials with similar color
                renderers[i].material.SetColor("_Color", baseColor);
                renderers[i].material.SetColor("_BaseColor", baseColor);
                renderers[i].material.SetColor("_FactionColor", factionColor);
            }
        }
    }

    private void Start () {}
}
