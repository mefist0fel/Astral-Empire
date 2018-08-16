using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model {
    public sealed class MoveAction : Map.AbstractAction {
        public readonly Unit Unit;
        public readonly Coord FromCoord;
        public readonly Coord ToCoord;
        public readonly List<Coord> Path; // move to controller only

        public MoveAction(Unit unit, Coord from, Coord to, List<Coord> path = null) {
            Unit = unit;
            FromCoord = from;
            ToCoord = to;
            Path = path;
        }
    }
}