using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model {
    public sealed class DeathAction: Map.Action {
        public readonly Unit Unit;
        public readonly Coord Coord;

        public DeathAction(Unit unit, Coord coord) {
            Unit = unit;
            Coord = coord;
        }
    }
}