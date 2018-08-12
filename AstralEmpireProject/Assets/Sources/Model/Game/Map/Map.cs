using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random; // TODO remove me

namespace Model {
    /// <summary>
    /// Map. Base class to store cell structure and manage paht finding and fire range
    /// </summary>
    public class Map {
        public abstract class Action { }

        private const int defaultSize = 21;
        private readonly int widht;
        private readonly int height;

        public int Widht { get { return widht; } }
        public int Height { get { return height; } }

        public Cell[,] cells = null;
        public List<Action> actions = new List<Action>();

        public enum CellType {
            None = 0, // nothing - end of map move zone
            Land = 1, // usual units
            Water = 2, // ships
            Rough = 3, // forest and hills
            Mountains = 4 // No ships can move - large objects
        }

        public class Cell {
            public Unit unit = null;
            public CellType type = CellType.None;

            public Unit GetUnit() {
                if (unit != null && unit.IsAlive)
                    return unit;
                return null;
            }
        }

        public Cell this[int x, int y] {
            get {
                if (x >= 0 && y >= 0 && x < widht && y < height) {
                    return cells[x, y];
                }
                return new Cell();
            }
            set {
                if (x >= 0 && y >= 0 && x < widht && y < height) {
                    cells[x, y] = value;
                }
            }
        }

        public Cell this[Coord coord] {
            get { return this[coord.x, coord.y]; }
            set { this[coord.x, coord.y] = value; }
        }

        public void MoveUnit(Coord from, Coord to, MarkersSet moveMarkers) {
            if (from == to) {
                return;
            }
            if (this[from].unit != null && this[to].unit == null) {
                var path = TryFindPath(from, to, moveMarkers); // TODO find separately for unit
                this[to].unit = this[from].unit;
                this[from].unit = null;
                this[to].unit.MoveTo(this, to);
                AddAction(new MoveAction(this[to].unit, from, to, path));
            }
#if UNITY_EDITOR
        else {
                Debug.LogError("move unit error - unit not found or place is not empty");
            }
#endif
        }

        public void KillUnit(Coord coord) {
            if (this[coord].unit != null) {
                AddAction(new DeathAction(this[coord].unit, coord));
                this[coord].unit = null;
            }
#if UNITY_EDITOR
        else {
                Debug.LogError("try kill unit - but cell is empty");
            }
#endif
        }
        // TODO remove
        public Coord GetRandomCoord(CellType startType = CellType.Land, Coord from = new Coord(), Coord to = new Coord()) {
            if (from == Coord.Zero) {
                from = new Coord(1, 1);
            }
            if (to == Coord.Zero) {
                to = new Coord(height - 1, height - 1);
            }
            for (int i = 0; i < 50; i++) {
                Coord coord = new Coord(Random.Range(from.x, to.x), Random.Range(from.y, to.y));
                if (this[coord].unit == null && this[coord].type == startType) {
                    return coord;
                }
            }
            return new Coord();
        }

        // -1: 1  --  0: 1  --  1: 1
        // 	 |         |         |
        // -1: 0  --  0: 0  --  1: 0 
        //   |         |         |
        // -1:-1  --  0:-1  --  1:-1

        public static readonly Coord[] Neigbhors = new Coord[] {
            new Coord( 1, 0),
            new Coord( 1,-1),
            new Coord( 0,-1),
            new Coord(-1, 0),
            new Coord(-1, 1),
            new Coord( 0, 1),
        };

        public Map(int mapWidht = defaultSize, int mapHeight = defaultSize) {
            widht = mapWidht;
            height = mapHeight;
            cells = GenerateCells(widht, height);
        }

        private Cell[,] GenerateCells(int widht, int height) {
            var cells = new Cell[widht, height];
            for (int i = 0; i < widht; i++) {
                for (int j = 0; j < height; j++) {
                    cells[i, j] = new Cell();
                }
            }
            for (int i = 2; i < widht - 2; i++) {
                for (int j = 2; j < height - 2; j++) {
                    if (i + j - 1 <= widht * 0.5f || i + j + 1 >= widht + height - widht * 0.5f - 2) {
                        continue;
                    }
                    cells[i, j].type = CellType.Land;
                }
            }
            for (int i = 2; i < widht - 2; i++) {
                for (int j = 2; j < height - 2; j++) {
                    if (i + j - 1 <= widht * 0.5f || i + j + 1 >= widht + height - widht * 0.5f - 2) {
                        continue;
                    }
                    cells[i, j].type = CellType.Land;
                    if (Random.Range(0, 16) == 0) {
                        cells[i, j].type = CellType.Rough;
                    }
                    if (Random.Range(0, 6) == 0) {
                        cells[i, j].type = CellType.Mountains;
                    }
                }
            }
            return cells;
        }

        public void AddAction(Action action) {
            actions.Add(action);
        }

        public void AttackUnit(Unit unit, Unit attackedUnit) {
            unit.AttackUnit(attackedUnit);
        }

        public MarkersSet GetFireZone(Unit unit) {
            var fireMarkers = new MarkersSet();
            MarkFireZoneRecursive(fireMarkers, unit.Coordinate, unit.maxFireRange + 1);
            ClearNearFireZoneRecursive(fireMarkers, unit.Coordinate, unit.minFireRange);
            MarkEnemyUnitsOnFireZone(fireMarkers, unit);
            return fireMarkers;
        }

        public MarkersSet GetMoveZone(Unit unit) {
            var moveMarkers = new MarkersSet();
            MarkMoveZoneRecursive(moveMarkers, unit.Coordinate, unit.moveTerrainMask, unit.moveDistance + 1, unit.faction);
            return moveMarkers;
        }

        private void MarkFireZoneRecursive(MarkersSet markers, Coord startPoint, int fireDistance) { // todo change
            if (fireDistance <= markers[startPoint])
                return;
            markers[startPoint] = fireDistance;
            for (int i = 0; i < Neigbhors.Length; i++) {
                MarkFireZoneRecursive(markers, startPoint + Neigbhors[i], fireDistance - 1);
            }
        }

        private void ClearNearFireZoneRecursive(MarkersSet markers, Coord startPoint, int clearDistance) { // todo change
            if (markers[startPoint] == 0)
                return;
            if (clearDistance <= 0)
                return;
            markers[startPoint] = 0;
            for (int i = 0; i < Neigbhors.Length; i++) {
                ClearNearFireZoneRecursive(markers, startPoint + Neigbhors[i], clearDistance - 1);
            }
        }

        private void MarkEnemyUnitsOnFireZone(MarkersSet fireMarkers, Unit unit) {
            List<Coord> fireZoneMarkers = fireMarkers.GetCoordList();
            foreach (var markerCoord in fireZoneMarkers) {
                bool isEnemyUnit = (this[markerCoord].unit != null && this[markerCoord].unit.IsAlive && this[markerCoord].unit.faction != unit.faction);
                fireMarkers[markerCoord] = isEnemyUnit ? 1 : 0;
            }
        }

        private void MarkMoveZoneRecursive(MarkersSet moveMarkers, Coord startPoint, CellType[] accessibilityMask, int distance, Faction team) {
            if (distance <= 0)
                return;
            bool canMove = false;
            for (int i = 0; i < accessibilityMask.Length; i++) {
                if (accessibilityMask[i] == this[startPoint].type) {
                    canMove = true;
                    break;
                }
            }
            if (!canMove)
                return;
            if (this[startPoint].unit != null && team != this[startPoint].unit.faction)
                return;
            if (distance <= moveMarkers[startPoint])
                return;
            moveMarkers[startPoint] = distance;
            for (int i = 0; i < Neigbhors.Length; i++) {
                MarkMoveZoneRecursive(moveMarkers, startPoint + Neigbhors[i], accessibilityMask, distance - 1, team);
            }
        }

        public MarkersSet GetFireZoneForMoveZone(Unit unit, MarkersSet moveZone = null) {
            if (moveZone == null) {
                moveZone = GetMoveZone(unit);
            }
            MarkersSet fullFireZone = new MarkersSet();
            var moveCoords = moveZone.GetCoordList();
            foreach (var coord in moveCoords) {
                if (this[coord].unit == null) {
                    MarkersSet localFireMarkers = new MarkersSet();
                    MarkFireZoneRecursive(localFireMarkers, coord, unit.maxFireRange + 1);
                    ClearNearFireZoneRecursive(localFireMarkers, coord, unit.minFireRange);
                    MarkEnemyUnitsOnFireZone(localFireMarkers, unit);
                    fullFireZone.Add(localFireMarkers);
                }
            }
            return fullFireZone;
        }

        public bool IsCanFireFromCoord(Unit unit, Coord coord, Coord fireCoord) {
            var fireMarkers = new MarkersSet();
            MarkFireZoneRecursive(fireMarkers, coord, unit.maxFireRange + 1);
            ClearNearFireZoneRecursive(fireMarkers, coord, unit.minFireRange);
            bool canFire = (fireMarkers[fireCoord] > 0);
            return canFire;
        }
        internal Coord FindOptimalMovePoint(Unit unit, MarkersSet moveMarkers, Coord fireCoord) {
            // TODO change to IEnumerator
            var moveCoords = moveMarkers.GetCoordList();
            int bestMarker = 0;
            Coord nearestCoord = Coord.Zero;
            foreach (var moveCoord in moveCoords) {
                if (moveMarkers[moveCoord] > bestMarker && (this[moveCoord].unit == unit || this[moveCoord].unit == null)) {
                    var fireMarkers = new MarkersSet();
                    MarkFireZoneRecursive(fireMarkers, moveCoord, unit.maxFireRange + 1);
                    ClearNearFireZoneRecursive(fireMarkers, moveCoord, unit.minFireRange);
                    if (fireMarkers[fireCoord] > 0) {
                        nearestCoord = moveCoord;
                        bestMarker = moveMarkers[moveCoord];
                    }
                }
            }
            if (nearestCoord == Coord.Zero) {
                Debug.LogError("Can't find nearest fire point");
            }
            return nearestCoord;
        }
        public List<Coord> TryFindPath(Coord from, Coord to, MarkersSet moveMarkers) {
            List<Coord> pathPoints = new List<Coord>();
            if (moveMarkers[to] > 0) {
                pathPoints.Add(to);
                Coord current = to;
                const int maxSearchDeep = 100;
                int deep = 0;
                while (deep < maxSearchDeep) {
                    for (int i = 0; i < Neigbhors.Length; i++) {
                        if (moveMarkers[current + Neigbhors[i]] > moveMarkers[current]) {
                            current = current + Neigbhors[i];
                            pathPoints.Add(current);
                            break;
                        }
                    }
                    if (current == from) {
                        break;
                    }
                    deep += 1;
                }
                if (pathPoints.Count >= 2) {
                    pathPoints.Reverse();
                    return pathPoints;
                }
            }
            return new List<Coord>() { from, to };
        }
    }
}