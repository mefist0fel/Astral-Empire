using System;
using UnityEngine;
using UnityEngine.UI;
using Model;

// panel to show project
public sealed class ProjectDescriptionPanel : MonoBehaviour {
    [SerializeField]
    private Text projectName; // Set from editor
    [SerializeField]
    private Text projectDescription; // Set from editor
    [SerializeField]
    private Button selectProjectButton; // Set from editor

    private event Action<AbstractProject> onSelectProjectClick;
    private AbstractProject project;

    public void Init(AbstractProject currentProject, Action<AbstractProject> onSelectProjectClickCallback) {
        project = currentProject;
        onSelectProjectClick = onSelectProjectClickCallback;
        ShowProjectParams();
    }

    public void ShowProjectParams(int cityProduction = 1) {
        projectName.text = project.ID;
        int turnsToEnd = Mathf.CeilToInt(project.Cost / (float)cityProduction);
        projectDescription.text = string.Format("{0}({1} turns)", project.Cost.ToString(), turnsToEnd);
    }

    private void Start() {
        selectProjectButton.onClick.AddListener(OnSelectProjectButtonClick);
    }

    private void OnSelectProjectButtonClick() {
        onSelectProjectClick.InvokeSafe(project);
    }
}
