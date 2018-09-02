using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Model {
    public sealed class Faction {
        public interface IController {
            void OnStartGame(Game game);
            void OnStartTurn(Faction faction);
            void OnEndTurn();
            void OnChangeStatus();
        }

        public readonly string Name;
        public readonly Color BaseColor = Color.white;
        public readonly Color FactionColor = Color.blue;
        public List<Unit> Units = new List<Unit>();

        private IController controller = null;

        public int UnitCount { get { return Units.Count(unit => unit != null && unit.IsAlive); }}

        public void OnStartGame(Game game) {
            controller.OnStartGame(game);
        }

        public Faction(IController turnController, Color baseColor, Color factionColor, string name = "") {
            Name = name;
            BaseColor = baseColor;
            FactionColor = factionColor;
            controller = turnController;
        }

        public void OnStartTurn() {
            foreach (var unit in Units) {
                if (unit == null)
                    continue;
                if (unit.IsAlive)
                    unit.OnStartTurn();
            }
            if (controller != null) {
                controller.OnStartTurn(this);
            }
        }

        public void OnChangeStatus() {
        }

        public void OnEndTurn() {
            if (controller != null) {
                controller.OnEndTurn();
            }
        }
    }

}