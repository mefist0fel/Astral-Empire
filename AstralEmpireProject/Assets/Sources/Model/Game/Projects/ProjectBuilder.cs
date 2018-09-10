using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model {
    public sealed class ProjectBuilder : City.IProjectBuilder {
        private readonly Game game;
        public readonly AbstractProject[] AvailableProjects = new AbstractProject[] {
            new BuildUnitProject("infantry", 5, new Unit.Data("infantry", 2, 3)),
            new BuildUnitProject("heavy_infantry", 7, new Unit.Data("heavy_infantry", 3, 3)),
            new BuildUnitProject("AIV", 10, new Unit.Data("AIV", 2, 3)),
            new BuildUnitProject("tank", 20, new Unit.Data("tank", 2, 3)),
        };

        public ProjectBuilder(Game controlGame) {
            game = controlGame;
        }

        public void EndProjectBuilding(string projectId, City city) {
            Debug.LogError("City " + city.Name + " ended " + projectId);
        }
    }
}
