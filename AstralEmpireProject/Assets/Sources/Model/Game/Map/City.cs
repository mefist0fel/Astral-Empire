using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Model {
    public sealed class City {
        private readonly Map map;
        public readonly Coord Coordinate;
        public string Name { get; private set; }
        public Faction Faction { get; private set; }
        public event Action<Faction> OnSetFaction;

        public City(Map controlMap, Faction faction, Coord position) {
            map = controlMap;
            Name = "City " + Random.Range(0, 10000);
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
