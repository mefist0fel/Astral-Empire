using System.Collections.Generic;
using System.Linq;

namespace Model {
    public sealed class MarkersSet {
        public const int Empty = -1;

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
            return elements.Keys.ToList();
        }

        private void SetElement(Coord coord, int value) {
            if (value <= Empty) {
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
            return Empty;
        }

        internal void Add(MarkersSet markers) {
            foreach (var marker in markers.elements) {
                if (marker.Value > Empty) {
                    if (!elements.ContainsKey(marker.Key)) {
                        elements.Add(marker.Key, marker.Value);
                    }
                }
            }
        }
    }
}
