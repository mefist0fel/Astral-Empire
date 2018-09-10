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
        public List<City> Cities = new List<City>();

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
            foreach (var unit in Units)
                unit.OnStartTurn();
            foreach (var city in Cities) {
                city.OnStartTurn();
                Debug.Log("Faction " + Name + " city " + city.Name);
            }
            if (controller != null)
                controller.OnStartTurn(this);
        }

        public void AddCity(City city) {
            Cities.Add(city);
        }

        public void RemoveCity(City city) {
            Cities.Remove(city);
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