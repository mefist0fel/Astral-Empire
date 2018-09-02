using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model {
    public sealed class City {
        private readonly Map map;
        public readonly Coord Coordinate;

        public City(Map controlMap, Coord position) {
            map = controlMap;
            Coordinate = position;
            map = controlMap;
            map[Coordinate].City = this; // add Set unit action
        }
    }
}
