using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model {
    public sealed class City {
        private readonly Map map;
        public readonly Coord Coordinate;
        public Faction Faction { get; private set; }
        public event Action<Faction> OnSetFaction;

        public City(Map controlMap, Faction faction, Coord position) {
            map = controlMap;
            Faction = faction;
            Coordinate = position;
            map = controlMap;
            map[Coordinate].City = this; // add Set city action
        }

        public void SetFaction(Faction faction) {
            Faction = faction;
            OnSetFaction.InvokeSafe(faction);
        }
    }
}
