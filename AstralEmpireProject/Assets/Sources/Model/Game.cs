﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model {
    public sealed class Game {
        public readonly Map Map;
        public readonly Faction[] Factions;
        private readonly List<Unit> Units = new List<Unit>();

        public event Action<Unit> OnAddUnit;

        public Game(Map map, Faction[] factions) {
            Map = map;
            Factions = factions;
        }

        public void CreateUnit(Faction faction, Coord position) {
            var unit = new Unit("test", 10, 2);
            unit.Init(Map, faction, position);
            OnAddUnit(unit);
        }
    }
}