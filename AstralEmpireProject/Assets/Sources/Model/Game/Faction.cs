using UnityEngine;

namespace Model {
    public sealed class Faction {
        public interface IController {
            void OnStartTurn(Faction faction);
            void OnEndTurn();
            void OnChangeStatus();
        }

        public readonly int SideId = 0;
        public readonly string Name;
        public readonly Color BaseColor = Color.white;
        public readonly Color FactionColor = Color.blue;
        public Unit[] Units = null;

        private IController turnController = null;

        public int UnitCount {
            get {
                int count = 0;
                if (Units != null) {
                    for (int i = 0; i < Units.Length; i++) {
                        if (Units[i] != null && Units[i].IsAlive) {
                            count += 1;
                        }
                    }
                }
                return count;
            }
        }

        public Faction(IController turnController, int sideId, Color baseColor, Color factionColor, string name = "") {
            this.Name = name;
            this.BaseColor = baseColor;
            this.FactionColor = factionColor;
            this.SideId = sideId;
            this.turnController = turnController;
        }

        public void OnStartTurn() {
            if (Units != null) {
                for (int i = 0; i < Units.Length; i++) {
                    if (Units[i] != null) {
                        Units[i].OnStartTurn();
                    }
                }
            }
            if (turnController != null) {
                turnController.OnStartTurn(this);
            }
        }

        public void OnChangeStatus() {
        }

        public void OnEndTurn() {
            if (turnController != null) {
                turnController.OnEndTurn();
            }
        }

        public void SetController(IController turnController) {
            this.turnController = turnController;
        }
    }

}