using System.Collections.Generic;

namespace Model {
    public sealed class Game {
        public readonly Map Map;
        public readonly Faction[] Factions;
        private readonly List<Unit> Units = new List<Unit>();
        private readonly IGameController controller;

        private int currentFactionId = 0;
        public Faction CurrentFaction { get { return Factions[currentFactionId]; } }

        public Game(IGameController gameController, Map map, Faction[] factions) {
            controller = gameController;
            Map = map;
            Factions = factions;
            CurrentFaction.OnStartTurn();
        }

        public void CreateUnit(Faction faction, Coord position) {
            var unit = new Unit("test", 10, 3);
            unit.Init(Map, faction, position);
            Units.Add(unit);
            faction.Units.Add(unit);
            controller.OnAddUnit(unit);
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
        }
    }
}