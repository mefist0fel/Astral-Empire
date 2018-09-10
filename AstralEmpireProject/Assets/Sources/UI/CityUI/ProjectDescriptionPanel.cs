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
    private AbstractProject currentProject;

    public void Init(AbstractProject project, Action<AbstractProject> onSelectProjectClickCallback) {
        currentProject = project;
        onSelectProjectClick = onSelectProjectClickCallback;
        ShowProjectParams(project);
    }

    private void ShowProjectParams(AbstractProject project) {
        projectName.text = project.ID;
        projectDescription.text = project.Cost.ToString();
    }

    private void Start() {
        selectProjectButton.onClick.AddListener(OnSelectProjectButtonClick);
    }

    private void OnSelectProjectButtonClick() {
        onSelectProjectClick.InvokeSafe(currentProject);
    }
}
