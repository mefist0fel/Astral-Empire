using System;
using Model;
using UnityEngine;
using UnityEngine.UI;

public sealed class CityView : MonoBehaviour {
    [SerializeField]
    private Text NameLabel; // Set from editor
    [SerializeField]
    private Text ProjectLabel; // Set from editor
    [SerializeField]
    private Image Flag; // Set from editor

    private City city;

    public void Init(City controlCity, Vector3 position) {
        city = controlCity;
        transform.localPosition = position;
        city.OnSetFaction += OnSetFaction;
        city.OnUpdate += OnUpdate;
        SetCityColor(city.Faction.BaseColor, city.Faction.FactionColor);
        NameLabel.text = city.Name;
        OnUpdate();
    }


    private void OnDestroy() {
        if (city == null)
            return;
        city.OnSetFaction -= OnSetFaction;
    }

    private void OnSetFaction(Faction faction) {
        SetCityColor(faction.BaseColor, faction.FactionColor);
    }

    private void OnUpdate() {
        ProjectLabel.text = city.CurrentProject == null ? "" : city.TurnsToEnd.ToString();
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
        Flag.color = baseColor;
    }

    private void Start () {}
}
