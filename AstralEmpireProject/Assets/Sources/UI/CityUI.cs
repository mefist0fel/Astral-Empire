using Model;
using UnityEngine;
using UnityEngine.UI;

public sealed class CityUI : UILayer {
    [SerializeField]
    private Button closeButton; // Set from editor
    [SerializeField]
    private Text cityNameLabel; // Set from editor
    [SerializeField]
    private Text projectLabel; // Set from editor

    private City city;

    public static void ShowCityUI(City city) {
        var cityUI = UILayer.Show<CityUI>();
        cityUI.Init(city);
    }

    private void Init(City controlCity) {
        city = controlCity;
        UpdateText();
    }

    private void UpdateText() {
        cityNameLabel.text = city.Name;
        var project = city.CurrentProject;
        if (project == null) {
            projectLabel.text = "Current project: none";
        } else {
            projectLabel.text = "Current project:\n" + project.ID + " in " + Mathf.CeilToInt(project.IndustryNeed / (float)city.IndustyProduction);
        }
    }

    public void OnCloseUIClick() { // set from editor
        Hide<CityUI>();
        Show<GameUI>();
    }

    private void Start() {
        closeButton.onClick.AddListener(OnCloseUIClick);
    }
}
