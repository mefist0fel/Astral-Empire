using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model {
    public sealed class ProjectBuilder : City.IProjectBuilder {
        private readonly Game game;
        public readonly AbstractProject[] Projects;

        private readonly Dictionary<string, AbstractProject> indexedProjects = new Dictionary<string, AbstractProject>();

        public ProjectBuilder(AbstractProject[] availableProjects) {
            Projects = availableProjects;
            foreach (var project in Projects)
                indexedProjects.Add(project.ID, project);
        }

        public void EndProjectBuilding(string projectId, City city) {
            if (!indexedProjects.ContainsKey(projectId)) {
                Debug.LogError("Project " + projectId + " is not exist in list of available projects");
                return;
            }
            var project = indexedProjects[projectId];
            project.EndBuilding(city);
        }
    }
}
