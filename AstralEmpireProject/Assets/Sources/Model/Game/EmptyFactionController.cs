using System;

namespace Model {
    public sealed class EmptyFactionController : Faction.IController {
        private Game game;

        public void OnStartGame(Game game) {
            this.game = game;
        }

        public void OnChangeStatus() {}

        public void OnEndTurn() {}

        public void OnStartTurn(Faction faction) {
            game.EndTurn();
        }
    }
}
