using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model {
    public sealed class ProjectBuilder : City.IProjectBuilder {
        private readonly Game game;
        public readonly AbstractProgect[] AvailableProjects = new AbstractProgect[] {
        };

        public ProjectBuilder(Game controlGame) {
            game = controlGame;
        }

        public void EndProjectBuilding(string projectId, City city) {
            Debug.LogError("City " + city.Name + " ended " + projectId);
        }
    }
}
