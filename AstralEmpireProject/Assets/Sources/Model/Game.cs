using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model {
    public sealed class Game {
        public readonly Map Map;

        private readonly List<Unit> Units = new List<Unit>();

        public Game(Map map) {
            Map = map;
        }
    }
}