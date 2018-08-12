using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model {
    public class MarkersSet {

        private Dictionary<Coord, int> elements = new Dictionary<Coord, int>();

        public int this[int x, int y] {
            get { return GetElement(new Coord(x, y)); }
            set { SetElement(new Coord(x, y), value); }
        }

        public int this[Coord coord] {
            get { return GetElement(coord); }
            set { SetElement(coord, value); }
        }

        public void Clear() {
            elements.Clear();
        }

        public List<Coord> GetCoordList() {
            List<Coord> list = new List<Coord>();
            foreach (var marker in elements) {
                if (marker.Value > 0) {
                    list.Add(marker.Key);
                }
            }
            return list;
        }

        private void SetElement(Coord coord, int value) {
            if (value == 0) {
                if (elements.ContainsKey(coord))
                    elements.Remove(coord);
                return;
            }
            if (elements.ContainsKey(coord)) {
                elements[coord] = value;
            } else {
                elements.Add(coord, value);
            }
        }

        private int GetElement(Coord coord) {
            if (elements.ContainsKey(coord)) {
                return elements[coord];
            }
            return 0;
        }

        internal void Add(MarkersSet markers) {
            foreach (var marker in markers.elements) {
                if (marker.Value > 0) {
                    if (!elements.ContainsKey(marker.Key)) {
                        elements.Add(marker.Key, marker.Value);
                    }
                }
            }
        }
    }
}
