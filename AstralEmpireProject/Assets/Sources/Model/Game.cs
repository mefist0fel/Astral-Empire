using System;
using System.Collections.Generic;
using UnityEngine;

namespace Model {
    public sealed class Game {
        public readonly Map Map;
        public readonly Faction[] Factions;
        public readonly Faction NeutralFaction = new Faction(new EmptyFactionController(), Color.gray, Color.white, "Neutral");
        public readonly List<Unit> Units = new List<Unit>();
        public readonly List<City> Cities = new List<City>();

        private readonly IGameController controller;

        private int currentFactionId = 0;
        public Faction CurrentFaction { get { return Factions[currentFactionId]; } }

        public Game(IGameController gameController, Map map, Faction[] factions) {
            controller = gameController;
            Map = map;
            Factions = factions;
            foreach (var faction in factions)
                faction.OnStartGame(this);
            CurrentFaction.OnStartTurn();
        }

        public void CreateUnit(Unit.Data data, Coord position, Faction faction) {
            var unit = new Unit(data);
            unit.Init(Map, faction, position);
            Units.Add(unit);
            faction.Units.Add(unit);
            controller.OnAddUnit(unit);
        }

        public void CreateCity(Coord position, Faction faction = null) {
            faction = faction ?? NeutralFaction;
            var city = new City(Map, faction, position);
            Cities.Add(city);
            controller.OnAddCity(city);
        }

        public void EndTurn() {
            CurrentFaction.OnEndTurn();
            currentFactionId += 1;
            currentFactionId = currentFactionId % Factions.Length;
            CurrentFaction.OnStartTurn();
        }

        public interface IGameController {
            void OnAddUnit(Unit unit);
            void OnRemoveUnit(Unit unit);
            void OnEndTurn(Faction faction);
            void OnStartTurn(Faction faction);
            void OnAddCity(City city);
        }
    }
}