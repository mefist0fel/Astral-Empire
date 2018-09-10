using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Model {
    public sealed class City {
        public readonly Coord Coordinate;
        private readonly Map map;
        private readonly IProjectBuilder projectBuilder;
        public string Name { get; private set; }
        public Faction Faction { get; private set; }

        public int IndustyProduction { get; private set; }

        public sealed class BuildProject {
            public string ID;
            public int IndustryNeed;
        }

        public BuildProject CurrentProject { get; private set; }

        public readonly Unit Garrison;
        public event Action<Faction> OnSetFaction;

        public City(IProjectBuilder builder, Faction faction, Coord position) {
            projectBuilder = builder;
            Name = "City " + Random.Range(0, 10000);
            SetFaction(faction);
            Coordinate = position;
            Garrison = new Unit(new Unit.Data("garrison", 2, 1));
            IndustyProduction = 1;
        }

        public void SetFaction(Faction faction) {
            if (Faction != null)
                Faction.RemoveCity(this);
            Faction = faction;
            if (Faction != null)
                Faction.AddCity(this);
            OnSetFaction.InvokeSafe(faction);
        }

        public void SetProject(string id, int industryCost) {
            CurrentProject = new BuildProject() { ID = id, IndustryNeed = industryCost };
        }

        public void OnStartTurn () {
            ProcessCurrentProject(IndustyProduction);
        }

        private void ProcessCurrentProject(int industy) {
            if (CurrentProject == null) {
                Debug.LogError("Current project in city " + Name + " is empty");
                return;
            }
            CurrentProject.IndustryNeed -= IndustyProduction;
            if (CurrentProject.IndustryNeed <= 0) {
                projectBuilder.EndProjectBuilding(CurrentProject.ID, this);
                CurrentProject = null;
            }
        }

        public interface IProjectBuilder {
            void EndProjectBuilding(string projectId, City city);
        }
    }
}