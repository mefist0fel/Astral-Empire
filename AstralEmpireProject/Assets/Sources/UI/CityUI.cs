using System;
using Model;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public sealed class CityUI : UILayer {
    [SerializeField]
    private Button closeButton; // Set from editor
    [SerializeField]
    private Text cityNameLabel; // Set from editor
    [SerializeField]
    private Text projectLabel; // Set from editor
    [SerializeField]
    private ProjectDescriptionPanel projectPanelPrefab; // Set from editor
    [SerializeField]
    private Transform projectsParent; // Set from editor

    private ProjectDescriptionPanel[] projectPanels;
    private Game game;
    private City city;

    public static void ShowCityUI(City city, Game game) {
        var cityUI = UILayer.Show<CityUI>();
        cityUI.Init(city, game);
    }

    private void Init(City controlCity, Game controlGame) {
        city = controlCity;
        UpdateText();
        game = controlGame;
        if (projectPanels == null)
            projectPanels = CreateProjectButtons(game.ProjectBuilder.Projects);
        foreach (var projectPanel in projectPanels)
            projectPanel.ShowProjectParams(city.IndustyProduction);
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

    private ProjectDescriptionPanel[] CreateProjectButtons(AbstractProject[] projects) {
        var projectPanelsList = new List<ProjectDescriptionPanel>();
        foreach (var project in projects) {
            projectPanelsList.Add(CreateProjectButton(project));
        }
        return projectPanelsList.ToArray();
    }

    private ProjectDescriptionPanel CreateProjectButton(AbstractProject project) {
        var newButton = Instantiate(projectPanelPrefab, projectsParent);
        newButton.gameObject.SetActive(true);
        newButton.Init(project, OnSelectProject);
        return newButton;
    }

    private void OnSelectProject(AbstractProject project) {
        city.SetProject(project.ID, project.Cost);
        UpdateText();
    }
}
