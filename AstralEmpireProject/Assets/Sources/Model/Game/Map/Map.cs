using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random; // TODO remove me
using System.Linq;
using Model.PathFind;

namespace Model {
    /// <summary>
    /// Map. Base class to store cell structure and manage paht finding and fire range
    /// </summary>
    public sealed class Map {
        public abstract class AbstractAction { }

        private readonly int width;
        private readonly int height;

        public int Width { get { return width; } }
        public int Height { get { return height; } }

        public Cell[,] cells = null;
        public List<AbstractAction> actions = new List<AbstractAction>();
        public readonly Navigation Navigation;
        public event System.Action<AbstractAction> OnAction;

        public Cell this[int x, int y] {
            get {
                if (x >= 0 && y >= 0 && x < width && y < height) {
                    return cells[x, y];
                }
                return new Cell();
            }
            set {
                if (x >= 0 && y >= 0 && x < width && y < height) {
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
            if (this[from].Unit != null && this[to].Unit == null) {
                var path = TryFindPath(from, to, moveMarkers); // TODO find separately for unit
                var actionPoints = this[from].Unit.ActionPoints - moveMarkers[to];
                this[to].Unit = this[from].Unit;
                this[from].Unit = null;
                this[to].Unit.MoveTo(to, actionPoints);
                AddAction(new MoveAction(this[to].Unit, from, to, actionPoints, path));
            }
#if UNITY_EDITOR
            else {
                Debug.LogError("move unit error - unit not found or place is not empty");
            }
#endif
        }

        public void KillUnit(Coord coord) {
            if (this[coord].Unit != null) {
                AddAction(new DeathAction(this[coord].Unit, coord));
                this[coord].Unit = null;
            }
#if UNITY_EDITOR
        else {
                Debug.LogError("try kill unit - but cell is empty");
            }
#endif
        }

        // TODO remove
        public Coord GetRandomCoord(MoveType startType = MoveType.Land, Coord from = new Coord(), Coord to = new Coord()) {
            if (from == Coord.Zero) {
                from = new Coord(1, 1);
            }
            if (to == Coord.Zero) {
                to = new Coord(height - 1, height - 1);
            }
            for (int i = 0; i < 50; i++) {
                Coord coord = new Coord(Random.Range(from.x, to.x), Random.Range(from.y, to.y));
                if (this[coord].Unit == null && this[coord].Type == startType) {
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

        public Map(Cell[,] mapCells) {
            width = mapCells.GetLength(0);
            height = mapCells.GetLength(1);
            if (mapCells == null || width == 0 || height == 0)
                throw new FormatException("need a valid two dimentional array of map cells");
            cells = mapCells;
            Navigation = new Navigation(this);
        }

        public void AddAction(AbstractAction action) {
            actions.Add(action);
            OnAction.InvokeSafe(action);
        }

        public void AttackUnit(Unit unit, Unit attackedUnit) {
            unit.AttackUnit(attackedUnit);
        }

        public MarkersSet GetMoveZone(Unit unit) {
            return Navigation.GetMoveZone(unit);
        }

        public List<Coord> TryFindPath(Coord from, Coord to, MarkersSet moveMarkers) {
            return Navigation.TryFindPath(from, to, moveMarkers);
        }

        public List<Coord> FindPath(Unit unit, Coord endCoord) {
            return Navigation.FindPathAStar(unit.Coordinate, endCoord, unit);
        }

        public MarkersSet GetFireZoneForMoveZone(Unit unit, MarkersSet moveZone = null) {
            if (moveZone == null) {
                moveZone = GetMoveZone(unit);
            }
            MarkersSet fullFireZone = new MarkersSet();
            fullFireZone.Add(GetFireZoneFromCoord(unit.Coordinate, unit));
            var moveCoords = moveZone.GetCoordList();
            int availableActionPoints;
            foreach (var coord in moveCoords) {
                if (this[coord].Unit != null)
                    continue;
                availableActionPoints = moveZone[coord];
                if (availableActionPoints > 0)
                    fullFireZone.Add(GetFireZoneFromCoord(coord, unit));
            }
            return fullFireZone;
        }

        private MarkersSet GetFireZoneFromCoord(Coord coord, Unit unit) {
            MarkersSet fireMarkers = new MarkersSet();
            for (int i = -unit.MaxFireRange; i <= unit.MaxFireRange; i++) {
                for (int j = -unit.MaxFireRange; j <= unit.MaxFireRange; j++) {
                    var distance = CubeDistance(i, j);
                    var distanceIsActual = unit.MinFireRange <= distance && distance <= unit.MaxFireRange;
                    var currentCoord = new Coord(i, j) + coord;
                    var hasEnemyUnit = this[currentCoord].HasEnemyUnit(unit);
                    if (distanceIsActual && hasEnemyUnit)
                        fireMarkers[currentCoord] = 1;
                }
            }
            return fireMarkers;
        }

        private int CubeDistance(int x, int y) {
            return Math.Max(
                Mathf.Abs(-x - y),
                Math.Max(
                    Mathf.Abs(x),
                    Mathf.Abs(y)));
        }

        public bool IsCanFireFromCoord(Unit unit, Coord coord, Coord fireCoord) {
            var delta = coord - fireCoord;
            var distance = CubeDistance(delta.x, delta.y);
            return unit.MinFireRange <= distance && distance <= unit.MaxFireRange;
        }

        public Coord FindOptimalMovePoint(Unit unit, MarkersSet moveMarkers, Coord fireCoord) {
            var moveCoords = moveMarkers.GetCoordList();
            int bestMarker = 0;
            Coord nearestCoord = Coord.Zero;
            foreach (var moveCoord in moveCoords) {
                if (moveMarkers[moveCoord] > bestMarker && (this[moveCoord].Unit == unit || this[moveCoord].Unit == null)) {
                    if (IsCanFireFromCoord(unit, moveCoord, fireCoord)) {
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
    }
}